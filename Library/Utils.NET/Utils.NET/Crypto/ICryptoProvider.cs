using System;
using System.Security.Cryptography;
using System.Text;

namespace Utils.NET.Crypto
{
    public interface ICryptoProvider
    {
        byte[] DecryptRsa(byte[] data);

        string DecryptRsaBase64(string base64);

        string DecryptRsaBase64(string base64, Encoding encoding);

        byte[] EncryptRsa(byte[] data);

        string EncryptRsaBase64(string value);

        string EncryptRsaBase64(string value, Encoding encoding);

        byte[] HashSha1(byte[] input);

        string HashSha1(string input, Encoding encoding);

        string HashSha1(string input);

        byte[] HashSha256(byte[] input);

        string HashSha256(string input, Encoding encoding);

        string HashSha256(string input);

        void SetRsaPublicKey(RSAParameters parameters);

        void SetRsaPublicKey(RsaSerializableParameters parameters);

        void SetRsaPrivateKey(RSAParameters parameters);

        void SetRsaPrivateKey(RsaSerializableParameters parameters);
    }
}
