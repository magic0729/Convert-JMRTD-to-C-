using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace org.jmrtd.CustomJavaAPI
{
    /// <summary>
    /// TLV (Tag-Length-Value) utility class for parsing ePassport data structures
    /// </summary>
    public static class TLVUtil
    {
        /// <summary>
        /// Read a TLV structure from input stream
        /// </summary>
        public static TLVObject ReadTLV(Stream inputStream)
        {
            if (inputStream == null) throw new ArgumentNullException(nameof(inputStream));
            
            int tag = ReadTag(inputStream);
            int length = ReadLength(inputStream);
            byte[] value = ReadValue(inputStream, length);
            
            return new TLVObject(tag, value);
        }
        
        /// <summary>
        /// Read a tag from input stream
        /// </summary>
        private static int ReadTag(Stream inputStream)
        {
            int firstByte = inputStream.ReadByte();
            if (firstByte == -1) throw new EndOfStreamException("Unexpected end of stream while reading tag");
            
            int tag = firstByte;
            
            // Handle multi-byte tags
            if ((firstByte & 0x1F) == 0x1F)
            {
                int nextByte;
                do
                {
                    nextByte = inputStream.ReadByte();
                    if (nextByte == -1) throw new EndOfStreamException("Unexpected end of stream while reading tag");
                    
                    tag = (tag << 8) | nextByte;
                } while ((nextByte & 0x80) != 0);
            }
            
            return tag;
        }
        
        /// <summary>
        /// Read length from input stream
        /// </summary>
        private static int ReadLength(Stream inputStream)
        {
            int firstByte = inputStream.ReadByte();
            if (firstByte == -1) throw new EndOfStreamException("Unexpected end of stream while reading length");
            
            if ((firstByte & 0x80) == 0)
            {
                // Short form
                return firstByte;
            }
            else
            {
                // Long form
                int numBytes = firstByte & 0x7F;
                if (numBytes == 0) throw new ArgumentException("Invalid length encoding");
                if (numBytes > 4) throw new ArgumentException("Length too large");
                
                int length = 0;
                for (int i = 0; i < numBytes; i++)
                {
                    int nextByte = inputStream.ReadByte();
                    if (nextByte == -1) throw new EndOfStreamException("Unexpected end of stream while reading length");
                    length = (length << 8) | nextByte;
                }
                
                return length;
            }
        }
        
        /// <summary>
        /// Read value from input stream
        /// </summary>
        private static byte[] ReadValue(Stream inputStream, int length)
        {
            if (length < 0) throw new ArgumentException("Invalid length");
            if (length == 0) return Array.Empty<byte>();
            
            byte[] value = new byte[length];
            int totalRead = 0;
            
            while (totalRead < length)
            {
                int bytesRead = inputStream.Read(value, totalRead, length - totalRead);
                if (bytesRead == 0) throw new EndOfStreamException("Unexpected end of stream while reading value");
                totalRead += bytesRead;
            }
            
            return value;
        }
        
        /// <summary>
        /// Write TLV structure to output stream
        /// </summary>
        public static void WriteTLV(Stream outputStream, TLVObject tlv)
        {
            if (outputStream == null) throw new ArgumentNullException(nameof(outputStream));
            if (tlv == null) throw new ArgumentNullException(nameof(tlv));
            
            WriteTag(outputStream, tlv.GetTag());
            WriteLength(outputStream, tlv.GetLength());
            WriteValue(outputStream, tlv.GetValue());
        }
        
        /// <summary>
        /// Write tag to output stream
        /// </summary>
        private static void WriteTag(Stream outputStream, int tag)
        {
            if (tag < 0x100)
            {
                // Single byte tag
                outputStream.WriteByte((byte)tag);
            }
            else
            {
                // Multi-byte tag
                var tagBytes = new List<byte>();
                while (tag > 0)
                {
                    tagBytes.Insert(0, (byte)(tag & 0xFF));
                    tag >>= 8;
                }
                
                // Set continuation bit on all but last byte
                for (int i = 0; i < tagBytes.Count - 1; i++)
                {
                    tagBytes[i] |= 0x80;
                }
                
                outputStream.Write(tagBytes.ToArray(), 0, tagBytes.Count);
            }
        }
        
        /// <summary>
        /// Write length to output stream
        /// </summary>
        private static void WriteLength(Stream outputStream, int length)
        {
            if (length < 0x80)
            {
                // Short form
                outputStream.WriteByte((byte)length);
            }
            else
            {
                // Long form
                var lengthBytes = new List<byte>();
                while (length > 0)
                {
                    lengthBytes.Insert(0, (byte)(length & 0xFF));
                    length >>= 8;
                }
                
                outputStream.WriteByte((byte)(0x80 | lengthBytes.Count));
                outputStream.Write(lengthBytes.ToArray(), 0, lengthBytes.Count);
            }
        }
        
        /// <summary>
        /// Write value to output stream
        /// </summary>
        private static void WriteValue(Stream outputStream, byte[] value)
        {
            if (value != null && value.Length > 0)
            {
                outputStream.Write(value, 0, value.Length);
            }
        }
        
        /// <summary>
        /// Find TLV with specific tag in a sequence
        /// </summary>
        public static TLVObject? FindTLV(Stream inputStream, int targetTag)
        {
            if (inputStream == null) throw new ArgumentNullException(nameof(inputStream));
            
            long originalPosition = inputStream.Position;
            
            try
            {
                while (inputStream.Position < inputStream.Length)
                {
                    long position = inputStream.Position;
                    TLVObject tlv = ReadTLV(inputStream);
                    
                    if (tlv.GetTag() == targetTag)
                    {
                        return tlv;
                    }
                }
                
                return null;
            }
            catch (EndOfStreamException)
            {
                return null;
            }
            finally
            {
                inputStream.Position = originalPosition;
            }
        }
        
        /// <summary>
        /// Parse TLV sequence into list
        /// </summary>
        public static List<TLVObject> ParseTLVSequence(Stream inputStream)
        {
            if (inputStream == null) throw new ArgumentNullException(nameof(inputStream));
            
            var tlvList = new List<TLVObject>();
            
            try
            {
                while (inputStream.Position < inputStream.Length)
                {
                    TLVObject tlv = ReadTLV(inputStream);
                    tlvList.Add(tlv);
                }
            }
            catch (EndOfStreamException)
            {
                // Expected when reaching end of stream
            }
            
            return tlvList;
        }
    }
}
