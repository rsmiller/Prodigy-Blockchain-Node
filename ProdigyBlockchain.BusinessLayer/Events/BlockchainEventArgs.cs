using Prodigy.BusinessLayer.Blockchain;

namespace Prodigy.BusinessLayer.Events
{
	public class BlockchainEventArgs
	{
		public string Hash { get; private set; }
		public DocumentBlock Block { get; private set; }
		public BlockchainEvent ChainEvent { get; private set; }

		public BlockchainEventArgs(BlockchainEvent chainEvent, DocumentBlock block)
		{
			Block = block;
			Hash = block.Hash;
			ChainEvent = chainEvent;
		}
	}
}
