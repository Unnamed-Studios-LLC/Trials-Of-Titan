using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Utils.NET.Crypto
{
    public class Rsa
    {
        private readonly CryptoProvider _cryptoProvider;

        public Rsa(string key, bool @private)
        {
            var decodedKey = Encoding.UTF8.GetString(Convert.FromBase64String(key));
            var @params = JsonConvert.DeserializeObject<RsaSerializableParameters>(decodedKey);
            _cryptoProvider = new CryptoProvider();
            if (@private) _cryptoProvider.SetRsaPrivateKey(@params.GetParameters());
            else _cryptoProvider.SetRsaPublicKey(@params.GetParameters());
        }

        public byte[] Decrypt(byte[] data) => _cryptoProvider.DecryptRsa(data);
        public byte[] Encrypt(byte[] data) => _cryptoProvider.EncryptRsa(data);
    }
}
