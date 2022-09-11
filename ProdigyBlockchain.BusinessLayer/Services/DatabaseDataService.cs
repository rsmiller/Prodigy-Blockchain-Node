using Prodigy.BusinessLayer.Blockchain;
using Prodigy.BusinessLayer.Models.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

using System.Threading.Tasks;
using Newtonsoft.Json;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.S3.Model;
using Prodigy.BusinessLayer.Models.Dto;
using System.Text;

namespace Prodigy.BusinessLayer.Services
{
    public interface IDatabaseDataService
    {
        Task<bool> CreateBlock(DocumentBlock block);
        Task<DocumentBlock> GetBlock(Guid block_id);
        Task<BlockDto> GetTransferBlock(Guid block_id);
        Task<DocumentBlock> GetBlock(int index);
        bool FullTestDatabase();
        int GetBlockSize();
    }

    public class DatabaseDataService : IDatabaseDataService
    {
        private IBlockchainContext _IContext;
        private INodeConfig _NodeConfig;
        private CryptoService _CryptoService;
        private string _DatabasePath = "";

        public DatabaseDataService(INodeConfig nodeConfig, IBlockchainContext context, string database_path, string privateKey)
        {
            _IContext = context;
            _NodeConfig = nodeConfig;

            _CryptoService = new CryptoService(privateKey);

            if (!Directory.Exists(database_path))
                throw new Exception("Database directory doesn't exist");
                
            _DatabasePath = database_path;
        }

        public bool FullTestDatabase()
        {
            var now = DateTime.Now;

            _IContext.Blocks.Add(new BlockRecord()
            {
                block_id = Guid.NewGuid(),
                index = 1728373434,
                block_size = 1728373434,
                created_on = DateTime.Now,
                transaction_count = 1728373434,
                mined_on = DateTime.FromFileTimeUtc(now.ToFileTime()),
                sum_of_payments = 1728373434
            });

            _IContext.Blocks.Add(new BlockRecord()
            {
                block_id = Guid.NewGuid(),
                index = 1728373435,
                block_size = 1728373434, // kilobytes
                created_on = DateTime.Now,
                transaction_count = 1728373434,
                mined_on = DateTime.FromFileTimeUtc(now.ToFileTime()),
                sum_of_payments = 1728373434
            });

            _IContext.SaveChanges();

            var results = _IContext.Blocks.ToList();

            Console.WriteLine("DB Result: " + results.Count);

            var block = _IContext.Blocks.Where(m => m.index == 1728373434).SingleOrDefault();

            Console.WriteLine("DB Result: " + block.index);

            var date_results = _IContext.Blocks.Where(m => m.mined_on <= DateTime.UtcNow.AddMinutes(10)).ToList();

            Console.WriteLine("DB Result: " + date_results.Count);

            if (results.Count != 2 || block == null || date_results.Count != 2)
                return false;

            return true;
        }

