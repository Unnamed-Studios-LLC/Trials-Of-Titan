using System;
using System.Security.Cryptography;
using System.Text;
using Utils.NET.Utils;

namespace Utils.NET.Crypto
{
    public class CryptoProvider : ICryptoProvider
    {
        private readonly SHA1 sha1;

        private readonly SHA256 sha256;

        private readonly RSA rsa;

        public CryptoProvider()
        {
            sha1 = SHA1.Create();
            sha256 = SHA256.Create();
            rsa = RSA.Create();
        }

        public byte[] DecryptRsa(byte[] data)
        {
            return rsa.Decrypt(data, RSAEncryptionPadding.Pkcs1);
        }

        public string DecryptRsaBase64(string base64)
        {
            return DecryptRsaBase64(base64, Encoding.UTF8);
        }

        public string DecryptRsaBase64(string base64, Encoding encoding)
        {
            var encrypted = Convert.FromBase64String(base64);
            var bytes = DecryptRsa(encrypted);
            return encoding.GetString(bytes);
        }

        public byte[] EncryptRsa(byte[] data)
        {
            return rsa.Encrypt(data, RSAEncryptionPadding.Pkcs1);
        }

        public string EncryptRsaBase64(string value)
        {
            return EncryptRsaBase64(value, Encoding.UTF8);
        }

        public string EncryptRsaBase64(string value, Encoding encoding)
        {
            var bytes = encoding.GetBytes(value);
            var encrypted = EncryptRsa(bytes);
            return Convert.ToBase64String(encrypted);
        }

        public byte[] HashSha1(byte[] input)
        {
            return sha1.ComputeHash(input);
        }

        public string HashSha1(string input, Encoding encoding)
        {
            return StringUtils.ToHexString(HashSha1(encoding.GetBytes(input)));
        }

        public string HashSha1(string input)
        {
            return HashSha1(input, Encoding.UTF8);
        }

        public byte[] HashSha256(byte[] input)
        {
            return sha256.ComputeHash(input);
        }

        public string HashSha256(string input, Encoding encoding)
        {
            return StringUtils.ToHexString(HashSha256(encoding.GetBytes(input)));
        }

        public string HashSha256(string input)
        {
            return HashSha256(input, Encoding.UTF8);
        }

        public void SetRsaPrivateKey(RSAParameters parameters)
        {
            rsa.ImportParameters(parameters);
        }

        public void SetRsaPrivateKey(RsaSerializableParameters parameters)
        {
            rsa.ImportParameters(parameters.GetParameters());
        }

        public void SetRsaPublicKey(RSAParameters parameters)
        {
            rsa.ImportParameters(parameters);
        }

        public void SetRsaPublicKey(RsaSerializableParameters parameters)
        {
            rsa.ImportParameters(parameters.GetParameters());
        }
    }
}
