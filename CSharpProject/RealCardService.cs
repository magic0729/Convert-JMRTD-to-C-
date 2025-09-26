using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using org.jmrtd.CustomJavaAPI;

namespace org.jmrtd
{
    /// <summary>
    /// Real smart card service implementation - currently using enhanced mock for testing
    /// This will be replaced with actual PC/SC implementation when hardware is available
    /// </summary>
    public class RealCardService : ICardService
    {
        private MockCardService mockService;
        private string readerName;
        private bool isOpen;
        private List<IAPDUListener> apduListeners;
        private byte[] atr;
        private bool connectionLost;

        public RealCardService(string readerName)
        {
            this.readerName = readerName ?? throw new ArgumentNullException(nameof(readerName));
            this.mockService = new MockCardService();
            this.apduListeners = new List<IAPDUListener>();
            this.isOpen = false;
            this.connectionLost = false;
            this.atr = new byte[] { 0x3B, 0x7F, 0x18, 0x00, 0x00, 0x00, 0x31, 0xC0, 0x73, 0x9E, 0x01, 0x0B, 0x64, 0x52, 0xD9, 0x04, 0x00, 0x82, 0x90, 0x00, 0x88 };
        }

        public void Open()
        {
            try
            {
                mockService.Open();
                isOpen = true;
                connectionLost = false;
                Console.WriteLine($"Connected to reader: {readerName}");
                Console.WriteLine($"ATR: {BitConverter.ToString(atr)}");
                Console.WriteLine("Note: Using enhanced mock service for demonstration");
            }
            catch (Exception ex)
            {
                throw new CardServiceException($"Failed to open card service: {ex.Message}", ex);
            }
        }

        public void Close()
        {
            try
            {
                mockService.Close();
                isOpen = false;
                connectionLost = true;
                Console.WriteLine("Card service closed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error closing card service: {ex.Message}");
            }
        }

        public bool IsOpen()
        {
            return isOpen && mockService.IsOpen();
        }

        public ResponseAPDU Transmit(CommandAPDU commandAPDU)
        {
            if (!IsOpen())
            {
                throw new CardServiceException("Card service is not open");
            }

            try
            {
                // Use mock service for transmission
                var responseAPDU = mockService.Transmit(commandAPDU);
                
                // Notify listeners after transmission
                foreach (var listener in apduListeners)
                {
                    var apduEvent = new APDUEvent(this, null, 0, commandAPDU, responseAPDU);
                    listener.ExchangedAPDU(apduEvent);
                }

                return responseAPDU;
            }
            catch (Exception ex)
            {
                connectionLost = true;
                throw new CardServiceException($"Transmission failed: {ex.Message}", ex);
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
            return connectionLost || !IsOpen();
        }

        public bool IsExtendedAPDULengthSupported()
        {
            return true;
        }

        /// <summary>
        /// Get list of available smart card readers (simulated)
        /// </summary>
        public static List<string> GetAvailableReaders()
        {
            var readers = new List<string>();
            
            try
            {
                // Simulate available readers
                readers.Add("ACS ACR122U PICC Interface 0");
                readers.Add("ACS ACR122U PICC Interface 1");
                readers.Add("Generic USB Smart Card Reader");
                
                Console.WriteLine("Note: Reader list is simulated for demonstration");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting readers: {ex.Message}");
            }
            
            return readers;
        }

        /// <summary>
        /// Check if a specific reader has a card inserted (simulated)
        /// </summary>
        public static bool HasCard(string readerName)
        {
            try
            {
                // Simulate card presence
                Console.WriteLine($"Checking for card in reader: {readerName}");
                Console.WriteLine("Note: Card presence is simulated for demonstration");
                return true; // Always return true for demo
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Wait for card insertion with timeout (simulated)
        /// </summary>
        public static bool WaitForCard(string readerName, int timeoutMs = 10000)
        {
            Console.WriteLine($"Waiting for card in reader: {readerName}");
            Console.WriteLine("Note: Card detection is simulated for demonstration");
            Thread.Sleep(1000); // Simulate wait time
            return true; // Always return true for demo
        }
    }

    /// <summary>
    /// Exception thrown by card service operations
    /// </summary>
    public class CardServiceException : Exception
    {
        public CardServiceException(string message) : base(message) { }
        public CardServiceException(string message, Exception innerException) : base(message, innerException) { }
    }
}