        public async Task<bool> CreateBlock(DocumentBlock block)
        {
            var file_name = block.BlockId.ToString() + ".blk";
            var file_path = _DatabasePath + block.BlockId.ToString() + ".blk";

            var json = JsonConvert.SerializeObject(block);

            string encrypted_data = "";
            
            if(_NodeConfig.encrypt_files == false)
                encrypted_data = json;
            else
                encrypted_data = _CryptoService.EncryptData(json, block.BlockId.ToString());

            if (_NodeConfig.document_db_type.ToLower() == "file")
            {
                if (File.Exists(file_path))
                    return false;

                await File.WriteAllTextAsync(file_path, encrypted_data);

                var info = new FileInfo(file_path);

                var sum_of_payments = block.TransactionList.Where(m => Guid.TryParse(m.from, out _) == true && m.document_block_id != "").Sum(m => m.amount);

                await _IContext.Blocks.AddAsync(new BlockRecord()
                {
                    block_id = block.BlockId,
                    index = block.Index,
                    block_size = (info.Length / 1024), // kilobytes
                    created_on = DateTime.Now,
                    transaction_count = block.TransactionList.Count,
                    mined_on = DateTime.FromFileTimeUtc(block.MinedOn),
                    sum_of_payments = sum_of_payments
                });

                await _IContext.SaveChangesAsync();

                
            }
            else if (_NodeConfig.document_db_type.ToLower() == "s3")
            {
                AmazonS3Config config = new AmazonS3Config();
                config.ServiceURL = _NodeConfig.s3_service_url;

                using (var s3Client = new AmazonS3Client(_NodeConfig.s3_service_access_key, _NodeConfig.s3_service_secret, config))
                {
                    using (var tu = new TransferUtility(s3Client))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var streamWRiter = new StreamWriter(memoryStream))
                            {
                                await streamWRiter.WriteAsync(encrypted_data);
                                await streamWRiter.FlushAsync();

                                memoryStream.Position = 0;

                                long size = memoryStream.Length;

                                await tu.UploadAsync(memoryStream, _NodeConfig.s3_service_bucket, file_name);

                                var sum_of_payments = block.TransactionList.Where(m => Guid.TryParse(m.from, out _) == true && m.document_block_id != "").Sum(m => m.amount);

                                await _IContext.Blocks.AddAsync(new BlockRecord()
                                {
                                    block_id = block.BlockId,
                                    index = block.Index,
                                    block_size = (size / 1024), // kilobytes
                                    created_on = DateTime.Now,
                                    transaction_count = block.TransactionList.Count,
                                    mined_on = DateTime.FromFileTimeUtc(block.MinedOn),
                                    sum_of_payments = sum_of_payments
                                });

                                await _IContext.SaveChangesAsync();
                            }
                        }
                    }
                }
            }
            else
            {
                throw new Exception("This document database type is not supported.");
            }

            return true;
        }

        public async Task<BlockDto> GetTransferBlock(Guid block_id)
        {
            var block = await GetBlock(block_id);

            return ConvertToDto(block);
        }

        public async Task<DocumentBlock> GetBlock(Guid block_id)
        {
            try
            {
                var file_path = _DatabasePath + block_id.ToString() + ".blk";
                var file_name = block_id.ToString() + ".blk";

                if (_NodeConfig.document_db_type.ToLower() == "file")
                {
                    if (!File.Exists(file_path))
                        return null;

                    var encrypted_data = File.ReadAllText(file_path);

                    var decrypted_data = "";
                    if (_NodeConfig.encrypt_files == false)
                        decrypted_data = encrypted_data;
                    else
                        decrypted_data = _CryptoService.DecryptData(encrypted_data, block_id.ToString());

                    return JsonConvert.DeserializeObject<DocumentBlock>(decrypted_data);
                }
                else if (_NodeConfig.document_db_type.ToLower() == "s3")
                {
                    AmazonS3Config config = new AmazonS3Config();
                    config.ServiceURL = _NodeConfig.s3_service_url;

                    using (var s3Client = new AmazonS3Client(_NodeConfig.s3_service_access_key, _NodeConfig.s3_service_secret, config))
                    {
                        var request = new GetObjectRequest();
                        request.BucketName = _NodeConfig.s3_service_bucket;
                        request.Key = file_name;

                        var response = await s3Client.GetObjectAsync(request);

                        using (var memoryStream = new MemoryStream())
                        {
                            using (Stream responseStream = response.ResponseStream)
                            {
                                responseStream.CopyTo(memoryStream);
                                await responseStream.FlushAsync();

                                memoryStream.Position = 0;

                                using (var reader = new StreamReader(memoryStream))
                                {
                                    var encrypted_data = reader.ReadToEnd();

                                    var decrypted_data = "";
                                    if (_NodeConfig.encrypt_files == false)
                                        decrypted_data = encrypted_data;
                                    else
                                        decrypted_data = _CryptoService.DecryptData(encrypted_data, block_id.ToString());

                                    return JsonConvert.DeserializeObject<DocumentBlock>(decrypted_data);
                                }
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception("This document database type is not supported.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<DocumentBlock> GetBlock(int index)
        {
            var existing_block = _IContext.Blocks.Where(m => m.index == index).FirstOrDefault();

            if (existing_block == null)
                return null;

            var file_path = _DatabasePath + existing_block.block_id + ".blk";
            var file_name = existing_block.block_id + ".blk";

            if (_NodeConfig.document_db_type.ToLower() == "file")
            {
                if (!File.Exists(file_path))
                    return null;

                TextReader reader = new StreamReader(File.OpenRead(file_path));

                var encrypted_data = reader.ReadToEnd();

                reader.Close();

                var decrypted_data = "";
                if (_NodeConfig.encrypt_files == false)
                    decrypted_data = encrypted_data;
                else
                    decrypted_data = _CryptoService.DecryptData(encrypted_data, existing_block.block_id.ToString());

                return JsonConvert.DeserializeObject<DocumentBlock>(decrypted_data);
            }
            else if (_NodeConfig.document_db_type.ToLower() == "s3")
            {
                AmazonS3Config config = new AmazonS3Config();
                config.ServiceURL = _NodeConfig.s3_service_url;
                

                using (var s3Client = new AmazonS3Client(_NodeConfig.s3_service_access_key, _NodeConfig.s3_service_secret, config))
                {
                    var request = new GetObjectRequest();
                    request.BucketName = _NodeConfig.s3_service_bucket;
                    request.Key = file_name;

                    try
                    {
                        var response = await s3Client.GetObjectAsync(request);

                        using (var memoryStream = new MemoryStream())
                        {
                            using (Stream responseStream = response.ResponseStream)
                            {
                                responseStream.CopyTo(memoryStream);
                                await responseStream.FlushAsync();

                                memoryStream.Position = 0;

                                using (var reader = new StreamReader(memoryStream))
                                {
                                    var encrypted_data = reader.ReadToEnd();

                                    var decrypted_data = "";
                                    if (_NodeConfig.encrypt_files == false)
                                        decrypted_data = encrypted_data;
                                    else
                                        decrypted_data = _CryptoService.DecryptData(encrypted_data, existing_block.block_id.ToString());

                                    return JsonConvert.DeserializeObject<DocumentBlock>(decrypted_data);
                                }
                            }
                        }
                    }
                    catch(AmazonS3Exception aex)
                    {
                        if (aex.Message.Contains("NoSuchKey"))
                            throw new Exception("Could not find document block file " + file_name);
                        else
                            throw aex;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
            else
            {
                throw new Exception("This document database type is not supported.");
            }
        }

        public int GetBlockSize()
        {
            try
            {
                return _IContext.Blocks.Max(m => m.index);
            }
            catch(Exception)
            {
                return 0;
            }
        }

        public BlockDto ConvertToDto(DocumentBlock block)
        {
            var dto = new BlockDto()
            {

                BlockId = block.BlockId,
                CustomerId = block.CustomerId,
                Identifier1 = block.Identifier1,
                Identifier2 = block.Identifier2,
                Identifier3 = block.Identifier3,
                Identifier4 = block.Identifier4,
                Identifier5 = block.Identifier5,
                Index = block.Index,
                PreviousHash = block.PreviousHash,
                Hash = block.Hash,
                Nonce = block.Nonce,
                MinedAmount = block.MinedAmount,
                CreatedOn = block.CreatedOn,
                MinedOn = block.MinedOn,
                TransactionList = block.TransactionList,
                Miners = block.Miners,
            };

            if (block.Data != null && block.Data.Length > 0)
            {
                dto.Data = Convert.ToBase64String(block.Data);
            }

            return dto;
        }

        public DocumentBlock ConvertFromDto(BlockDto block)
        {
            var converted_block = new DocumentBlock()
            {

                BlockId = block.BlockId,
                CustomerId = block.CustomerId,
                Identifier1 = block.Identifier1,
                Identifier2 = block.Identifier2,
                Identifier3 = block.Identifier3,
                Identifier4 = block.Identifier4,
                Identifier5 = block.Identifier5,
                Index = block.Index,
                PreviousHash = block.PreviousHash,
                Hash = block.Hash,
                Nonce = block.Nonce,
                MinedAmount = block.MinedAmount,
                CreatedOn = block.CreatedOn,
                MinedOn = block.MinedOn,
                TransactionList = block.TransactionList,
                Miners = block.Miners,

            };

            if (block.Data != null && block.Data.Length > 0)
            {
                converted_block.Data = Encoding.UTF8.GetBytes(block.Data);
            }


            return converted_block;
        }
    }
}
