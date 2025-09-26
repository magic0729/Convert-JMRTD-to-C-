using System;
using System.IO;

namespace org.jmrtd.lds
{
    public abstract class DataGroup : AbstractLDSFile
    {
        protected DataGroup(short dataGroupNumber) : base(dataGroupNumber) { }
        protected DataGroup(short dataGroupNumber, Stream inputStream) : base(dataGroupNumber, inputStream) { }

        public short GetDataGroupNumber() => GetTag();
    }
}
