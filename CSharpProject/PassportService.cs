using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using org.jmrtd.CustomJavaAPI;
using org.jmrtd.cert;
using org.jmrtd.protocol;

namespace org.jmrtd
{
    public class PassportService : AbstractMRTDCardService
    {
        public const byte NO_PACE_KEY_REFERENCE = 0;
        public const byte MRZ_PACE_KEY_REFERENCE = 1;
        public const byte CAN_PACE_KEY_REFERENCE = 2;
        public const byte PIN_PACE_KEY_REFERENCE = 3;
        public const byte PUK_PACE_KEY_REFERENCE = 4;

        public const short EF_CARD_ACCESS = 284;
        public const short EF_CARD_SECURITY = 285;
        public const short EF_DG1 = 257;
        public const short EF_DG2 = 258;
        public const short EF_DG3 = 259;
        public const short EF_DG4 = 260;
        public const short EF_DG5 = 261;
        public const short EF_DG6 = 262;
        public const short EF_DG7 = 263;
        public const short EF_DG8 = 264;
        public const short EF_DG9 = 265;
        public const short EF_DG10 = 266;
        public const short EF_DG11 = 267;
        public const short EF_DG12 = 268;
        public const short EF_DG13 = 269;
        public const short EF_DG14 = 270;
        public const short EF_DG15 = 271;
        public const short EF_DG16 = 272;
        public const short EF_SOD = 285;
        public const short EF_COM = 286;
        public const short EF_CVCA = 284;

        public const byte SFI_CARD_ACCESS = 28;
        public const byte SFI_CARD_SECURITY = 29;
        public const byte SFI_DG1 = 1;
        public const byte SFI_DG2 = 2;
        public const byte SFI_DG3 = 3;
        public const byte SFI_DG4 = 4;
        public const byte SFI_DG5 = 5;
        public const byte SFI_DG6 = 6;
        public const byte SFI_DG7 = 7;
        public const byte SFI_DG8 = 8;
        public const byte SFI_DG9 = 9;
        public const byte SFI_DG10 = 10;
        public const byte SFI_DG11 = 11;
        public const byte SFI_DG12 = 12;
        public const byte SFI_DG13 = 13;
        public const byte SFI_DG14 = 14;
        public const byte SFI_DG15 = 15;
        public const byte SFI_DG16 = 16;
        public const byte SFI_COM = 30;
        public const byte SFI_SOD = 29;
        public const byte SFI_CVCA = 28;

        public const int DEFAULT_MAX_BLOCKSIZE = 223;
        public const int NORMAL_MAX_TRANCEIVE_LENGTH = 256;
        public const int EXTENDED_MAX_TRANCEIVE_LENGTH = 65536;

        protected static readonly byte[] APPLET_AID = { 0xA0, 0x00, 0x00, 0x02, 0x47, 0x10, 0x01 };

        private readonly int maxBlockSize;
        private bool isOpen;
        private SecureMessagingWrapper? wrapper;
        private readonly int maxTranceiveLengthForSecureMessaging;
        private readonly int maxTranceiveLengthForPACEProtocol;
        private readonly bool shouldCheckMAC;
        private bool isAppletSelected;
        private readonly DefaultFileSystem rootFileSystem;
        private readonly DefaultFileSystem appletFileSystem;
        private readonly BACAPDUSender bacSender;
        private readonly PACEAPDUSender paceSender;
        private readonly AAAPDUSender aaSender;
        private readonly EACCAAPDUSender eacCASender;
        private readonly EACTAAPDUSender eacTASender;
        private readonly ReadBinaryAPDUSender readBinarySender;
        private readonly ICardService service;

        public PassportService(ICardService service, int maxTranceiveLengthForSecureMessaging, int maxBlockSize, bool isSFIEnabled, bool shouldCheckMAC)
            : this(service, 256, maxTranceiveLengthForSecureMessaging, maxBlockSize, isSFIEnabled, shouldCheckMAC)
        {
        }

        public PassportService() : this(new MockCardService(), 256, 256, 223, true, true)
        {
        }

        public PassportService(string readerName) : this(new RealCardService(readerName), 256, 256, 223, true, true)
        {
        }

        public PassportService(ICardService service, int maxTranceiveLengthForPACEProtocol, int maxTranceiveLengthForSecureMessaging, int maxBlockSize, bool isSFIEnabled, bool shouldCheckMAC)
        {
            this.service = service ?? throw new ArgumentNullException(nameof(service));
            this.bacSender = new BACAPDUSender(service);
            this.paceSender = new PACEAPDUSender(service);
            this.aaSender = new AAAPDUSender(service);
            this.eacCASender = new EACCAAPDUSender(service);
            this.eacTASender = new EACTAAPDUSender(service);
            this.readBinarySender = new ReadBinaryAPDUSender(service);
            this.maxTranceiveLengthForPACEProtocol = maxTranceiveLengthForPACEProtocol;
            this.maxTranceiveLengthForSecureMessaging = maxTranceiveLengthForSecureMessaging;
            this.maxBlockSize = maxBlockSize;
            this.shouldCheckMAC = shouldCheckMAC;
            this.isAppletSelected = false;
            this.isOpen = false;
            this.rootFileSystem = new DefaultFileSystem(this.readBinarySender, false);
            this.appletFileSystem = new DefaultFileSystem(this.readBinarySender, isSFIEnabled);
        }

