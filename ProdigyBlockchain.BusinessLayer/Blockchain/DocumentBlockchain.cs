using System;
using System.Collections.Generic;
using System.Linq;
using Prodigy.BusinessLayer.Models;
using Prodigy.BusinessLayer.Models;
using Newtonsoft.Json;
using static Prodigy.BusinessLayer.Blockchain.DocumentBlockchain;
using Prodigy.BusinessLayer.Events;
using Prodigy.BusinessLayer.Networks;
using System.Linq.Expressions;
using Prodigy.BusinessLayer.Services;
using Prodigy.BusinessLayer.Models.Command;
using System.Text;

namespace Prodigy.BusinessLayer.Blockchain
{
    public interface IDocumentBlockchain
	{
		Guid AddDataFromNode(DocumentCreateCommand createCommand);
		BlockchainMiningEventArgs SubmitMinedBlock(Guid blockId, string mined_hash, MiningUser user);
		void BlockValidatedByNode(Guid block_id, int index, string previous_hash);
		void BlockAddedByNode(DocumentBlock block);
		void BlockDownload(DocumentBlock block);
		void BlockPendingDownload(DocumentBlock block);
		List<DocumentBlock> GetCustomerBlocks(Guid customer_id);
		void SubmitTransaction(Transaction transaction);
		void SetGenesisBlock(Guid block_id);
		void StressTestBlockchain();
		void LoadFromDatabase(DocumentBlock block);
		DocumentBlock GetBlockToMine(MiningUser user);
		DocumentBlock LastOrDefault();
		DocumentBlock FirstOrDefault();
		DocumentBlock GetByIndex(int index);
		List<Transaction> GetWalletTransaction(string wallet_id);
		decimal PendingWalletTransactionAmount(string wallet_id);
		IEnumerable<DocumentBlock> Skip(int v);
		IEnumerable<DocumentBlock> Take(int v);
		IEnumerable<DocumentBlock> SkipPending(int v);
		IEnumerable<DocumentBlock> TakePending(int v);
		IEnumerable<DocumentBlock> GetLatest(int take);
		IEnumerable<DocumentBlock> Where(Func<DocumentBlock, bool> predicate);
		IEnumerable<Transaction> WhereTransaction(Func<Transaction, bool> predicate);
		IOrderedEnumerable<DocumentBlock> OrderBy();
		IOrderedEnumerable<DocumentBlock> OrderByDescending();
		IOrderedEnumerable<Transaction> OrderByTransactions();
		int IndexOf(DocumentBlock block);
		int Count();
		int TransactionsCount();
		int CountPending();

		event BlockchainEventHandler NewBlockAvailable;
		event TransactionMiningEventHandler BlockValidated;
		event TransactionMiningEventHandler BlockMined;
	}

    public class DocumentBlockchain : IDocumentBlockchain
	{
		private INetwork _Network;
		private CryptoService _CryptoService;
		private Guid _NodeId;
		private TransactionPool _TransactionPool;
		private INodeConfig _NodeConfig;

		public decimal Reward { get; } = 0.00009M; // Should equal about 3.5 per year ( x/20000 = 3.5 ) if the user gets the reward. Averaging about 20,000 certa a year
		public int Difficulty { get; private set; } = 1;
		public List<DocumentBlock> CurrentBlocks { get; private set; } = new List<DocumentBlock>();
		public List<DocumentBlock> NeedingConcensus { get; private set; } = new List<DocumentBlock>();
		public List<Transaction> AllTransactions { get; private set; } = new List<Transaction>();


		// EVENTS
		public delegate void BlockchainEventHandler(object sender, BlockchainEventArgs e);
		public delegate void TransactionMiningEventHandler(object sender, BlockchainMiningEventArgs e);
		public event BlockchainEventHandler NewBlockAvailable;
		public event TransactionMiningEventHandler BlockValidated;
		public event TransactionMiningEventHandler BlockMined;

