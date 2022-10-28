using System;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography.X509Certificates;

namespace TBModExtensionHost.Tools
{
    internal class SignUtils
    {
        private static IClrStrongName clrStrongName = (IClrStrongName)RuntimeEnvironment.GetRuntimeInterfaceAsObject(new Guid("B79B0ACD-F5CD-409b-B5A5-A16244610B92"), new Guid("9FD93CCF-3280-4391-B3A9-96E1CDE77C8D"));
        private static X509Certificate2 rootCA = loadHostCACert();

        public SignUtils() { }

        public static bool isValidCert(X509Certificate2 cert)
        {
            X509Certificate2 signatureCA = getIssuer(cert);

            return signatureCA != null && signatureCA.Equals(rootCA) && signatureCA.SerialNumber.Equals(rootCA.SerialNumber)
                && signatureCA.Thumbprint.Equals(rootCA.Thumbprint);
        }

        public static bool checkSignature(string filePath)
        {
            Signature signature = getSignature(filePath);
            if (signature != null)
            {
                //if (signature.Status == SignatureStatus.Valid)
                //    return true;

                return isValidCert(signature.SignerCertificate);
            }

            return false;
        }

        public static bool checkFile(string filePath)
        {
            try
            {
                X509Certificate2 certFromFile = new X509Certificate2(X509Certificate.CreateFromSignedFile(filePath));
                return isValidCert(certFromFile);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static Signature getSignature(string filePath)
        {
            using (Runspace runspace = RunspaceFactory.CreateRunspace(RunspaceConfiguration.Create()))
            {
                runspace.Open();
                using (var pipeline = runspace.CreatePipeline())
                {
                    pipeline.Commands.AddScript("Get-AuthenticodeSignature -FilePath \"" + filePath + "\"");
                    var results = pipeline.Invoke();
                    runspace.Close();

                    var signature = results[0].BaseObject as Signature;
                    return signature == null || signature.SignerCertificate == null ? null : signature;
                }
            }
        }

        public static X509Certificate2 getIssuer(X509Certificate2 leafCert)
        {
            if (leafCert.Subject == leafCert.Issuer)
                return null;

            using (X509Chain chain = new X509Chain())
            {
                chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                chain.Build(leafCert);

                if (chain.ChainElements.Count > 1)
                    return chain.ChainElements[1].Certificate;
            }

            return null;
        }

        public static X509Certificate2 loadHostCACert()
        {
            X509Certificate2 cert = null;
            string resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames().FirstOrDefault(rn => rn.EndsWith("TacticalBaconRootCA.cer", StringComparison.CurrentCultureIgnoreCase));
            if (resourceName != null)
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    byte[] assemblyData = new byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);

                    cert = new X509Certificate2(assemblyData);
                }
            }

            return cert;
        }

        public static bool checkWithWindowsLib(string filePath)
        {
            int result = clrStrongName.StrongNameSignatureVerificationEx(filePath, true, out bool pfWasVerified);
            return result == 0 && pfWasVerified;
        }

        [ComConversionLoss, Guid("9FD93CCF-3280-4391-B3A9-96E1CDE77C8D"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SecurityCritical]
        [ComImport]
        private interface IClrStrongName
        {
            [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
            int GetHashFromAssemblyFile([MarshalAs(UnmanagedType.LPStr)][In] string pszFilePath, [MarshalAs(UnmanagedType.U4)][In][Out] ref int piHashAlg, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)][Out] byte[] pbHash, [MarshalAs(UnmanagedType.U4)][In] int cchHash, [MarshalAs(UnmanagedType.U4)] out int pchHash);

            [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
            int GetHashFromAssemblyFileW([MarshalAs(UnmanagedType.LPWStr)][In] string pwzFilePath, [MarshalAs(UnmanagedType.U4)][In][Out] ref int piHashAlg, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)][Out] byte[] pbHash, [MarshalAs(UnmanagedType.U4)][In] int cchHash, [MarshalAs(UnmanagedType.U4)] out int pchHash);

