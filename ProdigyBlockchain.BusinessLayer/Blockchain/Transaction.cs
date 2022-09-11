using Prodigy.BusinessLayer.Services;
using System;

namespace Prodigy.BusinessLayer.Blockchain
{
    [Serializable]
    public class Transaction
    {
        public string txn { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public decimal amount { get; set; }
        public long created_on { get; set; }
        public string document_block_id { get; set; }
        public string note { get; set; }

        public Transaction()
        {

        }


        public Transaction(string from, string to, decimal amount, string cert_block_id, long created_on)
        {
            this.from = from;
            this.to = to;
            this.amount = amount;
            this.document_block_id = cert_block_id;
            this.created_on = created_on;
            this.note = note;

            this.txn = CryptoService.CalculateHash($"{this.from} + {this.to} + {this.amount} + {this.note} + {this.document_block_id}");
        }
    }
}
