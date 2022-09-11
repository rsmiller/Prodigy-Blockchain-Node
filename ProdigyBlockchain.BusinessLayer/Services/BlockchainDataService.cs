using Prodigy.BusinessLayer.Models;
using Prodigy.BusinessLayer.Blockchain;
using Prodigy.BusinessLayer.Models;
using Prodigy.BusinessLayer.Models.Command;
using Prodigy.BusinessLayer.Models.Dto;
using Prodigy.BusinessLayer.Models.Response;
using Prodigy.BusinessLayer.Networks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prodigy.BusinessLayer.Services
{
    public interface IBlockchainDataService
    {
        Task<Response<BlockDto>> GetBlockById(Guid block_id);
        Task<Response<List<BlockDto>>> GetBlocksByIdentifier(string identifier, string wildcard, bool include_data = false);
        Response<List<BlockDto>> GetLatestBlocks();
        Response<List<Transaction>> GetLatestTransactions();
        Response<Transaction> GetTransaction(string txn);
        DocumentSearchResults SearchDocument(Guid? customer_id, string wildcard);
        BlockPage SearchByCustomer(Guid customer_id, string wildcard, int page = 0);
        Task<BlockPage> DownloadBlockchain(int page);
        BlockPage DownloadPendingBlockchain(int page);


        Response<Transaction> AddTransaction(AddTransactionCommand command);

        MinerRequestBlockResponse MinerRequestBlock(MiningUser user);
        MinerBlockMinedResponse MinerBlockMined(MinerBlockMinedCommand command);
        NodeListDto MinerJoin_Ack(MinerJoinCommand command);


        void BlockAdded(BlockDto block);
        void BlockValidated(BlockValidatedCommand command);

        Task BlockAdded_Outbound(DocumentBlock block);
        Task BlockValidated_Outbound(DocumentBlock block);
        Task DownloadBlockchain_Outbound();

        Task<Response<byte[]>> GetDocument(Guid block_id);
    }


    public class BlockchainDataService : IBlockchainDataService
    {
        private INetwork _Network;
        private INodeConfig _NodeConfig;
        private ICertContext _ICertContext;
        private IDocumentBlockchain _Blockchain;
        private RestClient _Client;
        private IDatabaseDataService _DatabaseDataContext;

        private CryptoService _CryptoService;

        public BlockchainDataService(INetwork network, INodeConfig nodeConfig, ICertContext certContext, IDatabaseDataService databaseDataContext, IDocumentBlockchain blockchain, string privateKey)
        {
            _Network = network;
            _NodeConfig = nodeConfig;
            _ICertContext = certContext;
            _Blockchain = blockchain;
            _DatabaseDataContext = databaseDataContext;

            _CryptoService = new CryptoService(privateKey);
        }

        public async Task<Response<List<BlockDto>>> GetBlocksByIdentifier(string identifier, string wildcard, bool include_data = false)
        {
            Response<List<BlockDto>> response = new Response<List<BlockDto>>();
            response.Data = new List<BlockDto>();

            List<DocumentBlock> found_blocks = new List<DocumentBlock>();

            if (identifier.Contains("1"))
                found_blocks = _Blockchain.Where(m => m.Identifier1 == wildcard).ToList();
            else if (identifier.Contains("2"))
                found_blocks = _Blockchain.Where(m => m.Identifier2 == wildcard).ToList();
            else if (identifier.Contains("3"))
                found_blocks = _Blockchain.Where(m => m.Identifier3 == wildcard).ToList();
            else if (identifier.Contains("4"))
                found_blocks = _Blockchain.Where(m => m.Identifier4 == wildcard).ToList();
            else if (identifier.Contains("5"))
                found_blocks = _Blockchain.Where(m => m.Identifier5 == wildcard).ToList();
            else
                return new Response<List<BlockDto>>("Invalid identifer. Example: Indentifer1 or Indentifer5");


            foreach (var found_block in found_blocks)
            {
                var file_block = await _DatabaseDataContext.GetBlock(found_block.BlockId);

                if (file_block != null)
                {
                    if(include_data == false)
                    {
                        file_block.Data = null;
                    }

                    response.Data.Add(ConvertToDto(file_block));
                }
            }

            return response;
        }

        public async Task<Response<BlockDto>> GetBlockById(Guid block_id)
        {
            Response<BlockDto> response = new Response<BlockDto>();

            var found_block = _Blockchain.Where(m => m.BlockId == block_id).FirstOrDefault();

            if (found_block == null)
                return new Response<BlockDto>("Block not found");

            var file_block = await _DatabaseDataContext.GetBlock(block_id);

            if (file_block == null)
            {
                // TODO: LOG ERROR
                throw new Exception("Block file not found!");
            }

            response.Data = ConvertToDto(file_block);

            return response;
        }

        public Response<List<BlockDto>> GetLatestBlocks()
        {
            Response<List<BlockDto>> results = new Response<List<BlockDto>>();
            results.Data = new List<BlockDto>();

            var list = _Blockchain.GetLatest(10);

            foreach(var block in list)
            {
                results.Data.Add(ConvertToDto(block));
            }

            return results;
        }

        public Response<List<Transaction>> GetLatestTransactions()
        {
            Response<List<Transaction>> results = new Response<List<Transaction>>();
            results.Data = _Blockchain.OrderByTransactions().Take(20).ToList();

            return results;
        }

        public Response<Transaction> GetTransaction(string txn)
        {
            Response<Transaction> response = new Response<Transaction>();

            response.Data = _Blockchain.WhereTransaction(m => m.txn == txn).FirstOrDefault();

            return response;
        }

        public Response<Transaction> AddTransaction(AddTransactionCommand command)
        {
            var existin_transaction = _Blockchain.WhereTransaction(m => m.to == command.to && m.from == command.from && m.created_on == command.created_on).FirstOrDefault();
            
            if (existin_transaction != null)
                return new Response<Transaction>("Already have transaction");

            var transaction = new Transaction(command.from, command.to, command.amount, command.document_block_id.ToString(), command.created_on);

            _Blockchain.SubmitTransaction(transaction);

            return new Response<Transaction>(transaction);
        }

        public NodeListDto MinerJoin_Ack(MinerJoinCommand command)
        {
            NodeListDto dto = new NodeListDto();

            foreach(var address in _Network.IPSeeds)
            {
                // Get some random nodes
                if(_Network.NodeList.Count > 0)
                {
                    var rand = new Random((int)DateTime.UtcNow.Ticks);
                    var random_node = _Network.NodeList.ElementAt(rand.Next(_Network.NodeList.Count()));

                    if (random_node != null)
                        dto.nodes.Add(random_node.ip_address.ToString());
                }

                dto.nodes.Add(address.ToString()); // Add seed node
            }

            dto.difficulty = _Network.Difficulty;

            return dto;
        }


        public async Task<BlockPage> DownloadBlockchain(int page)
        {
            int page_count = 1000;
            int skip = (page - 1) * page_count;
            int next_skip_amount = (page) * page_count;
            int blockchain_count = _Blockchain.Count();

            var dto = new BlockPage();
            dto.current_page = page;
            dto.total_pages = (int)Math.Ceiling((double)(blockchain_count / page_count));

            if (dto.total_pages <= 0)
                dto.total_pages = 1;

            var blocks = _Blockchain.Skip(skip).Take(page_count).ToList();

            foreach (var block in blocks)
            {
                var data_block = await _DatabaseDataContext.GetBlock(block.BlockId);

                dto.blocks.Add(ConvertToDto(data_block));
            }

            if (next_skip_amount <= blockchain_count)
            {
                dto.next_page = (page + 1);
            }
            else
            {
                dto.next_page = -1;
            }


            return dto;
        }


        public BlockPage DownloadPendingBlockchain(int page)
        {
            int page_count = 1000;
            int skip = (page - 1) * page_count;
            int next_skip_amount = (page) * page_count;

            var dto = new BlockPage();
            dto.current_page = page;

            var blocks = _Blockchain.SkipPending(skip).Take(page_count).ToList();

            foreach (var block in blocks)
            {
                dto.blocks.Add(ConvertToDto(block));
                // Not downloading block because pending already contains the data
            }

            if (next_skip_amount <= _Blockchain.CountPending())
            {
                dto.next_page = (page + 1);
            }
            else
            {
                dto.next_page = -1;
            }


            return dto;
        }
       
        public async Task DownloadBlockchain_Outbound()
        {
            var url = "http://" + _Network.IPSeeds.First().ToString() + ":" + _Network.DefaultAPIPort + "/";

            _Client = new RestClient(url);
            var request = new RestRequest("api/Blockchain/DownloadBlockchain?page=1", Method.GET, DataFormat.Json);
            request.AddHeader("Authorization", "JWT " + _Network.NodePassword);

            var result = await _Client.ExecuteAsync<BlockPage>(request);
            if(result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                if(result.Data != null)
                {
                    Console.WriteLine("Processing Current Page " + result.Data.current_page);
                    if (result.Data.current_page <= result.Data.total_pages)
                    {
                        foreach (var block in result.Data.blocks)
                        {
                            var document_block = ConvertFromDto(block);
                            _Blockchain.BlockDownload(document_block);
                            
                            if(_NodeConfig.document_db_type.ToLower() == "file")
                                await _DatabaseDataContext.CreateBlock(document_block);

                        }
                            

                        await DownloadBlockchain_Outbound_Recursive(result.Data.current_page + 1);
                    }
                }

            }
            else
            {
                throw new Exception("Download failure");
            }
        }

        private async Task DownloadBlockchain_Outbound_Recursive(int page)
        {
            var url = "http://" + _Network.IPSeeds.First().ToString() + ":" + _Network.DefaultAPIPort + "/";

            _Client = new RestClient(url);
            var request = new RestRequest("api/Blockchain/DownloadBlockchain?page=" + page, Method.GET, DataFormat.Json);
            request.AddHeader("Authorization", "JWT " + _Network.NodePassword);

            var result = await _Client.ExecuteAsync<BlockPage>(request);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine("Processing Current Page " + result.Data.current_page);
                if (result.Data.current_page <= result.Data.total_pages)
                {
                    foreach (var block in result.Data.blocks)
                        _Blockchain.BlockDownload(ConvertFromDto(block));

                    await DownloadBlockchain_Outbound_Recursive(result.Data.current_page + 1);
                }
            }
            else
            {
                throw new Exception("Download failure");
            }
        }


        public async Task DownloadPendingBlockchain_Outbound()
        {
            var url = "http://" + _Network.IPSeeds.First().ToString() + ":" + _Network.DefaultAPIPort + "/";

            _Client = new RestClient(url);
            var request = new RestRequest("api/Blockchain/DownloadPendingBlockchain?page=1", Method.GET, DataFormat.Json);
            request.AddHeader("Authorization", "JWT " + _Network.NodePassword);

            var result = await _Client.ExecuteAsync<BlockPage>(request);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine("Processing Pending Page " + result.Data.current_page);
                if (result.Data.current_page <= result.Data.total_pages)
                {
                    foreach (var block in result.Data.blocks)
                        _Blockchain.BlockPendingDownload(ConvertFromDto(block));

                    await DownloadPendingBlockchain_Outbound_Recursive(result.Data.current_page + 1);
                }
            }
            else
            {
                throw new Exception("Download failure");
            }
        }

        private async Task DownloadPendingBlockchain_Outbound_Recursive(int page)
        {
            var url = "http://" + _Network.IPSeeds.First().ToString() + ":" + _Network.DefaultAPIPort + "/";

            _Client = new RestClient(url);
            var request = new RestRequest("api/Blockchain/DownloadPendingBlockchain?page=" + page, Method.GET, DataFormat.Json);
            request.AddHeader("Authorization", "JWT " + _Network.NodePassword);

            var result = await _Client.ExecuteAsync<BlockPage>(request);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine("Processing Pending Page " + result.Data.current_page);
                if (result.Data.current_page <= result.Data.total_pages)
                {
                    foreach (var block in result.Data.blocks)
                        _Blockchain.BlockPendingDownload(ConvertFromDto(block));

                    await DownloadPendingBlockchain_Outbound_Recursive(result.Data.current_page + 1);
                }
            }
            else
            {
                throw new Exception("Download failure");
            }
        }


        public void BlockValidated(BlockValidatedCommand command)
        {
            _Blockchain.BlockValidatedByNode(command.block_id, command.index, command.previous_hash);
        }

        public void BlockAdded(BlockDto block)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Got block: " + block.BlockId);
            Console.ForegroundColor = ConsoleColor.White;
            _Blockchain.BlockAddedByNode(new DocumentBlock(_CryptoService, block));
        }

        public async Task BlockAdded_Outbound(DocumentBlock block)
        {
            foreach (var node in _Network.NodeList)
            {
                Console.WriteLine("Sending block to " + node.ip_address.ToString());
                var url = "http://" + node.ip_address.ToString() + ":" + _Network.DefaultAPIPort + "/";

                _Client = new RestClient(url);
                var request = new RestRequest("api/Blockchain/BlockAdded", Method.POST, DataFormat.Json);
                request.AddHeader("Authorization", "JWT " + _Network.NodePassword);
                request.AddJsonBody(this.ConvertToDto(block));

                await _Client.ExecuteAsync(request);
            }
        }

        public async Task BlockValidated_Outbound(DocumentBlock block)
        {
            var command = new BlockValidatedCommand()
            {
                block_id = block.BlockId,
                index = block.Index,
                previous_hash = block.PreviousHash,
                node_id = _Network.NodeId,
                validated_on = DateTimeOffset.UtcNow
            };


            foreach (var node in _Network.NodeList)
            {
                var url = "http://" + node.ip_address.ToString() + ":" + _Network.DefaultAPIPort + "/";

                _Client = new RestClient(url);
                var request = new RestRequest("api/Blockchain/BlockValidated", DataFormat.Json);
                request.AddHeader("Authorization", "JWT " + _Network.NodePassword);
                request.AddJsonBody(command);

                await _Client.ExecuteAsync(request);
            }
        }

        public MinerRequestBlockResponse MinerRequestBlock(MiningUser user)
        {
            var dto = new MinerRequestBlockResponse()
            { 
                node_id = _Network.NodeId,
                response_time = DateTimeOffset.UtcNow
            };

            var real_block = _Blockchain.GetBlockToMine(user);

            if(real_block != null)
            {
                dto.block = ConvertToDto(real_block);
            }

            return dto;

        }

        public MinerBlockMinedResponse MinerBlockMined(MinerBlockMinedCommand command)
        {
            MinerBlockMinedResponse response = new MinerBlockMinedResponse();

            var mining_result = _Blockchain.SubmitMinedBlock(command.block_id, command.block_hash, command.miner);

            if(mining_result == null)
            {
                response.validated = false;
                response.reward = 0;

                return response;
            }

            if(mining_result.CompleteEventType == MiningEventCompleteType.Success)
            {
                response.validated = true;
                response.hash = command.block_hash;
                response.reward = mining_result.Reward;
            }
            else
            {
                response.validated = false;
                response.hash = command.block_hash;
                response.reward = mining_result.Reward;
            }

            return response;
        }

        public BlockPage SearchByCustomer(Guid customer_id, string wildcard, int page = 0)
        {
            int page_count = 1000;
            int skip = (page - 1) * page_count;
            int next_skip_amount = (page) * page_count;

            var dto = new BlockPage();
            dto.current_page = page;

            var blockchain_count = _Blockchain.GetCustomerBlocks(customer_id).Count();

            var blocks_to_work_with = _Blockchain.GetCustomerBlocks(customer_id).Skip(skip).Take(page_count);

            foreach (var result in blocks_to_work_with)
            {
                var block_dto = ConvertToDto(result);
                block_dto.Data = "";

                dto.blocks.Add(block_dto);
            }

            if (next_skip_amount <= blockchain_count)
            {
                dto.next_page = (page + 1);
            }
            else
            {
                dto.next_page = -1;
            }

            dto.total_pages = (int)Math.Ceiling((double)(blockchain_count / page_count));

            if (dto.total_pages <= 0)
                dto.total_pages = 1;

            return dto;
        }


        public DocumentSearchResults SearchDocument(Guid? customer_id, string wildcard)
        {
            List<BlockDto> list = new List<BlockDto>();

            if(customer_id.HasValue)
            {
                var blocks_to_work_with = _Blockchain.GetCustomerBlocks(customer_id.Value);

                List<DocumentBlock> results = new List<DocumentBlock>();

                if(!String.IsNullOrEmpty(wildcard))
                {
                    blocks_to_work_with.Where(m => m.Identifier1.Contains(wildcard)
                                           || m.Identifier2.Contains(wildcard)
                                           || m.Identifier3.Contains(wildcard)
                                           || m.Identifier4.Contains(wildcard)
                                           || m.Identifier5.Contains(wildcard)).ToList();
                }
                else
                {
                    results = blocks_to_work_with;
                }


                foreach (var result in results)
                {
                    var dto = ConvertToDto(result);
                    dto.Data = "";

                    list.Add(dto);
                }

                return new DocumentSearchResults()
                {
                    blocks = list,
                    total_company_results = blocks_to_work_with.Count(),
                    returned_results = list.Count()
                };
            }
            else
            {
                var results = _Blockchain.Where(m => m.Identifier1.Contains(wildcard)
                                                        || m.Identifier2.Contains(wildcard)
                                                        || m.Identifier3.Contains(wildcard)
                                                        || m.Identifier4.Contains(wildcard)
                                                        || m.Identifier5.Contains(wildcard)).ToList();

                foreach (var result in results)
                {
                    var dto = ConvertToDto(result);
                    dto.Data = "";

                    list.Add(dto);
                }

                return new DocumentSearchResults()
                {
                    blocks = list,
                    total_company_results = results.Count(),
                    returned_results = list.Count()
                };
            }
        }

        public async Task<Response<byte[]>> GetDocument(Guid block_id)
        {
            Response<byte[]> response = new Response<byte[]>();

            var found_block = _Blockchain.Where(m => m.BlockId == block_id).FirstOrDefault();

            if (found_block == null)
                return new Response<byte[]>("Block not found");

            var file_block = await _DatabaseDataContext.GetBlock(block_id);

            if (file_block == null)
            {
                // TODO: LOG ERROR
                throw new Exception("Block file not found!");
            }

            response.Data = file_block.Data;

            return response;
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

        public static bool IsBase64String(string base64)
        {
            Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
            return Convert.TryFromBase64String(base64, buffer, out int bytesParsed);
        }
    }
}