            [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
            int GetHashFromBlob([In] IntPtr pbBlob, [MarshalAs(UnmanagedType.U4)][In] int cchBlob, [MarshalAs(UnmanagedType.U4)][In][Out] ref int piHashAlg, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)][Out] byte[] pbHash, [MarshalAs(UnmanagedType.U4)][In] int cchHash, [MarshalAs(UnmanagedType.U4)] out int pchHash);

            [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
            int GetHashFromFile([MarshalAs(UnmanagedType.LPStr)][In] string pszFilePath, [MarshalAs(UnmanagedType.U4)][In][Out] ref int piHashAlg, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)][Out] byte[] pbHash, [MarshalAs(UnmanagedType.U4)][In] int cchHash, [MarshalAs(UnmanagedType.U4)] out int pchHash);

            [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
            int GetHashFromFileW([MarshalAs(UnmanagedType.LPWStr)][In] string pwzFilePath, [MarshalAs(UnmanagedType.U4)][In][Out] ref int piHashAlg, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)][Out] byte[] pbHash, [MarshalAs(UnmanagedType.U4)][In] int cchHash, [MarshalAs(UnmanagedType.U4)] out int pchHash);

            [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
            int GetHashFromHandle([In] IntPtr hFile, [MarshalAs(UnmanagedType.U4)][In][Out] ref int piHashAlg, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)][Out] byte[] pbHash, [MarshalAs(UnmanagedType.U4)][In] int cchHash, [MarshalAs(UnmanagedType.U4)] out int pchHash);

            [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
            [return: MarshalAs(UnmanagedType.U4)]
            int StrongNameCompareAssemblies([MarshalAs(UnmanagedType.LPWStr)][In] string pwzAssembly1, [MarshalAs(UnmanagedType.LPWStr)][In] string pwzAssembly2, [MarshalAs(UnmanagedType.U4)] out int dwResult);

            [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
            int StrongNameFreeBuffer([In] IntPtr pbMemory);

            [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
            int StrongNameGetBlob([MarshalAs(UnmanagedType.LPWStr)][In] string pwzFilePath, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)][Out] byte[] pbBlob, [MarshalAs(UnmanagedType.U4)][In][Out] ref int pcbBlob);

            [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
            int StrongNameGetBlobFromImage([In] IntPtr pbBase, [MarshalAs(UnmanagedType.U4)][In] int dwLength, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)][Out] byte[] pbBlob, [MarshalAs(UnmanagedType.U4)][In][Out] ref int pcbBlob);

            [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
            int StrongNameGetPublicKey([MarshalAs(UnmanagedType.LPWStr)][In] string pwzKeyContainer, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)][In] byte[] pbKeyBlob, [MarshalAs(UnmanagedType.U4)][In] int cbKeyBlob, out IntPtr ppbPublicKeyBlob, [MarshalAs(UnmanagedType.U4)] out int pcbPublicKeyBlob);

            [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
            [return: MarshalAs(UnmanagedType.U4)]
            int StrongNameHashSize([MarshalAs(UnmanagedType.U4)][In] int ulHashAlg, [MarshalAs(UnmanagedType.U4)] out int cbSize);

            [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
            int StrongNameKeyDelete([MarshalAs(UnmanagedType.LPWStr)][In] string pwzKeyContainer);

            [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
            int StrongNameKeyGen([MarshalAs(UnmanagedType.LPWStr)][In] string pwzKeyContainer, [MarshalAs(UnmanagedType.U4)][In] int dwFlags, out IntPtr ppbKeyBlob, [MarshalAs(UnmanagedType.U4)] out int pcbKeyBlob);

            [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
            int StrongNameKeyGenEx([MarshalAs(UnmanagedType.LPWStr)][In] string pwzKeyContainer, [MarshalAs(UnmanagedType.U4)][In] int dwFlags, [MarshalAs(UnmanagedType.U4)][In] int dwKeySize, out IntPtr ppbKeyBlob, [MarshalAs(UnmanagedType.U4)] out int pcbKeyBlob);

            [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
            int StrongNameKeyInstall([MarshalAs(UnmanagedType.LPWStr)][In] string pwzKeyContainer, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)][In] byte[] pbKeyBlob, [MarshalAs(UnmanagedType.U4)][In] int cbKeyBlob);

            [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
            int StrongNameSignatureGeneration([MarshalAs(UnmanagedType.LPWStr)][In] string pwzFilePath, [MarshalAs(UnmanagedType.LPWStr)][In] string pwzKeyContainer, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)][In] byte[] pbKeyBlob, [MarshalAs(UnmanagedType.U4)][In] int cbKeyBlob, [In][Out] IntPtr ppbSignatureBlob, [MarshalAs(UnmanagedType.U4)] out int pcbSignatureBlob);

            [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
            int StrongNameSignatureGenerationEx([MarshalAs(UnmanagedType.LPWStr)][In] string wszFilePath, [MarshalAs(UnmanagedType.LPWStr)][In] string wszKeyContainer, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)][In] byte[] pbKeyBlob, [MarshalAs(UnmanagedType.U4)][In] int cbKeyBlob, [In][Out] IntPtr ppbSignatureBlob, [MarshalAs(UnmanagedType.U4)] out int pcbSignatureBlob, [MarshalAs(UnmanagedType.U4)][In] int dwFlags);

            [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
            int StrongNameSignatureSize([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)][In] byte[] pbPublicKeyBlob, [MarshalAs(UnmanagedType.U4)][In] int cbPublicKeyBlob, [MarshalAs(UnmanagedType.U4)] out int pcbSize);

            [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
            [return: MarshalAs(UnmanagedType.U4)]
            int StrongNameSignatureVerification([MarshalAs(UnmanagedType.LPWStr)][In] string pwzFilePath, [MarshalAs(UnmanagedType.U4)][In] int dwInFlags, [MarshalAs(UnmanagedType.U4)] out int dwOutFlags);

            [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
            [return: MarshalAs(UnmanagedType.U4)]
            int StrongNameSignatureVerificationEx([MarshalAs(UnmanagedType.LPWStr)][In] string pwzFilePath, [MarshalAs(UnmanagedType.I1)][In] bool fForceVerification, [MarshalAs(UnmanagedType.I1)] out bool fWasVerified);

            [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
            [return: MarshalAs(UnmanagedType.U4)]
            int StrongNameSignatureVerificationFromImage([In] IntPtr pbBase, [MarshalAs(UnmanagedType.U4)][In] int dwLength, [MarshalAs(UnmanagedType.U4)][In] int dwInFlags, [MarshalAs(UnmanagedType.U4)] out int dwOutFlags);

            [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
            int StrongNameTokenFromAssembly([MarshalAs(UnmanagedType.LPWStr)][In] string pwzFilePath, out IntPtr ppbStrongNameToken, [MarshalAs(UnmanagedType.U4)] out int pcbStrongNameToken);

            [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
            int StrongNameTokenFromAssemblyEx([MarshalAs(UnmanagedType.LPWStr)][In] string pwzFilePath, out IntPtr ppbStrongNameToken, [MarshalAs(UnmanagedType.U4)] out int pcbStrongNameToken, out IntPtr ppbPublicKeyBlob, [MarshalAs(UnmanagedType.U4)] out int pcbPublicKeyBlob);

            [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
            int StrongNameTokenFromPublicKey([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)][In] byte[] pbPublicKeyBlob, [MarshalAs(UnmanagedType.U4)][In] int cbPublicKeyBlob, out IntPtr ppbStrongNameToken, [MarshalAs(UnmanagedType.U4)] out int pcbStrongNameToken);
        }

    }
}
