using System.Security.Cryptography;
using System.Text;
using System;

namespace DataCloner.Framework
{
    public class KeyGenerator
    {
        static readonly char[] Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

        public static string GetUniqueKey(int maxSize)
        {
            if (maxSize < 1) throw new ArgumentException("Max size must be positive");

            var data = new byte[1];
            using (var crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetNonZeroBytes(data);
                data = new byte[maxSize];
                crypto.GetNonZeroBytes(data);
            }
            var result = new StringBuilder(maxSize);
            foreach (var b in data)
                result.Append(Chars[b % (Chars.Length)]);

            return result.ToString();
        }
    }
}
