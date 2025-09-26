using System;
using System.Collections.Generic;
using System.IO;
using org.jmrtd.CustomJavaAPI;

namespace org.jmrtd
{
    public class MockCardService : ICardService
    {
        private bool isOpen = false;
        private readonly List<IAPDUListener> listeners = new List<IAPDUListener>();
        private readonly Dictionary<short, byte[]> mockFiles = new Dictionary<short, byte[]>();

        public MockCardService()
        {
            InitializeMockPassportData();
        }

        private void InitializeMockPassportData()
        {
            // Mock EF.COM data
            var comData = CreateMockCOMFile();
            mockFiles[PassportService.EF_COM] = comData;

            // Mock DG1 (MRZ) data
            var dg1Data = CreateMockDG1File();
            mockFiles[PassportService.EF_DG1] = dg1Data;

            // Mock EF.CardAccess data
            var cardAccessData = CreateMockCardAccessFile();
            mockFiles[PassportService.EF_CARD_ACCESS] = cardAccessData;

            // Mock DG2 (Face) data
            var dg2Data = CreateMockDG2File();
            mockFiles[PassportService.EF_DG2] = dg2Data;

            // Mock DG3 (Finger) data
            var dg3Data = CreateMockDG3File();
            mockFiles[PassportService.EF_DG3] = dg3Data;

            // Mock DG4 (Iris) data
            var dg4Data = CreateMockDG4File();
            mockFiles[PassportService.EF_DG4] = dg4Data;
        }

        private byte[] CreateMockCOMFile()
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            
            // LDS Version (0x5F01): "0101"
            writer.Write((byte)0x5F);
            writer.Write((byte)0x01);
            writer.Write((byte)0x04);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("0101"));

