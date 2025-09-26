using System;
using System.Collections.Generic;

namespace org.jmrtd.lds
{
	public abstract class AbstractListInfo<T> : AbstractLDSInfo where T : AbstractLDSInfo
	{
		protected readonly List<T> elements;

		protected AbstractListInfo()
		{
			elements = new List<T>();
		}

		public IReadOnlyList<T> GetElements() => elements.AsReadOnly();
	}
}
