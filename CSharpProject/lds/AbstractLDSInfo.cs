using System;
using System.IO;

namespace org.jmrtd.lds
{
    public abstract class AbstractLDSInfo
    {
        public abstract void WriteObject(Stream outputStream);
    }
}
