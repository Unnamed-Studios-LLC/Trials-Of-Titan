using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using Utils.NET.Crypto;

string privateKey, publicKey;
using (RSA rsa = RSA.Create())
{
    rsa.KeySize = 1024;
    privateKey = encodeKey(new RsaSerializableParameters(rsa.ExportParameters(true)));
    publicKey = encodeKey(new RsaSerializableParameters(rsa.ExportParameters(false)));
}

var path = Environment.CurrentDirectory;

File.WriteAllText(Path.Combine(path, "private.txt"), privateKey);
File.WriteAllText(Path.Combine(path, "public.txt"), publicKey);

Console.WriteLine("Private Key\n");
Console.WriteLine(privateKey);

Console.WriteLine("\nPublic Key\n");
Console.WriteLine(publicKey);

Console.WriteLine("Saved to path");
Console.WriteLine(path);

string encodeKey(RsaSerializableParameters parameters)
{
    return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(parameters, Formatting.None)));
}