using Prodigy.BusinessLayer.Models;
using System;

namespace Prodigy.BusinessLayer.Events
{
	public class TransactionMiningEventArgs
	{
		public MiningUser User { get; private set; }
		public decimal Reward { get; private set; }

		public Guid CertBlockId { get; private set; }

		public MiningEventCompleteType CompleteEventType;

		public TransactionMiningEventArgs(MiningUser user, MiningEventCompleteType completeType, decimal reward, Guid certBlockId)
		{
			User = user;
			Reward = reward;
			CompleteEventType = completeType;
			CertBlockId = certBlockId;
		}
	}
}
