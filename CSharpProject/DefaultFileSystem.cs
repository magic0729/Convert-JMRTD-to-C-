using System;
using System.Collections.Generic;
using System.IO;
using org.jmrtd.CustomJavaAPI;
using org.jmrtd.io;
using org.jmrtd.lds;

namespace org.jmrtd
{
    public class DefaultFileSystem : IFileSystemStructured
    {
        public const int NO_SFI = -1;
        private const int READ_AHEAD_LENGTH = 8;

        private short selectedFID;
        private readonly bool isSFIEnabled;
        private int maxReadBinaryLength;
        private bool isSelected;
        private readonly IAPDULevelReadBinaryCapable service;
        private readonly Dictionary<short, DefaultFileInfo> fileInfos;
        private readonly Dictionary<short, byte> fidToSFI;
        private IAPDUWrapper? wrapper;
        private IAPDUWrapper? oldWrapper;

        public DefaultFileSystem(IAPDULevelReadBinaryCapable service, bool isSFIEnabled)
            : this(service, isSFIEnabled, LDSFileUtil.FID_TO_SFI)
        {
        }

        public DefaultFileSystem(IAPDULevelReadBinaryCapable service, bool isSFIEnabled, Dictionary<short, byte> fidToSFI)
        {
            this.service = service ?? throw new ArgumentNullException(nameof(service));
            this.fileInfos = new Dictionary<short, DefaultFileInfo>();
            this.selectedFID = 0;
            this.isSelected = false;
            this.isSFIEnabled = isSFIEnabled;
            this.fidToSFI = fidToSFI ?? throw new ArgumentNullException(nameof(fidToSFI));
            this.maxReadBinaryLength = 65536;
        }

        public void SetWrapper(IAPDUWrapper? wrapper)
        {
            this.oldWrapper = this.wrapper;
            this.wrapper = wrapper;
        }

        public IAPDUWrapper? GetWrapper() => wrapper;

        public int GetMaxReadBinaryLength() => maxReadBinaryLength;

        public IFileInfo[] GetSelectedPath()
        {
            var fileInfo = GetFileInfo();
            if (fileInfo == null) return Array.Empty<IFileInfo>();
            return new IFileInfo[] { fileInfo };
        }

        public void SelectFile(short fid)
        {
            if (selectedFID == fid) return;
            selectedFID = fid;
            isSelected = false;
        }

        public byte[] ReadBinary(int offset, int length)
        {
            DefaultFileInfo? fileInfo = null;
            try
            {
                if (selectedFID <= 0)
                {
                    throw new CardServiceException("No file selected");
                }

                fileInfo = GetFileInfo();
                if (fileInfo == null)
                {
                    throw new InvalidOperationException("Could not get file info");
                }

                length = Math.Min(length, maxReadBinaryLength);
                var fragment = fileInfo.GetSmallestUnbufferedFragment(offset, length);
                int responseLength = length;
                byte[]? bytes = null;

                if (fragment.Length > 0)
                {
                    if (isSFIEnabled && offset < 256)
                    {
                        if (!fidToSFI.TryGetValue(selectedFID, out byte sfi))
                        {
                            throw new FormatException($"Unknown FID {selectedFID.ToString("X4")}");
                        }
                        bytes = SendReadBinary(0x80 | sfi, fragment.Offset, fragment.Length, false);
                        isSelected = true;
                    }
                    else
                    {
                        if (!isSelected)
                        {
                            SendSelectFile(selectedFID);
                            isSelected = true;
                        }
                        bytes = SendReadBinary(fragment.Offset, fragment.Length, offset > short.MaxValue);
                    }

                    if (bytes == null)
                    {
                        throw new InvalidOperationException("Could not read bytes");
                    }

                    if (bytes.Length > 0)
                    {
                        fileInfo.AddFragment(fragment.Offset, bytes);
                    }

                    if (bytes.Length < fragment.Length)
                    {
                        responseLength = bytes.Length;
                    }
                }

                var buffer = fileInfo.GetBuffer();
                var result = new byte[responseLength];
                Buffer.BlockCopy(buffer, offset, result, 0, responseLength);
                return result;
            }
            catch (CardServiceException cse)
            {
                short sw = 0; // Simplified - no SW available
                if ((sw & 0x6700) == 26368 && maxReadBinaryLength > 223)
                {
                    wrapper = oldWrapper;
                    maxReadBinaryLength = 223;
                    return Array.Empty<byte>();
                }
                throw new CardServiceException($"Read binary failed on file {(fileInfo?.ToString() ?? selectedFID.ToString("X4"))}", cse);
            }
            catch (Exception e)
            {
                throw new CardServiceException($"Read binary failed on file {(fileInfo?.ToString() ?? selectedFID.ToString("X4"))}", e);
            }
        }