		public DocumentBlockchain(INetwork network, INodeConfig nodeConfig, CryptoService cryptoService, TransactionPool transactionPool)
        {
			_Network = network;
			_NodeId = Guid.Parse(nodeConfig.node_id);
			_CryptoService = cryptoService;
			_TransactionPool = transactionPool;
			_NodeConfig = nodeConfig;

			Difficulty = network.Difficulty;

			Reward = ((decimal)_NodeConfig.max_coins_per_year / (decimal)_NodeConfig.average_certs_per_year);
		}

		public List<DocumentBlock> GetCustomerBlocks(Guid customer_id)
        {
			return this.CurrentBlocks.Where(m => m.CustomerId == customer_id).ToList();
        }

		public void BlockAddedByNode(DocumentBlock block)
        {
			var found_current_block = CurrentBlocks.Where(m => m.BlockId == block.BlockId).SingleOrDefault();
			var found_concensus_block = NeedingConcensus.Where(m => m.BlockId == block.BlockId).SingleOrDefault();

			if (found_current_block == null && found_concensus_block == null)
            {
				Console.ForegroundColor = ConsoleColor.Blue;
				Console.WriteLine("Block was added to another node. Adding " + block.BlockId.ToString());
				Console.ForegroundColor = ConsoleColor.White;
				ProcessNewBlock(block);
			}
		}

		public void BlockDownload(DocumentBlock block)
        {
			var found_current_block = CurrentBlocks.Where(m => m.BlockId == block.BlockId).SingleOrDefault();
			if (found_current_block == null)
			{
				CurrentBlocks.Add(block);
			}
		}

		public void BlockPendingDownload(DocumentBlock block)
		{
			var found_current_block = NeedingConcensus.Where(m => m.BlockId == block.BlockId).SingleOrDefault();
			if (found_current_block == null)
			{
				NeedingConcensus.Add(block);
			}
		}

		public void BlockValidatedByNode(Guid block_id, int index, string previous_hash)
        {
			var foundBlock = NeedingConcensus.Where(m => m.BlockId == block_id).SingleOrDefault();

			if(foundBlock == null)
            {
				var previous_has_exists = this.CurrentBlocks.SingleOrDefault(m => m.Hash == foundBlock.PreviousHash);

				if (previous_has_exists != null)
				{
					// To save on memory we need to remove the document from the file.
					// It will be retrieved when a users specifically wants it from the block file
					foundBlock.Data = new byte[0];
					AllTransactions.AddRange(foundBlock.TransactionList);

					this.CurrentBlocks.Add(foundBlock);

					NeedingConcensus.Remove(foundBlock);

					Console.ForegroundColor = ConsoleColor.Blue;
					Console.WriteLine("Block validaed from another node. Adding " + foundBlock.BlockId.ToString() + " to blockchain.");
					Console.ForegroundColor = ConsoleColor.White;
				}
				else
                {
					throw new Exception("Previous hash doesn't exist in the stack. Blockchain is out of sync");
				}
			}
			else
            {
				throw new Exception("Block validated without having corresponding block that needed concensus");
            }
		}

