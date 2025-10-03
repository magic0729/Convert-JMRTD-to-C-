using System;
using System.Collections.Generic;
using System.Linq;
using PCSC;
using PCSC.Iso7816;
using org.jmrtd.CustomJavaAPI;

namespace org.jmrtd.card
{
    /// <summary>
    /// PC/SC-based card service implementation for real smart card communication
    /// </summary>
    public class PCSCCardService : ICardService
    {
        private readonly string readerName;
        private readonly List<IAPDUListener> apduListeners;
        private ISCardContext? context;
        private ICardReader? reader;
        private bool isOpen;
        private byte[]? atr;
        private int sequenceNumber;

        public PCSCCardService(string readerName)
        {
            this.readerName = readerName ?? throw new ArgumentNullException(nameof(readerName));
            this.apduListeners = new List<IAPDUListener>();
            this.isOpen = false;
            this.sequenceNumber = 0;
        }

        public void Open()
        {
            if (isOpen) return;

            try
            {
                // Establish PC/SC context
                context = ContextFactory.Instance.Establish(SCardScope.System);
                
                // Connect to the specified reader
                reader = context.ConnectReader(readerName, SCardShareMode.Shared, SCardProtocol.Any);
                
                if (!reader.IsConnected)
                {
                    throw new CardServiceException($"Failed to connect to reader: {readerName}");
                }

                // Get ATR
                atr = reader.GetAttrib(SCardAttribute.AtrString);
                
                isOpen = true;
                Console.WriteLine($"Connected to PC/SC reader: {readerName}");
                Console.WriteLine($"ATR: {(atr != null ? BitConverter.ToString(atr) : "N/A")}");
            }
            catch (Exception ex)
            {
                Close();
                throw new CardServiceException($"Failed to open PC/SC card service: {ex.Message}", ex);
            }
        }

        public void Close()
        {
            try
            {
                reader?.Disconnect(SCardReaderDisposition.Leave);
                reader?.Dispose();
                reader = null;

                context?.Release();
                context?.Dispose();
                context = null;

                isOpen = false;
                Console.WriteLine("PC/SC card service closed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error closing PC/SC card service: {ex.Message}");
            }
        }

        public bool IsOpen()
        {
            return isOpen && reader?.IsConnected == true;
        }

        public ResponseAPDU Transmit(CommandAPDU command)
        {
            if (!IsOpen())
            {
                throw new CardServiceException("PC/SC card service is not open");
            }

            if (reader == null)
            {
                throw new CardServiceException("No reader connected");
            }

            try
            {
                // Convert CommandAPDU to raw bytes
                byte[] commandBytes = command.Bytes;
                
                // Transmit using PC/SC
                var receiveBuffer = new byte[256]; // Standard response buffer size
                int bytesReceived = reader.Transmit(commandBytes, receiveBuffer);
                
                // Create response array with actual received bytes
                byte[] responseBytes = new byte[bytesReceived];
                Array.Copy(receiveBuffer, responseBytes, bytesReceived);
                
                // Create ResponseAPDU from raw bytes
                var responseAPDU = new ResponseAPDU(responseBytes);
                
                // Notify APDU listeners
                NotifyAPDUListeners(command, responseAPDU);
                
                return responseAPDU;
            }
            catch (Exception ex)
            {
                throw new CardServiceException($"PC/SC transmission failed: {ex.Message}", ex);
            }
        }

        public void AddAPDUListener(IAPDUListener listener)
        {
            if (listener != null && !apduListeners.Contains(listener))
            {
                apduListeners.Add(listener);
            }
        }

        public void RemoveAPDUListener(IAPDUListener listener)
        {
            apduListeners.Remove(listener);
        }

        public ICollection<IAPDUListener> GetAPDUListeners()
        {
            return new List<IAPDUListener>(apduListeners);
        }

        public byte[] GetATR()
        {
            return atr?.ToArray() ?? Array.Empty<byte>();
        }

        public bool IsConnectionLost()
        {
            try
            {
                return !IsOpen() || reader?.IsConnected != true;
            }
            catch
            {
                return true;
            }
        }

        public bool IsExtendedAPDULengthSupported()
        {
            // Most modern PC/SC readers support extended APDU lengths
            return true;
        }

        private void NotifyAPDUListeners(CommandAPDU command, ResponseAPDU response)
        {
            if (apduListeners.Count == 0) return;

            var apduEvent = new APDUEvent(this, "PC/SC", ++sequenceNumber, command, response);
            
            foreach (var listener in apduListeners)
            {
                try
                {
                    listener.ExchangedAPDU(apduEvent);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error notifying APDU listener: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Get list of available PC/SC readers
        /// </summary>
        public static List<string> GetAvailableReaders()
        {
            var readers = new List<string>();
            
            try
            {
                using var context = ContextFactory.Instance.Establish(SCardScope.System);
                var readerNames = context.GetReaders();
                readers.AddRange(readerNames);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting PC/SC readers: {ex.Message}");
            }
            
            return readers;
        }

        /// <summary>
        /// Check if a specific reader has a card inserted
        /// </summary>
        public static bool HasCard(string readerName)
        {
            try
            {
                using var context = ContextFactory.Instance.Establish(SCardScope.System);
                using var reader = context.ConnectReader(readerName, SCardShareMode.Shared, SCardProtocol.Any);
                return reader.IsConnected;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Wait for card insertion with timeout
        /// </summary>
        public static bool WaitForCard(string readerName, int timeoutMs = 10000)
        {
            try
            {
                // Simple implementation - just check if card is present
                var startTime = DateTime.Now;
                while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
                {
                    if (HasCard(readerName))
                        return true;
                    
                    System.Threading.Thread.Sleep(100);
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Exception thrown by PC/SC card service operations
    /// </summary>
    public class CardServiceException : Exception
    {
        public CardServiceException(string message) : base(message) { }
        public CardServiceException(string message, Exception innerException) : base(message, innerException) { }
    }
}

