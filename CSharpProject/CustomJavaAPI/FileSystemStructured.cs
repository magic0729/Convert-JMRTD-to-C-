using System;

namespace org.jmrtd.CustomJavaAPI
{
    public interface IFileSystemStructured
    {
        IFileInfo[] GetSelectedPath();
        void SelectFile(short fid);
        byte[] ReadBinary(int offset, int length);
    }

    public interface IFileInfo
    {
        short GetFID();
        int GetFileLength();
    }
}
