namespace org.jmrtd
{
	public interface BACKeySpec : AccessKeySpec
	{
		string DocumentNumber { get; }
		string DateOfBirth { get; }
		string DateOfExpiry { get; }
	}
}


