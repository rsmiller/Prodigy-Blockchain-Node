using Newtonsoft.Json;
using Prodigy.BusinessLayer.Blockchain;
using Prodigy.BusinessLayer.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Prodigy.BusinessLayer.Wallet
{
    [Serializable]
    public class CryptoWallet
    {
        public string wallet_address { get; private set; }

        private string _pk { get; set; }

        private ICryptoService _CryptoService;

        private List<Transaction> Transactions { get; set; } = new List<Transaction>();

        public decimal Amount
        {
            get
            {
                var toTransAmount = Transactions.Where(m => m.to == wallet_address).Sum(m => m.amount);
                var fromTransAmount = Transactions.Where(m => m.from == wallet_address).Sum(m => m.amount);

                return toTransAmount - fromTransAmount;
            }
        }

        public CryptoWallet(ICryptoService service)
        {
            _CryptoService = service;
        }

        public CryptoWallet(string address)
        {
            wallet_address = address;
        }

        public CryptoWallet(WalletFile file, string address)
        {
            wallet_address = address;
            _pk = file.pk;
        }
    }
}