		public BlockchainMiningEventArgs SubmitMinedBlock(Guid blockId, string mined_hash, MiningUser user)
		{
			if (user == null)
				return null;

			var foundBlock = NeedingConcensus.Where(m => m.BlockId == blockId && m.Hash == mined_hash).SingleOrDefault();

			if (foundBlock != null)
			{
				//var found_miner = foundBlock.TransactionList.Where(m => m.from == _NodeId.ToString() && m.cert_block_id == foundBlock.BlockId.ToString() && m.to == user.walled_id).FirstOrDefault();
				var found_miner = foundBlock.Miners.Where(m => m.internal_ip_address == user.internal_ip_address && m.external_ip_address == user.external_ip_address && m.walled_id == user.walled_id).FirstOrDefault();

				if (foundBlock.MinedAmount >= _Network.MiningConfirmationSize)
				{
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine("Consensus complete. Adding block to chain.");
					Console.ForegroundColor = ConsoleColor.White;
					foundBlock.Miners.Add(user);

					// Side loading so we need to dupe the block for writing to the database and sending to the other nodes
					var duped_transactions = foundBlock.TransactionList;
					var dupeblock = foundBlock.Duplicate(_CryptoService, duped_transactions);

					// Sed to to be processed and put into a file
					var args = new BlockchainMiningEventArgs(user, MiningEventCompleteType.Success, Reward, dupeblock);
					BlockValidated(this, args);


					// To save on memory we need to remove the document from the file.
					// It will be retrieved when a users specifically wants it from the block file
					foundBlock.Data = new byte[0];
					
					AllTransactions.AddRange(foundBlock.TransactionList);

					foundBlock.TransactionList = new List<Transaction>();

					this.CurrentBlocks.Add(foundBlock);

					NeedingConcensus.Remove(foundBlock);

					// Mint some coins
					if (found_miner == null)
					{
						Console.ForegroundColor = ConsoleColor.Green;
						Console.WriteLine("Miner rewarded " + Reward);
						Console.ForegroundColor = ConsoleColor.White;
						_TransactionPool.AddRaw(new Transaction(_NodeId.ToString(), user.walled_id, Reward, foundBlock.BlockId.ToString(), DateTime.Now.ToFileTimeUtc()));
						CreateBlockIfTransactionsToHigh();
					}

					return args;
				}
				else
				{
					// Mining a current block
					if (found_miner == null)
					{
						foundBlock.MinedAmount++;
						foundBlock.Miners.Add(user);

						var participation_reward = Reward / 2;

						BlockchainMiningEventArgs args = new BlockchainMiningEventArgs(user, MiningEventCompleteType.Success, participation_reward, foundBlock);

						if (BlockMined != null)
						{
							BlockMined(this, args);
						}

						Console.ForegroundColor = ConsoleColor.Green;
						Console.WriteLine("Miner rewarded " + participation_reward);
						Console.ForegroundColor = ConsoleColor.White;
						_TransactionPool.AddRaw(new Transaction(_NodeId.ToString(), user.walled_id, participation_reward, foundBlock.BlockId.ToString(), DateTime.Now.ToFileTimeUtc()));
						CreateBlockIfTransactionsToHigh();

						return args;
					}
				}
			}

			return new BlockchainMiningEventArgs(user, MiningEventCompleteType.Failed, 0, null);
		}

		public void SetGenesisBlock(Guid block_id)
        {
			var block = this.CurrentBlocks.Where(m => m.BlockId == block_id).FirstOrDefault();
			if (block != null)
			{
				var index = this.CurrentBlocks.IndexOf(block);

				Console.WriteLine("Modiying blockchain...");

				this.CurrentBlocks = this.CurrentBlocks.Skip(index).ToList();

				Console.WriteLine("Set genesis to " + this.CurrentBlocks.First().BlockId);
			}
		}

		public void SubmitTransaction(Transaction transaction)
        {
			_TransactionPool.AddRaw(transaction);
        }

		public Guid AddDataFromNode(DocumentCreateCommand createCommand)
		{
			if(CurrentBlocks.Count() == 0)
            {
				this.GenerateGenesisBlock();
			}

			var block = new DocumentBlock(_CryptoService, CurrentBlocks.Count() + 1, createCommand.document_base64_data, createCommand.identifier1, createCommand.identifier2, createCommand.identifier3, createCommand.identifier4, createCommand.identifier5, createCommand.comapny_id);
			block.CustomerId = createCommand.comapny_id;
			block.TransactionList = _TransactionPool.TakeAll();

			ProcessNewBlock(block);

			return block.BlockId;
		}

