using Newtonsoft.Json;
using Prodigy.BusinessLayer.Blockchain;
using Prodigy.BusinessLayer.Models.Command;
using Prodigy.BusinessLayer.Models.Dto;
using Prodigy.BusinessLayer.Models.Response;
using Prodigy.BusinessLayer.Networks;
using Prodigy.BusinessLayer.Wallet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prodigy.BusinessLayer.Services
{
    public interface IWalletDataService
    {
        Response<WalletCreatedDto> CreateWallet(CreateWalletCommand command);
        Response<WalletStatisticsDto> LoadWallet(GetWalletCommand command);
        Response<WalletStatisticsDto> GetWalletStatistics(string wallet_id);
        PagedResult<List<Transaction>> GetWalletTransactions(string wallet_id, int page = 0);
        Response<bool> RequestSpend(RequestSpendCommand command);
    }

    public class WalletDataService : IWalletDataService
    {
        private ICryptoService _ICryptoService;
        private INetwork _Network;
        private IWalletSettings _WalletSettings;
        private IDocumentBlockchain _Blockchain;

        public WalletDataService(INetwork network,IWalletSettings settings, IDocumentBlockchain blockchain, ICryptoService cryptoService)
        {
            _Network = network;
            _WalletSettings = settings;
            _Blockchain = blockchain;
            _ICryptoService = cryptoService;
        }
        
        public Response<WalletStatisticsDto> GetWalletStatistics(string wallet_id)
        {
            Response<WalletStatisticsDto> response = new Response<WalletStatisticsDto>();
            response.Data = new WalletStatisticsDto();

            var all_transactions = _Blockchain.GetWalletTransaction(wallet_id).ToList();
            
            decimal recieved = all_transactions.Where(m => m.to == wallet_id).Sum(m => m.amount);
            decimal sent = all_transactions.Where(m => m.from == wallet_id).Sum(m => m.amount);
            decimal pending = _Blockchain.PendingWalletTransactionAmount(wallet_id);

            response.Data.balance = (recieved - sent - pending);
            response.Data.pending = pending;

            return response;
        }

        public PagedResult<List<Transaction>> GetWalletTransactions(string wallet_id, int page = 0)
        {
            PagedResult<List<Transaction>> results = new PagedResult<List<Transaction>>();
            int page_count = 1000;
            int skip = (page - 1) * page_count;

            var total_count = _Blockchain.GetWalletTransaction(wallet_id).Count();

            results.Data = _Blockchain.GetWalletTransaction(wallet_id).Skip(skip).Take(page_count).ToList();
            results.TotalResults = total_count;

            return results;
        }

        public Response<bool> RequestSpend(RequestSpendCommand command)
        {
            var wallet_statistics = GetWalletStatistics(command.wallet_id);

            if(wallet_statistics.Success && wallet_statistics.Data.balance >= command.amount)
            {
                this._Blockchain.SubmitTransaction(new Transaction()
                {
                    from = command.wallet_id,
                    to = _Network.NodeId.ToString(),
                    amount = command.amount,
                    created_on = DateTime.Now.ToFileTimeUtc(),
                });
            }
            else
            {
                return new Response<bool>(false);
            }

            return new Response<bool>(true);
        }

        public Response<WalletCreatedDto> CreateWallet(CreateWalletCommand command)
        {
            try
            {
                Response<WalletCreatedDto> response = new Response<WalletCreatedDto>();

                var wallet = this.Create(_ICryptoService, command.username, command.password);

                if (wallet == null)
                    return new Response<WalletCreatedDto>("Username in use");

                response.Data = new WalletCreatedDto()
                {
                    wallet_id = wallet.wallet_address,
                    created_on = DateTime.Now
                };

                return response;
            }
            catch (Exception e)
            {
                return new Response<WalletCreatedDto>(e.Message);
            }
        }

        public Response<WalletStatisticsDto> LoadWallet(GetWalletCommand command)
        {
            try
            {
                var wallet = this.Load(_ICryptoService, command.wallet_address, command.username, command.passphrase);

                return GetWalletStatistics(wallet.wallet_address);
            }
            catch (Exception e)
            {
                return new Response<WalletStatisticsDto>(e.Message);
            }
        }

        private CryptoWallet Load(ICryptoService service, string address, string username, string passphrase)
        {
            try
            {
                CreateDirectoryIfNotExists();

                var file_name = _WalletSettings.WalletDirectory + address + ".json";

                if (!File.Exists(file_name))
                    return null;

                var encrypted_data = File.ReadAllText(file_name);

                var decrypted_data = service.DecryptData(encrypted_data, address + "-:!:-" + passphrase);

                var wallet_file = JsonConvert.DeserializeObject<WalletFile>(decrypted_data);

                // Validate 
                if(service.DecryptSuccessful(address, wallet_file.pk, username, passphrase))
                {
                    return new CryptoWallet(wallet_file, address);
                }
                else
                {
                    throw new Exception("Invalid password");
                }
                
            }
            catch (Exception)
            {
                throw new Exception("Could not load wallet");
            }

        }

        private CryptoWallet Create(ICryptoService service, string username, string passphrase)
        {
            var pair = service.CreateWalletKeyPair(username, passphrase);

            WalletFile wf = new WalletFile();
            wf.pk = pair.priv;

            var new_wallet = new CryptoWallet(pair.pub);

            CreateDirectoryIfNotExists();

            var file_name = _WalletSettings.WalletDirectory + new_wallet.wallet_address + ".json";

            if (File.Exists(file_name))
                return null;


            var json = JsonConvert.SerializeObject(wf);

            var encrypted_data = service.EncryptData(json, pair.pub + "-:!:-" + passphrase);

            File.WriteAllText(file_name, encrypted_data);

            return new_wallet;
        }

        private void CreateDirectoryIfNotExists()
        {
            if (!Directory.Exists(_WalletSettings.WalletDirectory))
                Directory.CreateDirectory(_WalletSettings.WalletDirectory);
        }
    }
}
