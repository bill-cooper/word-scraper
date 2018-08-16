using Microsoft.Azure.KeyVault;
using System.Collections.Generic;
using Words.Util;

namespace Words
{
    public interface ISecretProvider {
        string GetSecret(string key);
    }
    public class SecretProvider : ISecretProvider
    {
        private static Dictionary<string, string> _secretCache = new Dictionary<string, string>();
        public string GetSecret(string key)
        {
            if (_secretCache.ContainsKey(key))
                return _secretCache[key];

            var kv = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(CertificateHelper.GetAccessToken));
            var secret = kv.GetSecretAsync($"https://russian-word-app-keys.vault.azure.net/secrets/{key}").Result;
            _secretCache.Add(key, secret.Value);
            return secret.Value;
        }
    }
}
