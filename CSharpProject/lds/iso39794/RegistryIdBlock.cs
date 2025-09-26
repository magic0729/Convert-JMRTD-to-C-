using System;

namespace org.jmrtd.lds.iso39794
{
	public class RegistryIdBlock
	{
		public int OwnerId { get; }
		public int TypeId { get; }

		public RegistryIdBlock(int ownerId, int typeId)
		{
			OwnerId = ownerId;
			TypeId = typeId;
		}

		public override string ToString() => $"Owner={OwnerId}, Type={TypeId}";
	}
}
