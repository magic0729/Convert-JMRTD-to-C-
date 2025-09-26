using org.jmrtd.CustomJavaAPI;
using System.Security.Cryptography;
using System;

namespace org.jmrtd.protocol
{
	public class AAAPDUSender
	{
		private readonly ICardService service;

		public AAAPDUSender(ICardService service)
		{
			this.service = service ?? throw new ArgumentNullException(nameof(service));
		}

		public byte[] SendInternalAuthenticate(byte[] command)
		{
			// Internal Authenticate command: 00 88 00 00 Lc Data Le
			var cmd = new CommandAPDU(0x00, 0x88, 0x00, 0x00, command, 0x00);
			var response = service.Transmit(cmd);
			if (response.StatusWord != 0x9000)
			{
				throw new CardServiceException($"APDU failed with SW=0x{response.StatusWord:X4}");
			}
			return response.Data ?? Array.Empty<byte>();
		}
	}

	public class EACCAAPDUSender
	{
		private readonly ICardService service;

		public EACCAAPDUSender(ICardService service)
		{
			this.service = service ?? throw new ArgumentNullException(nameof(service));
		}

		public byte[] SendMSESetAT(byte[] command)
		{
			// MSE:Set AT command: 00 22 C1 A4 Lc Data
			var cmd = new CommandAPDU(0x00, 0x22, 0xC1, 0xA4, command, 0x00);
			var response = service.Transmit(cmd);
			if (response.StatusWord != 0x9000)
			{
				throw new CardServiceException($"APDU failed with SW=0x{response.StatusWord:X4}");
			}
			return response.Data ?? Array.Empty<byte>();
		}

		public byte[] SendGeneralAuthenticate(byte[] command)
		{
			// General Authenticate command: 00 86 00 00 Lc Data Le
			var cmd = new CommandAPDU(0x00, 0x86, 0x00, 0x00, command, 0x00);
			var response = service.Transmit(cmd);
			if (response.StatusWord != 0x9000)
			{
				throw new CardServiceException($"APDU failed with SW=0x{response.StatusWord:X4}");
			}
			return response.Data ?? Array.Empty<byte>();
		}
	}

	public class EACTAAPDUSender
	{
		private readonly ICardService service;

		public EACTAAPDUSender(ICardService service)
		{
			this.service = service ?? throw new ArgumentNullException(nameof(service));
		}

		public byte[] SendMSESetDST(byte[] command)
		{
			// MSE:Set DST command: 00 22 C1 A4 Lc Data
			var cmd = new CommandAPDU(0x00, 0x22, 0xC1, 0xA4, command, 0x00);
			var response = service.Transmit(cmd);
			if (response.StatusWord != 0x9000)
			{
				throw new CardServiceException($"APDU failed with SW=0x{response.StatusWord:X4}");
			}
			return response.Data ?? Array.Empty<byte>();
		}

		public byte[] SendExternalAuthenticate(byte[] command)
		{
			// External Authenticate command: 00 82 00 00 Lc Data
			var cmd = new CommandAPDU(0x00, 0x82, 0x00, 0x00, command, 0x00);
			var response = service.Transmit(cmd);
			if (response.StatusWord != 0x9000)
			{
				throw new CardServiceException($"APDU failed with SW=0x{response.StatusWord:X4}");
			}
			return response.Data ?? Array.Empty<byte>();
		}
	}

	public class ReadBinaryAPDUSender : IAPDULevelReadBinaryCapable
	{
		private readonly ICardService service;

		public ReadBinaryAPDUSender(ICardService service)
		{
			this.service = service ?? throw new ArgumentNullException(nameof(service));
		}

		public void SendSelectApplet(IAPDUWrapper? wrapper, byte[] aid)
		{
			if (aid == null) throw new ArgumentNullException(nameof(aid));
			// 00 A4 04 00 Lc AID 00
			var cmd = new CommandAPDU(0x00, 0xA4, 0x04, 0x00, aid, 0x00);
			cmd = wrapper != null ? wrapper.Wrap(cmd) : cmd;
			var resp = service.Transmit(cmd);
			resp = wrapper != null ? wrapper.Unwrap(resp) : resp;
			EnsureSuccess(resp);
		}

		public void SendSelectMF()
		{
			// 00 A4 00 0C 02 3F 00
			var data = new byte[] { 0x3F, 0x00 };
			var cmd = new CommandAPDU(0x00, 0xA4, 0x00, 0x0C, data, 0x00);
			var resp = service.Transmit(cmd);
			EnsureSuccess(resp);
		}

		public void SendSelectFile(IAPDUWrapper? wrapper, short fid)
		{
			// 00 A4 02 0C 02 FIDhi FIDlo
			var data = new byte[] { (byte)((fid >> 8) & 0xFF), (byte)(fid & 0xFF) };
			var cmd = new CommandAPDU(0x00, 0xA4, 0x02, 0x0C, data, 0x00);
			cmd = wrapper != null ? wrapper.Wrap(cmd) : cmd;
			var resp = service.Transmit(cmd);
			resp = wrapper != null ? wrapper.Unwrap(resp) : resp;
			EnsureSuccess(resp);
		}

		public byte[] SendReadBinary(IAPDUWrapper? wrapper, int sfi, int offset, int length, bool isSFI, bool isTLVEncodedOffsetNeeded)
		{
			// Build READ BINARY command. INS=0xB0
			int p1, p2;
			if (isSFI)
			{
				p1 = 0x80 | (sfi & 0x1F);
				p2 = offset & 0xFF;
			}
			else
			{
				p1 = (offset >> 8) & 0x7F;
				p2 = offset & 0xFF;
			}
			// Some cards require TLV-encoded offset; here we use standard READ BINARY
			var cmd = new CommandAPDU(0x00, 0xB0, p1, p2, length);
			cmd = wrapper != null ? wrapper.Wrap(cmd) : cmd;
			var resp = service.Transmit(cmd);
			resp = wrapper != null ? wrapper.Unwrap(resp) : resp;
			EnsureSuccess(resp);
			return resp.Data ?? Array.Empty<byte>();
		}

		private static void EnsureSuccess(ResponseAPDU response)
		{
			if (response == null) throw new CardServiceException("Null response");
			int sw = response.StatusWord;
			if (sw != 0x9000)
			{
				throw new CardServiceException($"APDU failed with SW=0x{sw:X4}");
			}
		}
	}
}