            // Unicode Version (0x5F36): "060000"
            writer.Write((byte)0x5F);
            writer.Write((byte)0x36);
            writer.Write((byte)0x06);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("060000"));

            // Tag List (0x5C): DG1, DG2, DG3, DG4, DG14, DG15
            writer.Write((byte)0x5C);
            writer.Write((byte)0x06);
            writer.Write((byte)0x01); // DG1
            writer.Write((byte)0x02); // DG2
            writer.Write((byte)0x03); // DG3
            writer.Write((byte)0x04); // DG4
            writer.Write((byte)0x0E); // DG14
            writer.Write((byte)0x0F); // DG15

            return ms.ToArray();
        }

        private byte[] CreateMockDG1File()
        {
            // Mock MRZ data: TD3 format (2 lines of 44 characters each)
            string mrzLine1 = "P<USATEST<<JOHN<<DOE<<<<<<<<<<<<<<<<<<<<<<<<<<";
            string mrzLine2 = "L898902C36USA7408122M1204151<<<<<<<<<<<<<<<<<8";
            
            string mrzData = mrzLine1 + "\n" + mrzLine2;
            return System.Text.Encoding.ASCII.GetBytes(mrzData);
        }

        private byte[] CreateMockCardAccessFile()
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            
            // Mock BAC Security Info
            writer.Write((byte)0x80); // BAC Info tag
            writer.Write((byte)0x0E); // Length
            writer.Write((byte)0x0C); // OID length
            writer.Write(System.Text.Encoding.ASCII.GetBytes("0.4.0.127.0.7.2.2.1.1")); // BAC OID
            writer.Write((byte)0x02); // Version length
            writer.Write((byte)0x01); // Version 1

            return ms.ToArray();
        }

        private byte[] CreateMockDG2File()
        {
            // Mock face image data (simplified)
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            
            // Standard Biometric Header
            writer.Write((byte)0xA1); // Tag
            writer.Write((byte)0x10); // Length
            writer.Write((byte)0x02); // Format owner
            writer.Write((byte)0x01); // Format type
            writer.Write((byte)0x00); // Format version
            writer.Write((byte)0x02); // Biometric type (face)
            writer.Write((byte)0x00); // Biometric subtype
            writer.Write((byte)0x00); // Creation date/time
            writer.Write((byte)0x00);
            writer.Write((byte)0x00);
            writer.Write((byte)0x00);
            writer.Write((byte)0x00);
            writer.Write((byte)0x00);
            writer.Write((byte)0x00);
            writer.Write((byte)0x00);
            writer.Write((byte)0x00);
            writer.Write((byte)0x00);
            writer.Write((byte)0x00);

            // Mock face image data
            writer.Write((byte)0x87); // Face image tag
            writer.Write((byte)0x20); // Length
            for (int i = 0; i < 32; i++)
            {
                writer.Write((byte)(i % 256)); // Mock image data
            }

            return ms.ToArray();
        }

        private byte[] CreateMockDG3File()
        {
            // Mock finger image data (simplified)
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            
            // Standard Biometric Header
            writer.Write((byte)0xA1); // Tag
            writer.Write((byte)0x10); // Length
            writer.Write((byte)0x02); // Format owner
            writer.Write((byte)0x01); // Format type
            writer.Write((byte)0x00); // Format version
            writer.Write((byte)0x08); // Biometric type (finger)
            writer.Write((byte)0x04); // Biometric subtype (thumb)
            writer.Write((byte)0x00); // Creation date/time
            writer.Write((byte)0x00);
            writer.Write((byte)0x00);
            writer.Write((byte)0x00);
            writer.Write((byte)0x00);
            writer.Write((byte)0x00);
            writer.Write((byte)0x00);
            writer.Write((byte)0x00);
            writer.Write((byte)0x00);
            writer.Write((byte)0x00);
            writer.Write((byte)0x00);

            // Mock finger image data
            writer.Write((byte)0x87); // Finger image tag
            writer.Write((byte)0x20); // Length
            for (int i = 0; i < 32; i++)
            {
                writer.Write((byte)((i + 100) % 256)); // Mock image data
            }

            return ms.ToArray();
        }

        private byte[] CreateMockDG4File()
        {
            // Mock iris image data (simplified)
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            
            // Standard Biometric Header
            writer.Write((byte)0xA1); // Tag
            writer.Write((byte)0x10); // Length
            writer.Write((byte)0x02); // Format owner
            writer.Write((byte)0x01); // Format type
            writer.Write((byte)0x00); // Format version
            writer.Write((byte)0x10); // Biometric type (iris)
            writer.Write((byte)0x01); // Biometric subtype (right eye)
            writer.Write((byte)0x00); // Creation date/time
            writer.Write((byte)0x00);
            writer.Write((byte)0x00);
            writer.Write((byte)0x00);
            writer.Write((byte)0x00);
            writer.Write((byte)0x00);
            writer.Write((byte)0x00);
            writer.Write((byte)0x00);
            writer.Write((byte)0x00);
            writer.Write((byte)0x00);
            writer.Write((byte)0x00);

            // Mock iris image data
            writer.Write((byte)0x87); // Iris image tag
            writer.Write((byte)0x20); // Length
            for (int i = 0; i < 32; i++)
            {
                writer.Write((byte)((i + 200) % 256)); // Mock image data
            }

            return ms.ToArray();
        }

        public void Open()
        {
            isOpen = true;
        }

        public void Close()
        {
            isOpen = false;
        }

        public bool IsOpen()
        {
            return isOpen;
        }

        public ResponseAPDU Transmit(CommandAPDU command)
        {
            // Mock response - in a real implementation, this would communicate with actual hardware
            return new ResponseAPDU(new byte[] { 0x90, 0x00 }); // Success response
        }

        public void AddAPDUListener(IAPDUListener listener)
        {
            if (listener != null && !listeners.Contains(listener))
            {
                listeners.Add(listener);
            }
        }

        public void RemoveAPDUListener(IAPDUListener listener)
        {
            listeners.Remove(listener);
        }

        public ICollection<IAPDUListener> GetAPDUListeners()
        {
            return new List<IAPDUListener>(listeners);
        }

        public byte[] GetATR()
        {
            // Mock ATR for testing
            return new byte[] { 0x3B, 0x7F, 0x18, 0x00, 0x00, 0x00, 0x31, 0xC0, 0x73, 0x9E, 0x01, 0x0B, 0x64, 0x52, 0xD9, 0x04, 0x00, 0x82, 0x90, 0x00 };
        }

        public bool IsConnectionLost()
        {
            return !isOpen;
        }

        public bool IsExtendedAPDULengthSupported()
        {
            return true;
        }

        public byte[] GetMockFileData(short fid)
        {
            return mockFiles.TryGetValue(fid, out var data) ? data : new byte[0];
        }
    }
}
