using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Utils.NET.Crypto
{
    public static class RsaUtils
    {
        public static RsaKeyPair GenerateRsaKeys()
        {
            var keyPair = new RsaKeyPair();
            using (RSA rsa = RSA.Create())
            {
                rsa.KeySize = 1024;
                keyPair.privateKey = new RsaSerializableParameters(rsa.ExportParameters(true));
                keyPair.publicKey = new RsaSerializableParameters(rsa.ExportParameters(false));
            }
            return keyPair;
        }
    }
}
