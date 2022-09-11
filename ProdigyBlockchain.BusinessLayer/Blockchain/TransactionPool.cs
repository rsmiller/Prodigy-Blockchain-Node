using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prodigy.BusinessLayer.Blockchain
{
    public class TransactionPool
    {
        private List<Transaction> rawTransactionList;

        private object lockObj;

        public TransactionPool()
        {
            lockObj = new object();
            rawTransactionList = new List<Transaction>();
        }

        public void AddRaw(Transaction transaction)
        {
            lock (lockObj)
            {
                rawTransactionList.Add(transaction);
            }
        }

        public List<Transaction> GetByBlockId(Guid block_id)
        {
            return this.rawTransactionList.Where(m => m.document_block_id == block_id.ToString()).ToList();
        }

        public List<Transaction> GetByMiner(string wallet_id)
        {
            return this.rawTransactionList.Where(m => m.to == wallet_id).ToList();
        }

        public List<Transaction> GetPending(string wallet_id)
        {
            return this.rawTransactionList.Where(m => m.from == wallet_id).ToList();
        }

        public int Count()
        {
            return rawTransactionList.Count();
        }

        public List<Transaction> TakeAll()
        {
            lock (lockObj)
            {
                var all = rawTransactionList.ToList();
                rawTransactionList.Clear();
                return all;
            }
        }
    }
}