        public override void Open()
        {
            if (IsOpen()) return;

            lock (this)
            {
                service.Open();
                isOpen = true;
            }
        }

        public override void SendSelectApplet(bool hasPACESucceeded)
        {
            if (isAppletSelected)
            {
                // Log: "Re-selecting ICAO applet"
            }

            if (hasPACESucceeded)
            {
                readBinarySender.SendSelectApplet(wrapper, APPLET_AID);
            }
            else
            {
                readBinarySender.SendSelectApplet(null, APPLET_AID);
            }
            isAppletSelected = true;
        }

        public override void SendSelectMF()
        {
            readBinarySender.SendSelectMF();
            wrapper = null;
        }

        public override bool IsOpen() => isOpen;

        public override BACResult DoBAC(IAccessKeySpec bacKey)
        {
            if (!(bacKey is IBACKeySpec))
            {
                throw new ArgumentException("Unsupported key type");
            }

            var bacResult = new BACProtocol(bacSender, maxTranceiveLengthForSecureMessaging, shouldCheckMAC).DoBAC(bacKey);
            wrapper = bacResult.Wrapper;
            appletFileSystem.SetWrapper(wrapper);
            return bacResult;
        }

        public override BACResult DoBAC(SecretKey kEnc, SecretKey kMac)
        {
            var bacResult = new BACProtocol(bacSender, maxTranceiveLengthForSecureMessaging, shouldCheckMAC).DoBAC(kEnc, kMac);
            wrapper = bacResult.Wrapper;
            appletFileSystem.SetWrapper(wrapper);
            return bacResult;
        }

        public override PACEResult DoPACE(IAccessKeySpec keySpec, string oid, object? parameters, BigInteger? parameterId)
        {
            // Auto-discover PACE domain parameters from EF.CardAccess or DG14 if not provided
            object? staticParams = parameters;
            BigInteger? paramId = parameterId;
            try
            {
                if (staticParams == null)
                {
                    var discovered = TryDiscoverPACEParameters();
                    if (discovered != null)
                    {
                        staticParams = discovered.Item1;
                        paramId = discovered.Item2;
                    }
                }
            }
            catch { }

            var paceResult = new PACEProtocol(paceSender, wrapper, maxTranceiveLengthForPACEProtocol, maxTranceiveLengthForSecureMessaging, shouldCheckMAC).DoPACE(keySpec, oid, staticParams, paramId);
            wrapper = paceResult.Wrapper;
            appletFileSystem.SetWrapper(wrapper);
            return paceResult;
        }

        private Tuple<object?, BigInteger?>? TryDiscoverPACEParameters()
        {
            // Prefer EF.CardAccess
            try
            {
                rootFileSystem.SelectFile(EF_CARD_ACCESS);
                var bytes = rootFileSystem.ReadBinary(0, rootFileSystem.GetMaxReadBinaryLength());
                if (bytes != null && bytes.Length > 0)
                {
                    using var ms = new MemoryStream(bytes);
                    var caf = new org.jmrtd.lds.CardAccessFile(ms);
                    foreach (var si in caf.GetSecurityInfos())
                    {
                        if (si is org.jmrtd.lds.PACEDomainParameterInfo pdi)
                        {
                            return Tuple.Create<object?, BigInteger?>(pdi, pdi.GetParameterId());
                        }
                    }
                }
            }
            catch { }

            // Fallback: DG14
            try
            {
                appletFileSystem.SelectFile(EF_DG14);
                var bytes = appletFileSystem.ReadBinary(0, appletFileSystem.GetMaxReadBinaryLength());
                if (bytes != null && bytes.Length > 0)
                {
                    using var ms = new MemoryStream(bytes);
                    var dg14 = new org.jmrtd.lds.icao.DG14File(ms);
                    foreach (var si in dg14.GetSecurityInfos())
                    {
                        if (si is org.jmrtd.lds.PACEDomainParameterInfo pdi)
                        {
                            return Tuple.Create<object?, BigInteger?>(pdi, pdi.GetParameterId());
                        }
                    }
                }
            }
            catch { }

            // Try EF.CardSecurity (SignedData)
            try
            {
                rootFileSystem.SelectFile(EF_CARD_SECURITY);
                var bytes = rootFileSystem.ReadBinary(0, rootFileSystem.GetMaxReadBinaryLength());
                if (bytes != null && bytes.Length > 0)
                {
                    using var ms = new MemoryStream(bytes);
                    var csf = new org.jmrtd.lds.CardSecurityFile(ms);
                    foreach (var si in csf.GetSecurityInfos())
                    {
                        if (si is org.jmrtd.lds.PACEDomainParameterInfo pdi)
                        {
                            return Tuple.Create<object?, BigInteger?>(pdi, pdi.GetParameterId());
                        }
                    }
                }
            }
            catch { }

            return null;
        }

