using Prodigy.BusinessLayer.Blockchain;

namespace Prodigy.BusinessLayer.Events
{
	public class TranasctionBlockchainEventArgs
	{
		public string Hash { get; private set; }
		public DocumentBlock Block { get; private set; }
		public BlockchainEvent ChainEvent { get; private set; }

		public TranasctionBlockchainEventArgs(BlockchainEvent chainEvent, DocumentBlock block)
		{
			Block = block;
			Hash = block.Hash;
			ChainEvent = chainEvent;
		}
	}
}
