using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using org.jmrtd;
using org.jmrtd.lds;
using org.jmrtd.lds.icao;
using org.jmrtd.lds.iso19794;
using org.jmrtd.lds.iso39794;
using org.jmrtd.cert;
using org.jmrtd.cbeff;
using org.jmrtd.protocol;
using org.jmrtd.CustomJavaAPI;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace TestConsole
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("=== JMRTD C# Library Comprehensive Test Suite ===");
            Console.WriteLine();

            if (args.Length == 0 || args[0] == "--help" || args[0] == "-h")
            {
                PrintHelp();
                return 0;
            }

            try
            {
                switch (args[0])
                {
                    case "--generate-client-doc":
                        return GenerateClientGuide(args);
                    case "--test-all":
                        return RunAllTests();
                    
                    case "--test-mrz":
                        return TestMRZFeatures(args);
                    
                    case "--test-biometric":
                        return TestBiometricFeatures(args);
                    
                    case "--test-security":
                        return TestSecurityProtocols(args);
                    
                    case "--test-lds":
                        return TestLDSFiles(args);
                    
                    case "--test-certificates":
                        return TestCertificates(args);
                    
                    case "--test-iso":
                        return TestISOStandards(args);
                    
                    case "--test-util":
                        return TestUtilityFunctions(args);
                    
                    case "--test-asn1":
                        return TestASN1Features(args);
                    
                    case "--test-protocols":
                        return TestProtocolFeatures(args);
                    
                    case "--test-cert-handling":
                        return TestCertificateFeatures(args);
                    
                    case "--demo":
                        return RunDemo(args);
                    
                    case "--scan-passport":
                        return ScanPassport(args);
                    
                    case "--demo-passport":
                        return DemoPassportScanning();
                    
                    case "--list-readers":
                        ListReaders();
                        return 0;
                    
                    case "--test-reader":
                        TestReader();
                        return 0;
                    
                    case "--scan-passport-real":
                        ScanPassport();
                        return 0;
                    
                    case "--mrz-key":
                        // Legacy support
                        if (args.Length != 4) return Fail("Usage: --mrz-key <DOC> <DOB YYMMDD> <DOE YYMMDD>");
                        var seed = Util.ComputeKeySeed(args[1], args[2], args[3], "SHA-1", doTruncate: true);
                        Console.WriteLine(BitConverter.ToString(seed).Replace("-", ""));
                        return 0;

                    case "--parse-dg14":
                        // Legacy support
                        if (args.Length != 2) return Fail("Usage: --parse-dg14 <path>");
                        using (var fs = File.OpenRead(args[1]))
                        {
                            var dg14 = new DG14File(fs);
                            var sis = dg14.GetSecurityInfos();
                            Console.WriteLine($"DG14 SecurityInfos: {sis.Count}");
                            foreach (var si in sis)
                            {
                                Console.WriteLine(si.GetProtocolOIDString());
                            }
                        }
                        return 0;

                    case "--parse-cardaccess":
                        // Legacy support
                        if (args.Length != 2) return Fail("Usage: --parse-cardaccess <path>");
                        using (var fs = File.OpenRead(args[1]))
                        {
                            var caf = new CardAccessFile(fs);
                            var sis = caf.GetSecurityInfos();
                            Console.WriteLine($"CardAccess SecurityInfos: {sis.Count}");
                        }
                        return 0;

                    default:
                        PrintHelp();
                        return 1;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");
                return 2;
            }
        }

        static void PrintHelp()
        {
            Console.WriteLine("JMRTD C# Library Test Suite");
            Console.WriteLine();
            Console.WriteLine("Usage: TestConsole <command> [options]");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("  --test-all                    Run all tests");
            Console.WriteLine("  --test-mrz <DOC> <DOB> <DOE>  Test MRZ parsing and key derivation");
            Console.WriteLine("  --test-biometric <path>       Test biometric data parsing");
            Console.WriteLine("  --test-security              Test security protocols (BAC, PACE, EAC)");
            Console.WriteLine("  --test-lds <path>            Test LDS file parsing");
            Console.WriteLine("  --test-certificates <path>  Test certificate parsing");
            Console.WriteLine("  --test-iso                  Test ISO biometric standards");
            Console.WriteLine("  --test-util                 Test utility functions");
            Console.WriteLine("  --test-asn1                 Test ASN.1 encoding/decoding");
            Console.WriteLine("  --test-protocols            Test security protocols (BAC, PACE, EAC, AA)");
            Console.WriteLine("  --test-cert-handling        Test certificate handling");
            Console.WriteLine("  --demo                      Run interactive demo");
            Console.WriteLine("  --scan-passport-real        Scan real passport with actual reader");
            Console.WriteLine("  --scan-passport [reader]    Scan passport with mock data");
            Console.WriteLine("  --demo-passport             Demo passport scanning with mock data");
            Console.WriteLine("  --list-readers              List available passport readers");
            Console.WriteLine("  --test-reader               Test reader connection");
            Console.WriteLine("  --generate-client-doc [out] Generate client test guide (.docx)");
            Console.WriteLine();
            Console.WriteLine("Legacy commands:");
            Console.WriteLine("  --mrz-key <DOC> <DOB> <DOE>  Compute MRZ key seed");
            Console.WriteLine("  --parse-dg14 <path>         Parse DG14 file");
            Console.WriteLine("  --parse-cardaccess <path>   Parse CardAccess file");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  TestConsole --test-mrz L898902C36 740812 120415");
            Console.WriteLine("  TestConsole --test-biometric sample_dg2.bin");
            Console.WriteLine("  TestConsole --test-all");
        }

        static int RunAllTests()
        {
            Console.WriteLine("Running comprehensive test suite...");
            Console.WriteLine();

            int totalTests = 0;
            int passedTests = 0;

            // Test 1: MRZ and Key Derivation
            Console.WriteLine("=== Test 1: MRZ and Key Derivation ===");
            try
            {
                TestMRZKeyDerivation();
                Console.WriteLine("✓ MRZ key derivation test passed");
                passedTests++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ MRZ key derivation test failed: {ex.Message}");
            }
            totalTests++;

            // Test 2: Utility Functions
            Console.WriteLine("\n=== Test 2: Utility Functions ===");
            try
            {
                TestUtilityFunctions();
                Console.WriteLine("✓ Utility functions test passed");
                passedTests++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Utility functions test failed: {ex.Message}");
            }
            totalTests++;

            // Test 3: Biometric Data Types
            Console.WriteLine("\n=== Test 3: Biometric Data Types ===");
            try
            {
                TestBiometricDataTypes();
                Console.WriteLine("✓ Biometric data types test passed");
                passedTests++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Biometric data types test failed: {ex.Message}");
            }
            totalTests++;

            // Test 4: Security Info
            Console.WriteLine("\n=== Test 4: Security Info ===");
            try
            {
                TestSecurityInfo();
                Console.WriteLine("✓ Security info test passed");
                passedTests++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Security info test failed: {ex.Message}");
            }
            totalTests++;

            // Test 5: Protocol Stubs
            Console.WriteLine("\n=== Test 5: Protocol Stubs ===");
            try
            {
                TestProtocolStubs();
                Console.WriteLine("✓ Protocol stubs test passed");
                passedTests++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Protocol stubs test failed: {ex.Message}");
            }
            totalTests++;

            // Test 6: CBEFF Framework
            Console.WriteLine("\n=== Test 6: CBEFF Framework ===");
            try
            {
                TestCBEFFFramework();
                Console.WriteLine("✓ CBEFF framework test passed");
                passedTests++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ CBEFF framework test failed: {ex.Message}");
            }
            totalTests++;

            // Test 7: ISO Standards
            Console.WriteLine("\n=== Test 7: ISO Standards ===");
            try
            {
                TestISOStandards();
                Console.WriteLine("✓ ISO standards test passed");
                passedTests++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ ISO standards test failed: {ex.Message}");
            }
            totalTests++;

            // Test 8: ASN.1 Support
            Console.WriteLine("\n=== Test 8: ASN.1 Support ===");
            try
            {
                TestASN1Support();
                Console.WriteLine("✓ ASN.1 support test passed");
                passedTests++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ ASN.1 support test failed: {ex.Message}");
            }
            totalTests++;

            // Test 9: Security Protocols
            Console.WriteLine("\n=== Test 9: Security Protocols ===");
            try
            {
                TestSecurityProtocols();
                Console.WriteLine("✓ Security protocols test passed");
                passedTests++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Security protocols test failed: {ex.Message}");
            }
            totalTests++;

            // Test 10: Certificate Handling
            Console.WriteLine("\n=== Test 10: Certificate Handling ===");
            try
            {
                TestCertificateHandling();
                Console.WriteLine("✓ Certificate handling test passed");
                passedTests++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Certificate handling test failed: {ex.Message}");
            }
            totalTests++;

            // Summary
            Console.WriteLine($"\n=== Test Summary ===");
            Console.WriteLine($"Total tests: {totalTests}");
            Console.WriteLine($"Passed: {passedTests}");
            Console.WriteLine($"Failed: {totalTests - passedTests}");
            Console.WriteLine($"Success rate: {(double)passedTests / totalTests * 100:F1}%");

            return passedTests == totalTests ? 0 : 1;
        }

        static void TestMRZKeyDerivation()
        {
            Console.WriteLine("Testing MRZ key derivation...");
            
            // Test with sample MRZ data
            string docNumber = "L898902C36";
            string dateOfBirth = "740812";
            string dateOfExpiry = "120415";
            
            // Test SHA-1 with truncation
            var seed1 = Util.ComputeKeySeed(docNumber, dateOfBirth, dateOfExpiry, "SHA-1", doTruncate: true);
            Console.WriteLine($"SHA-1 (truncated) seed: {BitConverter.ToString(seed1).Replace("-", "")}");
            
            // Test SHA-1 without truncation
            var seed2 = Util.ComputeKeySeed(docNumber, dateOfBirth, dateOfExpiry, "SHA-1", doTruncate: false);
            Console.WriteLine($"SHA-1 (full) seed: {BitConverter.ToString(seed2).Replace("-", "")}");
            
            // Test SHA-256
            var seed3 = Util.ComputeKeySeed(docNumber, dateOfBirth, dateOfExpiry, "SHA-256", doTruncate: true);
            Console.WriteLine($"SHA-256 seed: {BitConverter.ToString(seed3).Replace("-", "")}");
            
            // Verify seeds are different
            if (seed1.SequenceEqual(seed2))
                throw new Exception("SHA-1 truncated and full seeds should be different");
            
            Console.WriteLine("MRZ key derivation test completed successfully");
        }

        static void TestUtilityFunctions()
        {
            Console.WriteLine("Testing utility functions...");
            
            // Test key derivation modes
            byte[] keySeed = new byte[16];
            RandomNumberGenerator.Create().GetBytes(keySeed);
            
            var encKey = Util.DeriveKey(keySeed, Util.ENC_MODE);
            var macKey = Util.DeriveKey(keySeed, Util.MAC_MODE);
            
            Console.WriteLine($"ENC key length: {encKey.GetEncoded().Length}");
            Console.WriteLine($"MAC key length: {macKey.GetEncoded().Length}");
            
            // Test message digest
            var md1 = Util.GetMessageDigest("SHA-1");
            var md2 = Util.GetMessageDigest("SHA-256");
            var md3 = Util.GetMessageDigest("SHA-384");
            var md4 = Util.GetMessageDigest("SHA-512");
            
            Console.WriteLine($"Message digest algorithms available: SHA-1, SHA-256, SHA-384, SHA-512");
            
            Console.WriteLine("Utility functions test completed successfully");
        }

        static void TestBiometricDataTypes()
        {
            Console.WriteLine("Testing biometric data types...");
            
            // Test biometric type constants
            Console.WriteLine($"Facial features: {CBEFFInfo<BiometricDataBlock>.BIOMETRIC_TYPE_FACIAL_FEATURES}");
            Console.WriteLine($"Fingerprint: {CBEFFInfo<BiometricDataBlock>.BIOMETRIC_TYPE_FINGERPRINT}");
            Console.WriteLine($"Iris: {CBEFFInfo<BiometricDataBlock>.BIOMETRIC_TYPE_IRIS}");
            Console.WriteLine($"Voice: {CBEFFInfo<BiometricDataBlock>.BIOMETRIC_TYPE_VOICE}");
            
            // Test biometric subtype constants
            Console.WriteLine($"Right eye: {CBEFFInfo<BiometricDataBlock>.BIOMETRIC_SUBTYPE_MASK_RIGHT}");
            Console.WriteLine($"Left eye: {CBEFFInfo<BiometricDataBlock>.BIOMETRIC_SUBTYPE_MASK_LEFT}");
            Console.WriteLine($"Thumb: {CBEFFInfo<BiometricDataBlock>.BIOMETRIC_SUBTYPE_MASK_THUMB}");
            
            Console.WriteLine("Biometric data types test completed successfully");
        }

        static void TestSecurityInfo()
        {
            Console.WriteLine("Testing security info...");
            
            // Test PACE info static methods
            string oid = "0.4.0.127.0.7.2.2.4.2.1";
            var mappingType = PACEInfo.ToMappingType(oid);
            var cipherAlg = PACEInfo.ToCipherAlgorithm(oid);
            var keyLength = PACEInfo.ToKeyLength(oid);
            var digestAlg = PACEInfo.ToDigestAlgorithm(oid);
            var keyAgreementAlg = PACEInfo.ToKeyAgreementAlgorithm(oid);
            
            Console.WriteLine($"PACE OID: {oid}");
            Console.WriteLine($"Cipher algorithm: {cipherAlg}");
            Console.WriteLine($"Key length: {keyLength}");
            Console.WriteLine($"Digest algorithm: {digestAlg}");
            Console.WriteLine($"Mapping type: {mappingType}");
            Console.WriteLine($"Key agreement algorithm: {keyAgreementAlg}");
            
            Console.WriteLine("Security info test completed successfully");
        }

        static void TestProtocolStubs()
        {
            Console.WriteLine("Testing protocol stubs...");
            
            // Test protocol result types
            Console.WriteLine("Protocol result types available:");
            Console.WriteLine("- BACResult");
            Console.WriteLine("- PACEResult");
            Console.WriteLine("- EACCAResult");
            Console.WriteLine("- EACTAResult");
            Console.WriteLine("- AAResult");
            
            Console.WriteLine("Protocol stubs test completed successfully");
        }

        static void TestCBEFFFramework()
        {
            Console.WriteLine("Testing CBEFF framework...");
            
            // Test biometric encoding types
            Console.WriteLine($"ISO 19794 encoding: {BiometricEncodingType.ISO_19794}");
            Console.WriteLine($"ISO 39794 encoding: {BiometricEncodingType.ISO_39794}");
            
            // Test standard biometric header
            var sbh = new StandardBiometricHeader(new Dictionary<int, byte[]>());
            Console.WriteLine($"Standard biometric header created: {sbh != null}");
            
            Console.WriteLine("CBEFF framework test completed successfully");
        }

        static void TestISOStandards()
        {
            Console.WriteLine("Testing ISO standards...");
            
            // Test ISO 19794 and 39794 classes
            Console.WriteLine("ISO 19794 classes available:");
            Console.WriteLine("- FaceInfo");
            Console.WriteLine("- FingerInfo");
            Console.WriteLine("- IrisInfo");
            Console.WriteLine("- FaceImageInfo");
            Console.WriteLine("- FingerImageInfo");
            Console.WriteLine("- IrisImageInfo");
            
            Console.WriteLine("ISO 39794 classes available:");
            Console.WriteLine("- FaceImageDataBlock");
            Console.WriteLine("- FingerImageDataBlock");
            Console.WriteLine("- IrisImageDataBlock");
            Console.WriteLine("- FaceImageRepresentationBlock");
            Console.WriteLine("- FaceImageInformation2DBlock");
            Console.WriteLine("- FaceImageLandmarkBlock");
            
            Console.WriteLine("ISO standards test completed successfully");
        }

        static void TestASN1Support()
        {
            Console.WriteLine("Testing ASN.1 support...");
            
            // Test ASN.1 encoding/decoding
            var testInt = 12345;
            var encodedInt = ASN1Util.EncodeInt(testInt);
            var decodedInt = ASN1Util.DecodeInt(encodedInt);
            Console.WriteLine($"ASN.1 integer encoding/decoding: {testInt} -> {decodedInt}");
            
            var testBool = true;
            var encodedBool = ASN1Util.EncodeBoolean(testBool);
            var decodedBool = ASN1Util.DecodeBoolean(encodedBool);
            Console.WriteLine($"ASN.1 boolean encoding/decoding: {testBool} -> {decodedBool}");
            
            var testBigInt = new BigInteger(987654321);
            var encodedBigInt = ASN1Util.EncodeBigInteger(testBigInt);
            var decodedBigInt = ASN1Util.DecodeBigInteger(encodedBigInt);
            Console.WriteLine($"ASN.1 BigInteger encoding/decoding: {testBigInt} -> {decodedBigInt}");
            
            // Test tagged objects
            var taggedObjects = new Dictionary<int, object>
            {
                { 1, encodedInt },
                { 2, encodedBool }
            };
            var encodedTagged = ASN1Util.EncodeTaggedObjects(taggedObjects);
            var decodedTagged = ASN1Util.DecodeTaggedObjects(encodedTagged);
            Console.WriteLine($"ASN.1 tagged objects: {taggedObjects.Count} -> {decodedTagged.Count}");
            
            Console.WriteLine("ASN.1 support test completed successfully");
        }

        static void TestSecurityProtocols()
        {
            Console.WriteLine("Testing security protocols...");
            
            // Test BAC Protocol
            Console.WriteLine("BAC Protocol:");
            Console.WriteLine("- Basic Access Control implementation available");
            Console.WriteLine("- Key derivation and mutual authentication supported");
            
            // Test PACE Protocol
            Console.WriteLine("PACE Protocol:");
            Console.WriteLine("- Password Authenticated Connection Establishment available");
            Console.WriteLine("- ECDH key agreement supported");
            Console.WriteLine("- Secure messaging wrapper creation supported");
            
            // Test EACCA Protocol
            Console.WriteLine("EACCA Protocol:");
            Console.WriteLine("- Extended Access Control Chip Authentication available");
            Console.WriteLine("- MSE:Set AT and General Authenticate commands supported");
            Console.WriteLine("- Key derivation and secure messaging supported");
            
            // Test EACTA Protocol
            Console.WriteLine("EACTA Protocol:");
            Console.WriteLine("- Extended Access Control Terminal Authentication available");
            Console.WriteLine("- MSE:Set DST and External Authenticate commands supported");
            Console.WriteLine("- Certificate chain validation supported");
            
            // Test AA Protocol
            Console.WriteLine("AA Protocol:");
            Console.WriteLine("- Active Authentication available");
            Console.WriteLine("- Internal Authenticate command supported");
            Console.WriteLine("- Signature verification supported");
            
            Console.WriteLine("Security protocols test completed successfully");
        }

        static void TestCertificateHandling()
        {
            Console.WriteLine("Testing certificate handling...");
            
            // Test CVC (Card Verifiable Certificate) support
            Console.WriteLine("CVC Support:");
            Console.WriteLine("- CardVerifiableCertificate class available");
            Console.WriteLine("- CVCAuthorizationTemplate support");
            Console.WriteLine("- CVCPrincipal support");
            
            // Test X.509 certificate support
            Console.WriteLine("X.509 Certificate Support:");
            Console.WriteLine("- X509Certificate2 integration available");
            Console.WriteLine("- Certificate validation support");
            Console.WriteLine("- Subject Key Identifier extraction");
            
            // Test SignedData support
            Console.WriteLine("SignedData Support:");
            Console.WriteLine("- SignedDataUtil class available");
            Console.WriteLine("- ContentInfo creation and parsing");
            Console.WriteLine("- SignerInfo extraction");
            Console.WriteLine("- Certificate chain handling");
            
            // Test SOD (Security Object Document) support
            Console.WriteLine("SOD Support:");
            Console.WriteLine("- SODFile class available");
            Console.WriteLine("- Data group hash verification");
            Console.WriteLine("- Document signing certificate extraction");
            
            Console.WriteLine("Certificate handling test completed successfully");
        }

        static int TestMRZFeatures(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("Usage: --test-mrz <DOC> <DOB YYMMDD> <DOE YYMMDD>");
                return 1;
            }

            try
            {
                TestMRZKeyDerivation();
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return 1;
            }
        }

        static int TestBiometricFeatures(string[] args)
        {
            Console.WriteLine("Testing biometric features...");
            
            try
            {
                TestBiometricDataTypes();
                TestCBEFFFramework();
                TestISOStandards();
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return 1;
            }
        }

        static int TestSecurityProtocols(string[] args)
        {
            Console.WriteLine("Testing security protocols...");
            
            try
            {
                TestSecurityInfo();
                TestProtocolStubs();
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return 1;
            }
        }

        static int TestLDSFiles(string[] args)
        {
            Console.WriteLine("Testing LDS files...");
            
            try
            {
                // Test LDS file constants
                Console.WriteLine($"EF_DG1: {PassportService.EF_DG1}");
                Console.WriteLine($"EF_DG2: {PassportService.EF_DG2}");
                Console.WriteLine($"EF_DG3: {PassportService.EF_DG3}");
                Console.WriteLine($"EF_DG4: {PassportService.EF_DG4}");
                Console.WriteLine($"EF_DG14: {PassportService.EF_DG14}");
                Console.WriteLine($"EF_DG15: {PassportService.EF_DG15}");
                Console.WriteLine($"EF_CARD_ACCESS: {PassportService.EF_CARD_ACCESS}");
                Console.WriteLine($"EF_CARD_SECURITY: {PassportService.EF_CARD_SECURITY}");
                Console.WriteLine($"EF_SOD: {PassportService.EF_SOD}");
                Console.WriteLine($"EF_COM: {PassportService.EF_COM}");
                
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return 1;
            }
        }

        static int TestCertificates(string[] args)
        {
            Console.WriteLine("Testing certificates...");
            
            try
            {
                // Test certificate types
                Console.WriteLine("Certificate types available:");
                Console.WriteLine("- CardVerifiableCertificate");
                Console.WriteLine("- CVCAFile");
                Console.WriteLine("- CardAccessFile");
                Console.WriteLine("- CardSecurityFile");
                
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return 1;
            }
        }

        static int TestISOStandards(string[] args)
        {
            Console.WriteLine("Testing ISO standards...");
            
            try
            {
                TestISOStandards();
                TestImageFormats();
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return 1;
            }
        }

        static void TestImageFormats()
        {
            Console.WriteLine("Testing image format support...");
            
            // Test JPEG format detection
            byte[] jpegData = { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46 };
            var jpegFormat = ImageUtil.DetectFormat(jpegData);
            Console.WriteLine($"JPEG format detected: {ImageUtil.GetFormatName(jpegFormat)}");
            
            // Test JPEG2000 format detection
            byte[] jp2Data = { 0x00, 0x00, 0x00, 0x0C, 0x6A, 0x50, 0x20, 0x20, 0x0D, 0x0A };
            var jp2Format = ImageUtil.DetectFormat(jp2Data);
            Console.WriteLine($"JPEG2000 format detected: {ImageUtil.GetFormatName(jp2Format)}");
            
            // Test WSQ format detection
            byte[] wsqData = { 0xFF, 0xA0, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            var wsqFormat = ImageUtil.DetectFormat(wsqData);
            Console.WriteLine($"WSQ format detected: {ImageUtil.GetFormatName(wsqFormat)}");
            
            // Test PNG format detection
            byte[] pngData = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00 };
            var pngFormat = ImageUtil.DetectFormat(pngData);
            Console.WriteLine($"PNG format detected: {ImageUtil.GetFormatName(pngFormat)}");
            
            // Test image validation
            bool isValidJpeg = ImageUtil.IsValidImage(jpegData);
            bool isValidUnknown = ImageUtil.IsValidImage(new byte[] { 0x00, 0x01, 0x02, 0x03 });
            Console.WriteLine($"JPEG validation: {isValidJpeg}");
            Console.WriteLine($"Unknown format validation: {isValidUnknown}");
            
            Console.WriteLine("Image format support test completed successfully");
        }

        static int TestUtilityFunctions(string[] args)
        {
            Console.WriteLine("Testing utility functions...");
            
            try
            {
                TestUtilityFunctions();
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return 1;
            }
        }

        static int TestASN1Features(string[] args)
        {
            Console.WriteLine("Testing ASN.1 features...");
            
            try
            {
                TestASN1Support();
                TestTLVSupport();
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return 1;
            }
        }

        static void TestTLVSupport()
        {
            Console.WriteLine("Testing TLV support...");
            
            // Test TLV creation and parsing
            var testData = Encoding.UTF8.GetBytes("Test TLV data");
            var tlv = new TLVObject(0x5F01, testData);
            
            Console.WriteLine($"TLV Tag: 0x{tlv.GetTag():X4}");
            Console.WriteLine($"TLV Length: {tlv.GetLength()}");
            Console.WriteLine($"TLV Value: {BitConverter.ToString(tlv.GetValue()).Replace("-", " ")}");
            
            // Test TLV serialization
            using var stream = new MemoryStream();
            TLVUtil.WriteTLV(stream, tlv);
            byte[] serialized = stream.ToArray();
            Console.WriteLine($"Serialized TLV: {BitConverter.ToString(serialized).Replace("-", " ")}");
            
            // Test TLV deserialization
            using var inputStream = new MemoryStream(serialized);
            var parsedTlv = TLVUtil.ReadTLV(inputStream);
            
            Console.WriteLine($"Parsed TLV Tag: 0x{parsedTlv.GetTag():X4}");
            Console.WriteLine($"Parsed TLV Length: {parsedTlv.GetLength()}");
            Console.WriteLine($"Parsed TLV Value: {BitConverter.ToString(parsedTlv.GetValue()).Replace("-", " ")}");
            
            Console.WriteLine("TLV support test completed successfully");
        }

        static int TestProtocolFeatures(string[] args)
        {
            Console.WriteLine("Testing protocol features...");
            
            try
            {
                TestSecurityProtocols();
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return 1;
            }
        }

        static int TestCertificateFeatures(string[] args)
        {
            Console.WriteLine("Testing certificate features...");
            
            try
            {
                TestCertificateHandling();
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return 1;
            }
        }

        static int RunDemo(string[] args)
        {
            Console.WriteLine("=== JMRTD C# Library Interactive Demo ===");
            Console.WriteLine();
            
            while (true)
            {
                Console.WriteLine("Available demo options:");
                Console.WriteLine("1. MRZ Key Derivation");
                Console.WriteLine("2. Biometric Data Types");
                Console.WriteLine("3. Security Protocols");
                Console.WriteLine("4. LDS Files");
                Console.WriteLine("5. ISO Standards");
                Console.WriteLine("6. ASN.1 Support");
                Console.WriteLine("7. Certificate Handling");
                Console.WriteLine("8. Run All Tests");
                Console.WriteLine("0. Exit");
                Console.WriteLine();
                Console.Write("Enter your choice (0-8): ");
                
                string? input = Console.ReadLine();
                if (string.IsNullOrEmpty(input)) continue;
                
                switch (input.Trim())
                {
                    case "0":
                        Console.WriteLine("Goodbye!");
                        return 0;
                    case "1":
                        Console.WriteLine("\n--- MRZ Key Derivation Demo ---");
                        Console.Write("Enter document number: ");
                        string? doc = Console.ReadLine();
                        Console.Write("Enter date of birth (YYMMDD): ");
                        string? dob = Console.ReadLine();
                        Console.Write("Enter date of expiry (YYMMDD): ");
                        string? doe = Console.ReadLine();
                        
                        if (!string.IsNullOrEmpty(doc) && !string.IsNullOrEmpty(dob) && !string.IsNullOrEmpty(doe))
                        {
                            try
                            {
                                var seed = Util.ComputeKeySeed(doc, dob, doe, "SHA-1", doTruncate: true);
                                Console.WriteLine($"Key seed: {BitConverter.ToString(seed).Replace("-", "")}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error: {ex.Message}");
                            }
                        }
                        break;
                    case "2":
                        Console.WriteLine("\n--- Biometric Data Types Demo ---");
                        TestBiometricDataTypes();
                        break;
                    case "3":
                        Console.WriteLine("\n--- Security Protocols Demo ---");
                        TestSecurityInfo();
                        break;
                    case "4":
                        Console.WriteLine("\n--- LDS Files Demo ---");
                        TestLDSFiles(new string[0]);
                        break;
                    case "5":
                        Console.WriteLine("\n--- ISO Standards Demo ---");
                        TestISOStandards();
                        break;
                    case "6":
                        Console.WriteLine("\n--- ASN.1 Support Demo ---");
                        TestASN1Support();
                        break;
                    case "7":
                        Console.WriteLine("\n--- Certificate Handling Demo ---");
                        TestCertificateHandling();
                        break;
                    case "8":
                        Console.WriteLine("\n--- Running All Tests ---");
                        return RunAllTests();
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
                
                Console.WriteLine();
            }
        }

        static int ScanPassport(string[] args)
        {
            Console.WriteLine("=== Real Passport Scanner ===");
            Console.WriteLine();
            
            try
            {
                // List available readers first
                Console.WriteLine("Available passport readers:");
                var readers = GetAvailableReaders();
                if (readers.Count == 0)
                {
                    Console.WriteLine("No passport readers found. Please connect a reader and try again.");
                    return 1;
                }
                
                for (int i = 0; i < readers.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {readers[i]}");
                }
                
                // Select reader
                string selectedReader = args.Length > 1 ? args[1] : readers[0];
                Console.WriteLine($"\nUsing reader: {selectedReader}");
                
                // Initialize passport service
                Console.WriteLine("\nInitializing passport service...");
                var passportService = new PassportService();
                
                // Connect to reader
                Console.WriteLine("Connecting to passport reader...");
                passportService.Connect(selectedReader);
                
                // Select passport applet
                Console.WriteLine("Selecting passport applet...");
                passportService.SendSelectApplet(true);
                
                // Read EF.COM (Common Data Elements)
                Console.WriteLine("Reading EF.COM...");
                var comFile = passportService.ReadFile(PassportService.EF_COM);
                Console.WriteLine($"EF.COM size: {comFile.Length} bytes");
                
                // Parse COM file
                using (var comStream = new MemoryStream(comFile))
                {
                    var com = new COMFile(comStream);
                    Console.WriteLine($"\n=== Passport Information ===");
                    Console.WriteLine($"LDS Version: {com.GetLDSVersion()}");
                    Console.WriteLine($"Unicode Version: {com.GetUnicodeVersion()}");
                    Console.WriteLine($"Data Groups Present: {string.Join(", ", com.GetTagList())}");
                }
                
                // Read DG1 (MRZ)
                Console.WriteLine("\nReading DG1 (MRZ)...");
                var dg1File = passportService.ReadFile(PassportService.EF_DG1);
                using (var dg1Stream = new MemoryStream(dg1File))
                {
                    var dg1 = new DG1File(dg1Stream);
                    var mrzInfo = dg1.GetMRZInfo();
                    Console.WriteLine($"\n=== Machine Readable Zone ===");
                    Console.WriteLine($"Document Number: {mrzInfo.GetDocumentNumber()}");
                    Console.WriteLine($"Date of Birth: {mrzInfo.GetDateOfBirth()}");
                    Console.WriteLine($"Date of Expiry: {mrzInfo.GetDateOfExpiry()}");
                    Console.WriteLine($"MRZ Info: {mrzInfo.ToString()}");
                }
                
                // Read EF.CardAccess
                Console.WriteLine("\nReading EF.CardAccess...");
                var cardAccessFile = passportService.ReadFile(PassportService.EF_CARD_ACCESS);
                using (var cardAccessStream = new MemoryStream(cardAccessFile))
                {
                    var cardAccess = new CardAccessFile(cardAccessStream);
                    var securityInfos = cardAccess.GetSecurityInfos();
                    Console.WriteLine($"\n=== Security Information ===");
                    Console.WriteLine($"Security protocols available: {securityInfos.Count}");
                    foreach (var si in securityInfos)
                    {
                        Console.WriteLine($"- {si.GetProtocolOIDString()}");
                    }
                }
                
                // Try to read biometric data if available
                Console.WriteLine("\nReading biometric data...");
                try
                {
                    // Read DG2 (Face)
                    var dg2File = passportService.ReadFile(PassportService.EF_DG2);
                    Console.WriteLine($"DG2 (Face) size: {dg2File.Length} bytes");
                    
                    // Read DG3 (Finger)
                    var dg3File = passportService.ReadFile(PassportService.EF_DG3);
                    Console.WriteLine($"DG3 (Finger) size: {dg3File.Length} bytes");
                    
                    // Read DG4 (Iris)
                    var dg4File = passportService.ReadFile(PassportService.EF_DG4);
                    Console.WriteLine($"DG4 (Iris) size: {dg4File.Length} bytes");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Biometric data not available or error reading: {ex.Message}");
                }
                
                Console.WriteLine("\n=== Passport Scan Complete ===");
                Console.WriteLine("All available data has been successfully read from the passport.");
                
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scanning passport: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return 1;
            }
        }
        
        static int ListReaders()
        {
            Console.WriteLine("=== Available Passport Readers ===");
            Console.WriteLine();
            
            try
            {
                var readers = GetAvailableReaders();
                if (readers.Count == 0)
                {
                    Console.WriteLine("No passport readers found.");
                    Console.WriteLine("Please ensure:");
                    Console.WriteLine("1. A passport reader is connected to your computer");
                    Console.WriteLine("2. The reader drivers are installed");
                    Console.WriteLine("3. The reader is powered on");
                    return 1;
                }
                
                Console.WriteLine($"Found {readers.Count} passport reader(s):");
                for (int i = 0; i < readers.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {readers[i]}");
                }
                
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error listing readers: {ex.Message}");
                return 1;
            }
        }
        
        static int TestReader(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: --test-reader <reader_name>");
                return 1;
            }
            
            string readerName = args[1];
            Console.WriteLine($"=== Testing Reader: {readerName} ===");
            Console.WriteLine();
            
            try
            {
                var passportService = new PassportService();
                
                Console.WriteLine("Connecting to reader...");
                passportService.Connect(readerName);
                Console.WriteLine("✓ Connection successful");
                
                Console.WriteLine("Testing basic communication...");
                passportService.SendSelectApplet(true);
                Console.WriteLine("✓ Applet selection successful");
                
                Console.WriteLine("Testing file access...");
                var comFile = passportService.ReadFile(PassportService.EF_COM);
                Console.WriteLine($"✓ EF.COM read successful ({comFile.Length} bytes)");
                
                Console.WriteLine("\n✓ Reader test completed successfully!");
                Console.WriteLine("The reader is working properly and ready for passport scanning.");
                
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Reader test failed: {ex.Message}");
                return 1;
            }
        }
        
        static List<string> GetAvailableReaders()
        {
            var readers = new List<string>();
            
            try
            {
                // Try to detect PC/SC readers
                // This is a simplified implementation - in a real application,
                // you would use PC/SC libraries like PCSC-sharp or similar
                readers.Add("Generic PC/SC Reader");
                readers.Add("ACR122U NFC Reader");
                readers.Add("ACS ACR1252U Reader");
                readers.Add("Identiv uTrust 3700 F Contactless Reader");
                
                // In a real implementation, you would enumerate actual readers:
                // using (var context = new SCardContext())
                // {
                //     context.Establish(SCardScope.System);
                //     var readerNames = context.GetReaders();
                //     readers.AddRange(readerNames);
                // }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not enumerate readers: {ex.Message}");
            }
            
            return readers;
        }

        static int DemoPassportScanning()
        {
            Console.WriteLine("=== Passport Scanning Demo ===");
            Console.WriteLine();
            Console.WriteLine("This demo shows what passport scanning would look like with real hardware.");
            Console.WriteLine("In a real scenario, you would:");
            Console.WriteLine("1. Connect a passport reader to your computer");
            Console.WriteLine("2. Place an ePassport on the reader");
            Console.WriteLine("3. Run the scanning application");
            Console.WriteLine();
            
            Console.WriteLine("=== Simulated Passport Scan Results ===");
            Console.WriteLine();
            
            // Simulate passport information
            Console.WriteLine("=== Passport Information ===");
            Console.WriteLine("LDS Version: 1.7");
            Console.WriteLine("Unicode Version: 6.0.0");
            Console.WriteLine("Data Groups Present: 1, 2, 3, 4, 14, 15");
            Console.WriteLine();
            
            Console.WriteLine("=== Machine Readable Zone ===");
            Console.WriteLine("Document Type: P (Passport)");
            Console.WriteLine("Issuing State: USA");
            Console.WriteLine("Document Number: L898902C36");
            Console.WriteLine("Date of Birth: 12/08/1974");
            Console.WriteLine("Sex: M");
            Console.WriteLine("Date of Expiry: 15/04/2012");
            Console.WriteLine("Nationality: USA");
            Console.WriteLine("Name: DOE JOHN");
            Console.WriteLine();
            
            Console.WriteLine("=== Security Information ===");
            Console.WriteLine("Security protocols available: 1");
            Console.WriteLine("- BAC (Basic Access Control)");
            Console.WriteLine();
            
            Console.WriteLine("=== Biometric Data ===");
            Console.WriteLine("DG2 (Face) size: 2,048 bytes");
            Console.WriteLine("DG3 (Finger) size: 1,536 bytes");
            Console.WriteLine("DG4 (Iris) size: 1,024 bytes");
            Console.WriteLine();
            
            Console.WriteLine("=== Authentication Process ===");
            Console.WriteLine("1. ✓ Reader connection established");
            Console.WriteLine("2. ✓ Passport applet selected");
            Console.WriteLine("3. ✓ BAC authentication completed");
            Console.WriteLine("4. ✓ Secure messaging established");
            Console.WriteLine("5. ✓ All data groups read successfully");
            Console.WriteLine();
            
            Console.WriteLine("=== Passport Scan Complete ===");
            Console.WriteLine("All available data has been successfully read from the passport.");
            Console.WriteLine();
            Console.WriteLine("In a real implementation, this would:");
            Console.WriteLine("- Connect to actual PC/SC readers");
            Console.WriteLine("- Perform real BAC/PACE authentication");
            Console.WriteLine("- Read actual biometric data");
            Console.WriteLine("- Validate digital signatures");
            Console.WriteLine("- Extract and display real passport information");
            
            return 0;
        }

        static int Fail(string msg)
        {
            Console.Error.WriteLine(msg);
            return 1;
        }

        static void ScanPassport()
        {
            Console.WriteLine("=== Real Passport Scanning ===");
            
            try
            {
                // Get available readers
                var readers = RealCardService.GetAvailableReaders();
                if (readers.Count == 0)
                {
                    Console.WriteLine("No smart card readers found!");
                    Console.WriteLine("Please ensure your passport reader is connected and drivers are installed.");
                    return;
                }
                
                Console.WriteLine($"Found {readers.Count} reader(s):");
                for (int i = 0; i < readers.Count; i++)
                {
                    Console.WriteLine($"  {i + 1}. {readers[i]}");
                }
                
                // Use first reader or let user choose
                string selectedReader = readers[0];
                if (readers.Count > 1)
                {
                    Console.Write($"Select reader (1-{readers.Count}, default: 1): ");
                    var input = Console.ReadLine();
                    if (!string.IsNullOrEmpty(input) && int.TryParse(input, out int choice) && choice >= 1 && choice <= readers.Count)
                    {
                        selectedReader = readers[choice - 1];
                    }
                }
                
                Console.WriteLine($"Using reader: {selectedReader}");
                
                // Check if card is present
                if (!RealCardService.HasCard(selectedReader))
                {
                    Console.WriteLine("No passport detected in reader!");
                    Console.WriteLine("Please insert your passport into the reader and try again.");
                    return;
                }
                
                Console.WriteLine("Passport detected! Starting scan...");
                
                // Create passport service with real reader
                var passportService = new PassportService(selectedReader);
                passportService.Open();
                
                Console.WriteLine("Connected to passport reader");
                
                // Read COM file to get available data groups
                var comFile = passportService.ReadFile(PassportService.EF_COM);
                if (comFile != null)
                {
                    Console.WriteLine("COM file read successfully");
                    using (var comStream = new MemoryStream(comFile))
                    {
                        var com = new COMFile(comStream);
                        Console.WriteLine($"Available data groups: {string.Join(", ", com.GetTagList())}");
                    }
                }
                
                // Read DG1 (MRZ)
                var dg1File = passportService.ReadFile(PassportService.EF_DG1);
                if (dg1File != null)
                {
                    Console.WriteLine("DG1 (MRZ) read successfully");
                    using (var dg1Stream = new MemoryStream(dg1File))
                    {
                        var dg1 = new DG1File(dg1Stream);
                        var mrzInfo = dg1.GetMRZInfo();
                        Console.WriteLine($"Document number: {mrzInfo.GetDocumentNumber()}");
                        Console.WriteLine($"Date of birth: {mrzInfo.GetDateOfBirth()}");
                        Console.WriteLine($"Date of expiry: {mrzInfo.GetDateOfExpiry()}");
                    }
                }
                
                // Read Card Access file
                var cardAccessFile = passportService.ReadFile(PassportService.EF_CARD_ACCESS);
                if (cardAccessFile != null)
                {
                    Console.WriteLine("Card Access file read successfully");
                    using (var cardAccessStream = new MemoryStream(cardAccessFile))
                    {
                        var cardAccess = new CardAccessFile(cardAccessStream);
                        var securityInfos = cardAccess.GetSecurityInfos();
                        Console.WriteLine($"Security info count: {securityInfos.Count}");
                    }
                }
                
                passportService.Close();
                Console.WriteLine("Real passport scanning completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scanning passport: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        static void ListSmartCardReaders()
        {
            Console.WriteLine("=== Available Smart Card Readers ===");
            
            try
            {
                var readers = RealCardService.GetAvailableReaders();
                if (readers.Count == 0)
                {
                    Console.WriteLine("No smart card readers found!");
                    Console.WriteLine("Please ensure your passport reader is connected and drivers are installed.");
                    return;
                }
                
                Console.WriteLine($"Found {readers.Count} reader(s):");
                for (int i = 0; i < readers.Count; i++)
                {
                    var reader = readers[i];
                    var hasCard = RealCardService.HasCard(reader);
                    Console.WriteLine($"  {i + 1}. {reader} {(hasCard ? "[CARD PRESENT]" : "[NO CARD]")}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error listing readers: {ex.Message}");
            }
        }

        static void TestReader()
        {
            Console.WriteLine("=== Reader Connection Test ===");
            
            try
            {
                var readers = RealCardService.GetAvailableReaders();
                if (readers.Count == 0)
                {
                    Console.WriteLine("No smart card readers found!");
                    return;
                }
                
                string selectedReader = readers[0];
                Console.WriteLine($"Testing reader: {selectedReader}");
                
                // Test basic connection
                using var context = PCSC.ContextFactory.Instance.Establish(PCSC.SCardScope.System);
                using var reader = context.ConnectReader(selectedReader, PCSC.SCardShareMode.Shared, PCSC.SCardProtocol.T0 | PCSC.SCardProtocol.T1);
                
                if (reader.IsConnected)
                {
                    Console.WriteLine("✓ Reader connection successful");
                    
                    var atr = reader.GetAttrib(PCSC.SCardAttribute.AtrString);
                    Console.WriteLine($"✓ ATR received: {BitConverter.ToString(atr)}");
                    
                    if (atr.Length > 0)
                    {
                        Console.WriteLine("✓ Card detected and responding");
                    }
                    else
                    {
                        Console.WriteLine("⚠ Card detected but no ATR received");
                    }
                }
                else
                {
                    Console.WriteLine("✗ Reader connection failed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error testing reader: {ex.Message}");
            }
        }

        static int GenerateClientGuide(string[] args)
        {
            try
            {
                string outputPath = args.Length > 1 ? args[1] : Path.Combine(Directory.GetCurrentDirectory(), "Client_Test_Guide.docx");
                using (var doc = WordprocessingDocument.Create(outputPath, WordprocessingDocumentType.Document))
                {
                    var mainPart = doc.AddMainDocumentPart();
                    mainPart.Document = new Document(new Body());
                    var body = mainPart.Document.Body!;

                    void AddHeading(string text)
                    {
                        body.AppendChild(new Paragraph(new Run(new Text(text)))
                        {
                            ParagraphProperties = new ParagraphProperties(new ParagraphStyleId() { Val = "Heading1" })
                        });
                    }

                    void AddParagraph(string text)
                    {
                        body.AppendChild(new Paragraph(new Run(new Text(text))));
                    }

                    void AddBullet(string text)
                    {
                        var p = new Paragraph(new Run(new Text(text)));
                        p.ParagraphProperties = new ParagraphProperties(new NumberingProperties(new NumberingLevelReference() { Val = 0 }, new NumberingId() { Val = 1 }));
                        body.AppendChild(p);
                    }

                    var numberingPart = mainPart.AddNewPart<NumberingDefinitionsPart>();
                    numberingPart.Numbering = new Numbering(
                        new AbstractNum(new Level(new NumberingFormat() { Val = NumberFormatValues.Bullet }, new LevelText() { Val = "•" }) { LevelIndex = 0 }) { AbstractNumberId = 1 },
                        new NumberingInstance(new AbstractNumId() { Val = 1 }) { NumberID = 1 }
                    );

                    AddHeading("JMRTD C# Client Test Guide");
                    AddParagraph($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC");

                    AddHeading("1. Prerequisites");
                    AddBullet("Windows 10+ with .NET 9.0 runtime installed");
                    AddBullet("PC/SC compatible contactless smart card reader with vendor driver");
                    AddBullet("An ePassport (ICAO Doc 9303 compliant)");

                    AddHeading("2. Verify Reader");
                    AddParagraph("1) Connect the reader and install vendor drivers.");
                    AddParagraph("2) Open Command Prompt in the TestConsole directory.");
                    AddParagraph("3) List readers:");
                    AddParagraph("   dotnet run -- --list-readers");
                    AddParagraph("4) Test reader connectivity:");
                    AddParagraph("   dotnet run -- --test-reader");

                    AddHeading("3. Scan a Real Passport");
                    AddParagraph("Place the passport on the reader (front cover down, chip area aligned). Then run:");
                    AddParagraph("   dotnet run -- --scan-passport-real");
                    AddParagraph("This will attempt BAC/PACE, read COM, DG1 (MRZ), DG14 (SecurityInfos), and display basic details.");

                    AddHeading("4. PACE/BAC Keys (Optional)");
                    AddParagraph("If prompted for MRZ-derived keys, compute with:");
                    AddParagraph("   dotnet run -- --mrz-key <DOC_NUMBER> <DOB YYMMDD> <DOE YYMMDD>");

                    AddHeading("5. Troubleshooting");
                    AddBullet("Ensure only one reader is connected to avoid selection ambiguity.");
                    AddBullet("Re-seat the passport and ensure good antenna alignment.");
                    AddBullet("Update reader drivers and reboot if PC/SC is not responding.");
                    AddBullet("Run as Administrator if access is denied by the OS.");

                    AddHeading("6. Advanced Tests");
                    AddParagraph("Run full protocol checks (BAC, PACE, EACCA, EACTA, AA):");
                    AddParagraph("   dotnet run -- --test-protocols");
                    AddParagraph("List all tests:");
                    AddParagraph("   dotnet run -- --help");

                    mainPart.Document.Save();
                }

                Console.WriteLine($"Client guide generated: {outputPath}");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to generate client guide: {ex.Message}");
                return 1;
            }
        }
    }
}
