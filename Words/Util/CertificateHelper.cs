using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Words.Util
{
    public static class CertificateHelper
    {
        private static X509Certificate2 FindCertificateByThumbprint(string findValue)
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            try
            {
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection col = store.Certificates.Find(X509FindType.FindByThumbprint,
                    findValue, false); // Don't validate certs, since the test root isn't installed.
                if (col == null || col.Count == 0)
                    return null;
                return col[0];
            }
            finally
            {
                store.Close();
            }
        }

        private static string Clientid = "7b5323d4-0b9e-4aa2-9703-828d3b8f0a5d";
        private static string Thumbprint = "B6B89FF95EFBCF395FCEA2655DEEA3A685AA7F56";
        private static ClientAssertionCertificate _assertionCert;
        private static ClientAssertionCertificate AssertionCert
        {
            get
            {
                if (_assertionCert == null)
                {
                    var clientAssertionCertPfx = FindCertificateByThumbprint(Thumbprint);
                    _assertionCert = new ClientAssertionCertificate(Clientid, clientAssertionCertPfx);
                }
                return _assertionCert;
            }
        }



        public static async Task<string> GetAccessToken(string authority, string resource, string scope)
        {
            var context = new AuthenticationContext(authority, TokenCache.DefaultShared);
            var result = await context.AcquireTokenAsync(resource, AssertionCert);
            return result.AccessToken;
        }
    }
}