        public override EACCAResult DoEACCA(BigInteger keyId, string chipAuthenticationAlgorithm, string keyAgreementAlgorithm, AsymmetricAlgorithm publicKey)
        {
            var caResult = new EACCAProtocol(eacCASender, GetWrapper(), maxTranceiveLengthForSecureMessaging, shouldCheckMAC).DoCA(keyId, chipAuthenticationAlgorithm, keyAgreementAlgorithm, publicKey);
            wrapper = caResult.Wrapper;
            appletFileSystem.SetWrapper(wrapper);
            return caResult;
        }

        public override EACTAResult DoEACTA(object cvcPrincipal, IList<object> cvcCertificates, AsymmetricAlgorithm terminalAuthenticationPrivateKey, string signatureAlgorithm, EACCAResult eaccaResult, string chipAuthenticationAlgorithm)
        {
            return new EACTAProtocol(eacTASender, GetWrapper()).DoEACTA(cvcPrincipal, cvcCertificates, terminalAuthenticationPrivateKey, signatureAlgorithm, eaccaResult, chipAuthenticationAlgorithm);
        }

        public override EACTAResult DoEACTA(object cvcPrincipal, IList<object> cvcCertificates, AsymmetricAlgorithm terminalAuthenticationPrivateKey, string signatureAlgorithm, EACCAResult eaccaResult, PACEResult paceResult)
        {
            return new EACTAProtocol(eacTASender, GetWrapper()).DoTA(cvcPrincipal, cvcCertificates, terminalAuthenticationPrivateKey, signatureAlgorithm, eaccaResult, paceResult);
        }

        public override AAResult DoAA(AsymmetricAlgorithm publicKey, string digestAlgorithm, string signatureAlgorithm, byte[] challenge)
        {
            return new AAProtocol(aaSender, GetWrapper()).DoAA(publicKey, digestAlgorithm, signatureAlgorithm, challenge);
        }

        public override void Close()
        {
            try
            {
                service.Close();
                wrapper = null;
            }
            finally
            {
                isOpen = false;
            }
        }

        public int GetMaxTranceiveLength() => maxTranceiveLengthForSecureMessaging;

        public override SecureMessagingWrapper GetWrapper()
        {
            var ldsSecureMessagingWrapper = appletFileSystem.GetWrapper();
            if (ldsSecureMessagingWrapper != null && (wrapper == null || ldsSecureMessagingWrapper.GetSendSequenceCounter() > wrapper.GetSendSequenceCounter()))
            {
                wrapper = ldsSecureMessagingWrapper as SecureMessagingWrapper;
            }
            return wrapper!;
        }

        public override ResponseAPDU Transmit(CommandAPDU commandAPDU)
        {
            return service.Transmit(commandAPDU);
        }

        public override byte[] GetATR()
        {
            return service.GetATR();
        }

        public override bool IsConnectionLost()
        {
            return service.IsConnectionLost();
        }

        public bool IsConnectionLost(Exception e)
        {
            return service.IsConnectionLost();
        }

        public bool ShouldCheckMAC() => shouldCheckMAC;

        public override CardFileInputStream GetInputStream(short fid)
        {
            return GetInputStream(fid, maxBlockSize);
        }

        public override CardFileInputStream GetInputStream(short fid, int maxBlockSize)
        {
            if (!isAppletSelected)
            {
                lock (rootFileSystem)
                {
                    rootFileSystem.SelectFile(fid);
                    return new CardFileInputStream(rootFileSystem.GetInputStream(), maxBlockSize);
                }
            }

            lock (appletFileSystem)
            {
                appletFileSystem.SelectFile(fid);
                return new CardFileInputStream(appletFileSystem.GetInputStream(), maxBlockSize);
            }
        }

        public override int GetMaxReadBinaryLength()
        {
            return maxTranceiveLengthForSecureMessaging;
        }

        public void Connect(string readerName)
        {
            // In a real implementation, this would connect to the specific reader
            // For now, we'll just open the service
            Open();
        }

        public byte[] ReadFile(short fid)
        {
            // If using mock service, return mock data
            if (service is MockCardService mockService)
            {
                return mockService.GetMockFileData(fid);
            }
            
            using (var inputStream = GetInputStream(fid))
            {
                using (var memoryStream = new MemoryStream())
                {
                    inputStream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }
        public override void AddAPDUListener(IAPDUListener l)
        {
            service.AddAPDUListener(l);
        }

        public override void RemoveAPDUListener(IAPDUListener l)
        {
            service.RemoveAPDUListener(l);
        }

        public override ICollection<IAPDUListener> GetAPDUListeners()
        {
            return service.GetAPDUListeners();
        }

        public override bool IsExtendedAPDULengthSupported()
        {
            return service.IsExtendedAPDULengthSupported();
        }

        protected void NotifyExchangedAPDU(APDUEvent apduEvent)
        {
            var apduListeners = GetAPDUListeners();
            if (apduListeners == null || apduListeners.Count == 0) return;

            foreach (var apduListener in apduListeners)
            {
                apduListener.ExchangedAPDU(apduEvent);
            }
        }
    }
}