		public DocumentBlock GetBlockToMine(MiningUser user)
        {
			// Need to always return the oldest block to validate
			var block = this.NeedingConcensus.FirstOrDefault();

			if(block != null)
            {
				//Clean so miner doesn't overide the mining process
				block.CustomerId = Guid.Empty;

				var already_mined = block.Miners.Where(m => m.internal_ip_address == user.internal_ip_address && m.external_ip_address == user.external_ip_address && m.walled_id == user.walled_id).FirstOrDefault();

				if (already_mined != null) // Can only mine the block once
					return null;

				Console.ForegroundColor = ConsoleColor.Magenta;
				Console.WriteLine("Giving " + user.external_ip_address + " " + block.BlockId.ToString());
				Console.ForegroundColor = ConsoleColor.White;

			}
			return block;
		}

		private DocumentBlock AddToConcensus(DocumentBlock block_to_add)
        {
			DocumentBlock block_to_use;

			var next_pending_block = this.NeedingConcensus.LastOrDefault();

			if (next_pending_block != null)
				block_to_use = next_pending_block; // Next block in the chain is the last once needing concensus
			else
				block_to_use = this.CurrentBlocks.Last(); // Next block is already on the block chain


			block_to_add.Index = block_to_use.Index + 1;
			block_to_add.PreviousHash = block_to_use.Hash;

			block_to_add.Mine(Difficulty);

			return block_to_add;
		}

		private void ProcessNewBlock(DocumentBlock block)
        {
			var foundBlock = NeedingConcensus.Where(m => m.BlockId == block.BlockId).SingleOrDefault();

			if(foundBlock == null)
            {
				var added_block = AddToConcensus(block);

				this.NeedingConcensus.Add(added_block);

				if (NewBlockAvailable != null)
				{
					BlockchainEventArgs args = new BlockchainEventArgs(BlockchainEvent.Added, added_block);
					NewBlockAvailable(this, args);
				}
			}
		}

		private void CreateBlockIfTransactionsToHigh()
        {
			if (_TransactionPool.Count() >= _Network.TransactionBlockHeight)
			{
				var transaction_only_block = new DocumentBlock(_CryptoService, 1, "TransactionBlock", "0", "0", Guid.Empty);
				transaction_only_block.TransactionList = _TransactionPool.TakeAll();

				Console.ForegroundColor = ConsoleColor.Magenta;
				Console.WriteLine("Transactions too high. making a block and adding it for miners. Alerting others of the new block.");
				Console.ForegroundColor = ConsoleColor.White;

				var added_block = AddToConcensus(transaction_only_block);
				this.NeedingConcensus.Add(added_block);

				if (NewBlockAvailable != null)
				{
					BlockchainEventArgs args = new BlockchainEventArgs(BlockchainEvent.Added, added_block);
					NewBlockAvailable(this, args);
				}
			}
		}

        public void GenerateGenesisBlock()
		{
			var block = new DocumentBlock(_CryptoService, 1, Convert.ToBase64String(Encoding.UTF8.GetBytes("Genesis")), "", "0", Guid.Empty);
			block.Hash = block.GenerateHash();
			block.Index = 1;
			block.Mine(Difficulty);

			this.CurrentBlocks.Add(block);


			BlockchainMiningEventArgs args = new BlockchainMiningEventArgs(null, MiningEventCompleteType.Success, 0, block);
			BlockValidated(this, args);

		}

		public List<Transaction> GetWalletTransaction(string wallet_id)
        {
			return this.AllTransactions.Where(m => m.to == wallet_id || m.from == wallet_id).ToList();
        }

		/// <summary>
		/// Returns an amount of spending transactions for a single account.
		/// </summary>
		/// <param name="wallet_id">Wallet address</param>
		/// <returns>Amount</returns>
		public decimal PendingWalletTransactionAmount(string wallet_id)
        {
			decimal concensus_amount = 0;
			this.NeedingConcensus.ForEach(m => concensus_amount += m.TransactionList.Where(x => x.from == wallet_id).Sum(m => m.amount) );
			
			decimal transactionpool_amount = this._TransactionPool.GetPending(wallet_id).Sum(m => m.amount);

			return concensus_amount + transactionpool_amount;

		}

