using Prodigy.BusinessLayer.Blockchain;
using System;
using System.Collections.Generic;

namespace Prodigy.BusinessLayer.Models.Dto
{
    public class BlockDto
    {
		public Guid BlockId { get; set; }
		public Guid CustomerId { get; set; }
		public string Identifier1 { get; set; }
		public string Identifier2 { get; set; }
		public string Identifier3 { get; set; }
		public string Identifier4 { get; set; }
		public string Identifier5 { get; set; }
		public int Index { get; set; }
		public string PreviousHash { get; set; }
		public string Hash { get; set; }
		public int Nonce { get; set; } = 0;
		public int MinedAmount { get; set; }
		public long CreatedOn { get; set; }
		public long MinedOn { get; set; }
		public string Data { get; set; }
		public List<Transaction> TransactionList { get; set; } = new List<Transaction>();
		public List<MiningUser> Miners { get; set; } = new List<MiningUser>();
	}
}
