using Prodigy.BusinessLayer.Blockchain;
using System;
using System.Collections.Generic;

namespace Prodigy.BusinessLayer.Models.Dto
{
    public class TransactionBlockDto
    {
        public Guid BlockId { get; set; }
        public long Index { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public string Hash { get; set; }
        public string PreviousHash { get; set; }
        public long Nounce { get; set; }
        public List<Transaction> TransactionList { get; set; }
    }
}