		public DocumentBlock GetLastBlock()
        {
			return this.CurrentBlocks.LastOrDefault();
        }

		public IEnumerator<DocumentBlock> GetEnumerator()
		{
			return this.CurrentBlocks.GetEnumerator();
		}

		public IEnumerable<DocumentBlock> Skip(int v)
        {
			return this.CurrentBlocks.Skip(v);
        }
		public IEnumerable<DocumentBlock> Take(int v)
        {
			return this.CurrentBlocks.Take(v);
		}

		public IEnumerable<DocumentBlock> SkipPending(int v)
		{
			return this.NeedingConcensus.Skip(v);
		}
		public IEnumerable<DocumentBlock> TakePending(int v)
		{
			return this.NeedingConcensus.Take(v);
		}

		public DocumentBlock LastOrDefault()
		{
			return this.CurrentBlocks.LastOrDefault();
		}
		public DocumentBlock FirstOrDefault()
        {
			return this.CurrentBlocks.FirstOrDefault();
		}

		public int IndexOf(DocumentBlock block)
        {
			return this.CurrentBlocks.IndexOf(block);
        }

		public int Count()
        {
			return this.CurrentBlocks.Count();
		}

		public int CountPending()
		{
			return this.NeedingConcensus.Count();
		}

		public int TransactionsCount()
        {
			return this.AllTransactions.Count();
		}

		public DocumentBlock GetByIndex(int index)
		{
			return this.CurrentBlocks[index];
		}

		public IEnumerable<DocumentBlock> GetLatest(int take)
        {
			return this.CurrentBlocks.OrderByDescending(m => m.CreatedOn).Take(take);

		}

		public IEnumerable<Transaction> WhereTransaction(Func<Transaction, bool> predicate)
		{
			return this.AllTransactions.Where(predicate);
		}

		public IEnumerable<DocumentBlock> Where(Func<DocumentBlock, bool> predicate)
        {
			return this.CurrentBlocks.Where(predicate);
        }

		public IOrderedEnumerable<DocumentBlock> OrderBy()
		{
			return this.CurrentBlocks.OrderBy(m => m.CreatedOn);
		}

		public IOrderedEnumerable<DocumentBlock> OrderByDescending()
		{
			return this.CurrentBlocks.OrderByDescending(m => m.CreatedOn);
		}

		public IOrderedEnumerable<Transaction> OrderByTransactions()
        {
			return this.AllTransactions.OrderBy(m => m.created_on);
		}

		public void LoadFromDatabase(DocumentBlock block)
        {
			if(block != null)
            {
				block.Data = new byte[0];
				AllTransactions.AddRange(block.TransactionList);
				block.TransactionList = new List<Transaction>();

				this.CurrentBlocks.Add(block);
				Console.WriteLine("Added " + block.BlockId.ToString());
			}
		}

		public void StressTestBlockchain()
		{
			Console.WriteLine("Adding a million blocks");
			// Make a million of these bad boys with 25 million transactions
			for (int i = 0; i < 1000000000; i++)
            {
				var last_block = this.GetLastBlock();
				List<Transaction> transactions = new List<Transaction>();
				for (int t = 0; t < 25; t++)
				{
					transactions.Add(new Transaction(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), 1, Guid.NewGuid().ToString(), DateTime.Now.ToFileTimeUtc()));

				}

				var block = new DocumentBlock(_CryptoService, CurrentBlocks.Count() + 1, "", "1111111", "1111111-1-1", Guid.NewGuid());
				block.TransactionList = transactions;

				if (last_block == null)
					block.PreviousHash = "";
				else
					block.PreviousHash = last_block.Hash;

				block.Mine(Difficulty);

				this.CurrentBlocks.Add(block);

				if (i % 10000 == 0)
                {
					Console.WriteLine("Adding nth: " + i);
				}
			}
		}

		
	}
}
