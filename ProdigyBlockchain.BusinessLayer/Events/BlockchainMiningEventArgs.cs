using Prodigy.BusinessLayer.Blockchain;
using Prodigy.BusinessLayer.Models;
using System;

namespace Prodigy.BusinessLayer.Events
{
	public class BlockchainMiningEventArgs
	{
		public MiningUser User { get; private set; }
		public decimal Reward { get; private set; }

		public DocumentBlock DocumentBlock { get; private set; }

		public MiningEventCompleteType CompleteEventType;

		public BlockchainMiningEventArgs(MiningUser user, MiningEventCompleteType completeType, decimal reward, DocumentBlock block)
		{
			User = user;
			Reward = reward;
			CompleteEventType = completeType;
			DocumentBlock = block;
		}
	}
}
