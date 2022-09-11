using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using Prodigy.BusinessLayer.Models.Dto;
using Prodigy.BusinessLayer.Models;
using Prodigy.BusinessLayer.Services;

namespace Prodigy.BusinessLayer.Blockchain
{
	public interface IDocumentBlock
	{
		Guid BlockId { get; set; }
		Guid CustomerId { get; set; }
		string Identifier1 { get; set; }
		string Identifier2 { get; set; }
		string Identifier3 { get; set; }
		string Identifier4 { get; set; }
		string Identifier5 { get; set; }
		int Index { get; set; }
		long CreatedOn { get; set; }
		long MinedOn { get; set; }
		string PreviousHash { get; set; }
		string Hash { get; set; }
		int Nonce { get; set; }
		int MinedAmount { get; set; }
		string ValidateHash(int difficulty);
		List<Transaction> TransactionList { get; set; }
		List<MiningUser> Miners { get; set; }
	}
	[Serializable]
	public class DocumentBlock : IDocumentBlock
	{
		public Guid BlockId { get; set; } = Guid.NewGuid();
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
		public byte[] Data { get; set; }
		public List<Transaction> TransactionList { get; set; } = new List<Transaction>();
		public List<MiningUser> Miners { get; set; } = new List<MiningUser>();

		private CryptoService _CryptoService;

		public DocumentBlock()
        {

        }

		public DocumentBlock(CryptoService cryptoService, BlockDto dto)
        {
			_CryptoService = cryptoService;

			this.BlockId = dto.BlockId;
            this.Identifier1 = dto.Identifier1;
            this.Identifier2 = dto.Identifier2;
			this.Identifier3 = dto.Identifier3;
			this.Identifier4 = dto.Identifier4;
			this.Identifier5 = dto.Identifier5;
			this.CreatedOn = dto.CreatedOn;
            this.MinedOn = dto.MinedOn;
            this.PreviousHash = dto.PreviousHash;
            this.Nonce = dto.Nonce;
            this.Data = Convert.FromBase64String(dto.Data);
            this.Hash = dto.Hash;
        }

		public DocumentBlock(CryptoService cryptoService, int index, string data, string identifier1, Guid customer_id)
		{
			_CryptoService = cryptoService;

			CreatedOn = DateTime.Now.ToFileTimeUtc();
			PreviousHash = "";
			Index = index;
			Identifier1 = identifier1;
			Hash = "1111";
			Data = System.Convert.FromBase64String(data);
			//Data = Encoding.UTF8.GetBytes(_CryptoService.EncryptData(data, this.BlockId.ToString())) ?? throw new ArgumentException(nameof(data));
		}

		public DocumentBlock(CryptoService cryptoService, int index, string data, string identifier1, string identifier2, Guid customer_id)
		{
			_CryptoService = cryptoService;

			CreatedOn = DateTime.Now.ToFileTimeUtc();
			PreviousHash = "";
			Index = index;
			Identifier1 = identifier1;
			Identifier2 = identifier2;
			Hash = "1111";
			Data = System.Convert.FromBase64String(data);
			//Data = Encoding.UTF8.GetBytes(_CryptoService.EncryptData(data, this.BlockId.ToString())) ?? throw new ArgumentException(nameof(data));
		}

		public DocumentBlock(CryptoService cryptoService, int index, string data, string identifier1, string identifier2, string identifier3, Guid customer_id)
		{
			_CryptoService = cryptoService;

			CreatedOn = DateTime.Now.ToFileTimeUtc();
			PreviousHash = "";
			Index = index;
			Identifier1 = identifier1;
			Identifier2 = identifier2;
			Identifier3 = identifier3;
			Hash = "1111";
			Data = System.Convert.FromBase64String(data);
			//Data = Encoding.UTF8.GetBytes(_CryptoService.EncryptData(data, this.BlockId.ToString())) ?? throw new ArgumentException(nameof(data));
		}

		public DocumentBlock(CryptoService cryptoService, int index, string data, string identifier1, string identifier2, string identifier3, string identifier4, Guid customer_id)
		{
			_CryptoService = cryptoService;

			CreatedOn = DateTime.Now.ToFileTimeUtc();
			PreviousHash = "";
			Index = index;
			Identifier1 = identifier1;
			Identifier2 = identifier2;
			Identifier3 = identifier3;
			Identifier4 = identifier4;
			Hash = "1111";
			Data = System.Convert.FromBase64String(data);
			//Data = Encoding.UTF8.GetBytes(_CryptoService.EncryptData(data, this.BlockId.ToString())) ?? throw new ArgumentException(nameof(data));
		}

