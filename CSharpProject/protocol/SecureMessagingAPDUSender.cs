using System;
using System.Collections.Generic;
using org.jmrtd;
using org.jmrtd.CustomJavaAPI;

namespace org.jmrtd.protocol
{
	public class SecureMessagingAPDUSender
	{
		private readonly ICardService service;
		private int apduCount;

		public SecureMessagingAPDUSender(ICardService service)
		{
			this.service = service;
			this.apduCount = 0;
		}

		public ResponseAPDU transmit(APDUWrapper? wrapper, CommandAPDU commandAPDU)
		{
			var plainCapdu = commandAPDU;
			if (wrapper != null)
			{
				commandAPDU = wrapper.Wrap(commandAPDU);
			}
			var rawRapdu = service.Transmit(commandAPDU);
			var responseAPDU = rawRapdu;
			int sw = responseAPDU.StatusWord;
			if (wrapper != null)
			{
				try
				{
					if ((sw & 0x6700) == 0x6700)
					{
						return responseAPDU;
					}
					if (responseAPDU.Bytes.Length <= 2)
					{
						throw new Exception($"Exception during transmission of wrapped APDU, C={BitConverter.ToString(plainCapdu.Bytes)}");
					}
					responseAPDU = wrapper.Unwrap(responseAPDU);
				}
				finally
				{
					apduCount++;
				}
			}
			else
			{
				apduCount++;
			}
			return responseAPDU;
		}

		public bool isExtendedAPDULengthSupported() => service.IsExtendedAPDULengthSupported();
		public void addAPDUListener(IAPDUListener l) => service.AddAPDUListener(l);
		public void removeAPDUListener(IAPDUListener l) => service.RemoveAPDUListener(l);
		public ICollection<IAPDUListener> getAPDUListeners() => service.GetAPDUListeners();
	}
}