        private DefaultFileInfo? GetFileInfo()
        {
            if (selectedFID <= 0)
            {
                throw new CardServiceException("No file selected");
            }

            if (fileInfos.TryGetValue(selectedFID, out var fileInfo))
            {
                return fileInfo;
            }

            try
            {
                byte[]? prefix = null;
                if (isSFIEnabled)
                {
                    if (!fidToSFI.TryGetValue(selectedFID, out byte sfi))
                    {
                        throw new FormatException($"Unknown FID {selectedFID.ToString("X4")}");
                    }
                    prefix = SendReadBinary(0x80 | sfi, 0, 8, false);
                    isSelected = true;
                }
                else
                {
                    if (!isSelected)
                    {
                        SendSelectFile(selectedFID);
                        isSelected = true;
                    }
                    prefix = SendReadBinary(0, 8, false);
                }

                if (prefix == null || prefix.Length == 0)
                {
                    // Log warning: "Something is wrong with prefix"
                    return null;
                }

                int fileLength = GetFileLength(selectedFID, 8, prefix);
                if (fileLength < prefix.Length)
                {
                    prefix = new byte[fileLength];
                    Buffer.BlockCopy(prefix, 0, prefix, 0, fileLength);
                }

                fileInfo = new DefaultFileInfo(selectedFID, fileLength);
                fileInfo.AddFragment(0, prefix);
                fileInfos[selectedFID] = fileInfo;
                return fileInfo;
            }
            catch (IOException ioe)
            {
                throw new CardServiceException($"Error getting file info for {selectedFID.ToString("X4")}", ioe);
            }
        }

        private static int GetFileLength(short fid, int le, byte[] prefix)
        {
            if (prefix.Length < le) return prefix.Length;

            using var byteArrayInputStream = new MemoryStream(prefix);
            // TODO: Implement TLV parsing when TLV support is available
            // For now, return a default length
            return prefix.Length;
        }

        public void SendSelectFile(short fid)
        {
            service.SendSelectFile(wrapper, fid);
        }

        public byte[] SendReadBinary(int offset, int le, bool isTLVEncodedOffsetNeeded)
        {
            oldWrapper = wrapper is SecureMessagingWrapper smw ? SecureMessagingWrapper.GetInstance(smw.GetEncryptionKey(), smw.GetMACKey(), smw.GetMaxTranceiveLength(), smw.ShouldCheckMAC(), smw.GetSendSequenceCounter()) : wrapper;
            return service.SendReadBinary(wrapper, -1, offset, le, false, isTLVEncodedOffsetNeeded);
        }

        public byte[] SendReadBinary(int sfi, int offset, int le, bool isTLVEncodedOffsetNeeded)
        {
            return service.SendReadBinary(wrapper, sfi, offset, le, true, isTLVEncodedOffsetNeeded);
        }

        private class DefaultFileInfo : IFileInfo
        {
            private readonly short fid;
            private readonly FragmentBuffer buffer;

            public DefaultFileInfo(short fid, int length)
            {
                this.fid = fid;
                this.buffer = new FragmentBuffer(length);
            }

            public byte[] GetBuffer() => buffer.GetBuffer();
            public short GetFID() => fid;
            public int GetFileLength() => buffer.GetLength();

            public override string ToString() => fid.ToString("X4");

            public FragmentBuffer.Fragment GetSmallestUnbufferedFragment(int offset, int length)
            {
                return buffer.GetSmallestUnbufferedFragment(offset, length);
            }

            public void AddFragment(int offset, byte[] bytes)
            {
                buffer.AddFragment(offset, bytes);
            }
        }

        public Stream GetInputStream()
        {
            // TODO: Implement proper input stream
            return new MemoryStream();
        }
    }
}