		public DocumentBlock(CryptoService cryptoService, int index, string data, string identifier1, string identifier2, string identifier3, string identifier4, string identifier5, Guid customer_id)
		{
			_CryptoService = cryptoService;

			CreatedOn = DateTime.Now.ToFileTimeUtc();
			PreviousHash = "";
			Index = index;
			Identifier1 = identifier1;
			Identifier2 = identifier2;
			Identifier3 = identifier3;
			Identifier4 = identifier4;
			Identifier5 = identifier5;
			Hash = "1111";
			Data = System.Convert.FromBase64String(data);
			//Data = Encoding.UTF8.GetBytes(_CryptoService.EncryptData(data, this.BlockId.ToString())) ?? throw new ArgumentException(nameof(data));
		}

		public DocumentBlock(CryptoService cryptoService, int index, byte[] data, string identifier1, string identifier2, string identifier3, string identifier4, string identifier5, Guid customer_id)
		{
			_CryptoService = cryptoService;

			CreatedOn = DateTime.Now.ToFileTimeUtc();
			PreviousHash = "";
			Index = index;
			Identifier1 = identifier1;
			Identifier2 = identifier2;
			Identifier3 = identifier3;
			Identifier4 = identifier4;
			Identifier5 = identifier5;
			Hash = "1111";
			Data = data;
		}

		public DocumentBlock Duplicate(CryptoService cryptoService, List<Transaction> transactions)
        {
			var block = new DocumentBlock(cryptoService, this.Index, this.Data, this.Identifier1, this.Identifier2, this.Identifier3, this.Identifier4, this.Identifier5, this.CustomerId);
			block.BlockId = this.BlockId;
			block.Index = this.Index;
			block.CreatedOn = this.CreatedOn;
			block.Nonce = this.Nonce;
			block.PreviousHash = this.PreviousHash;
			block.Hash = this.Hash;
			block.Data = this.Data;
			block.Identifier1 = this.Identifier1;
			block.Identifier2 = this.Identifier2;
			block.Identifier3 = this.Identifier3;
			block.Identifier4 = this.Identifier4;
			block.Identifier5 = this.Identifier5;
			block.TransactionList = transactions;
			block.Miners = this.Miners;

			return block;
		}

		public string GenerateHash()
		{
			var merkleRootHash = FindMerkleRootHash(this.TransactionList);
			return CryptoService.CalculateHash($"{this.Data} + {this.PreviousHash} + {this.CreatedOn} + {JsonConvert.SerializeObject(this.Data) + this.Nonce + merkleRootHash}");
		}

		public string ValidateHash(int difficulty)
		{
			if (this.Data == null)
				throw new ArgumentException(nameof(this.Data));

			string hash_to_match = "";
			int max_nonce = 10000000;
			this.Nonce = 0; // Setting nonce for proof of work

			while (this.Hash != hash_to_match)
			{
				hash_to_match = this.GenerateHash();
				this.Nonce++;

				if (this.Nonce > max_nonce)
					return "";
			}

			return this.Hash;
		}

		public virtual DocumentBlock Mine(int difficulty)
		{
			if (this.Data == null)
				throw new ArgumentException(nameof(this.Data));

			//this.Hash = GenerateHash();
			while (this.Hash.Substring(0, difficulty) != "0".PadLeft(difficulty, '0'))
			{
				this.Nonce++;
				this.Hash = this.GenerateHash();
			}

			this.MinedOn = DateTime.Now.ToFileTimeUtc();

			return this;
		}

		private string FindMerkleRootHash(IList<Transaction> transactionList)
		{
			var transactionStrList = transactionList.Select(tran => CryptoService.CalculateHash(CryptoService.CalculateHash(tran.from + tran.to + tran.amount))).ToList();
			return BuildMerkleRootHash(transactionStrList);
		}

		private string BuildMerkleRootHash(IList<string> merkelLeaves)
		{
			if (merkelLeaves == null || !merkelLeaves.Any())
				return string.Empty;

			if (merkelLeaves.Count() == 1)
				return merkelLeaves.First();

			if (merkelLeaves.Count() % 2 > 0)
				merkelLeaves.Add(merkelLeaves.Last());

			var merkleBranches = new List<string>();

			for (int i = 0; i < merkelLeaves.Count(); i += 2)
			{
				var leafPair = string.Concat(merkelLeaves[i], merkelLeaves[i + 1]);
				merkleBranches.Add(CryptoService.CalculateHash(CryptoService.CalculateHash(leafPair)));
			}
			return BuildMerkleRootHash(merkleBranches);
		}

		public bool IsValid()
		{
			return this.Hash.SequenceEqual(this.GenerateHash());
		}
		public bool IsValid(IEnumerable<DocumentBlock> blocks)
		{
			return blocks.Zip(blocks.Skip(1), Tuple.Create).All(block => block.Item2.IsValid() && block.Item2.IsValidPreviousBlock(block.Item1));
		}

		public bool IsValidPreviousBlock(DocumentBlock previousBlock)
		{
			if (previousBlock == null)
				throw new ArgumentException(nameof(previousBlock));

			return previousBlock.IsValid();// && this.PreviousHash == previousBlock.GenerateHash();
		}
	}
}
