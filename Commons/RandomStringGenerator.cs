using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;

namespace WDAIIP.WEB.Commons
{
    public static class RandomStringGenerator
    {
        public static string GenerateAlphanumeric(int length)
        {
            // Define the characters to use for the random string (alphanumeric)
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            // Use RNGCryptoServiceProvider for cryptographically strong random number generation
            using (var rng = new RNGCryptoServiceProvider())
            {
                var result = new StringBuilder(length); var byteBuffer = new byte[1]; // Buffer to hold a single random byte
                for (int i = 0; i < length; i++)
                {
                    rng.GetBytes(byteBuffer); // Get a random byte // Map the random byte to an index within the 'chars' string
                    result.Append(chars[byteBuffer[0] % chars.Length]);
                }
                return result.ToString();
            }
        }

        public static string GetRS10()
        {
            //string randomString = GenerateAlphanumeric(10); //Console.WriteLine($"Generated random string: {randomString}");
            return GenerateAlphanumeric(10);
        }

    }
}