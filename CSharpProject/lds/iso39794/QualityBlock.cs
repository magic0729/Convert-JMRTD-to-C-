using System;

namespace org.jmrtd.lds.iso39794
{
	public class QualityBlock
	{
		public int Score { get; }
		public string Algorithm { get; }

		public QualityBlock(int score, string algorithm)
		{
			Score = score;
			Algorithm = algorithm;
		}

		public override string ToString() => $"{Algorithm}:{Score}";
	}
}
