namespace org.jmrtd.protocol
{
	public class PACEMappingResult
	{
		public object EphemeralParameters { get; }
		public PACEMappingResult(object ephemeralParameters)
		{
			EphemeralParameters = ephemeralParameters;
		}
	}
}


