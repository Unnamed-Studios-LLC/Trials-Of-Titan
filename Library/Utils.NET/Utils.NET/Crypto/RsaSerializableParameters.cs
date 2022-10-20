using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Utils.NET.Crypto
{
    public class RsaSerializableParameters
    {
        public string D;

        public string DP;

        public string DQ;

        public string Exponent;

        public string InverseQ;

        public string Modulus;

        public string P;

        public string Q;

        public RsaSerializableParameters()
        {

        }

        public RsaSerializableParameters(RSAParameters parameters)
        {
            ImportParameters(parameters);
        }

        public void ImportParameters(RSAParameters parameters)
        {
            D = StringValue(parameters.D);
            DP = StringValue(parameters.DP);
            DQ = StringValue(parameters.DQ);
            Exponent = StringValue(parameters.Exponent);
            InverseQ = StringValue(parameters.InverseQ);
            Modulus = StringValue(parameters.Modulus);
            P = StringValue(parameters.P);
            Q = StringValue(parameters.Q);
        }

        public RSAParameters GetParameters()
        {
            return new RSAParameters
            {
                D = ByteValue(D),
                DP = ByteValue(DP),
                DQ = ByteValue(DQ),
                Exponent = ByteValue(Exponent),
                InverseQ = ByteValue(InverseQ),
                Modulus = ByteValue(Modulus),
                P = ByteValue(P),
                Q = ByteValue(Q),
            };
        }

        private string StringValue(byte[] bytes)
        {
            return bytes == null ? null : Convert.ToBase64String(bytes);
        }

        private byte[] ByteValue(string base64Value)
        {
            return base64Value == null ? null : Convert.FromBase64String(base64Value);
        }
    }
}
