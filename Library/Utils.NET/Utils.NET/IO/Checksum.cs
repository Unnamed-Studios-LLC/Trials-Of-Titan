using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Utils.NET.IO
{
    public static class Checksum
    {
        public static byte[] MD5(string file)
        {
            byte[] hash;
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                using (var stream = File.OpenRead(file))
                {
                    hash = md5.ComputeHash(stream);
                }
            }
            return hash;
        }
    }
}
