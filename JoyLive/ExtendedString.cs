using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JoyLive
{
    public static class ExtendedString
    {
        public static string ToPlaylist(this string value)
        {
            if (!value.Contains("rtmp://"))
                return value;

            return value.Replace("rtmp://", "http://") + "/playlist.m3u8";
        }

        public static string ToHumanReadableFormat(this long value)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddSeconds(value)
                .ToLocalTime()
                .ToString("yyyy-MM-dd HH:mm");
        }

        public static string ToHumanReadableFormat(this double value)
        {
            string[] suffixes = { "", "K", "M", "G", "T", "P", "E", "Z", "Y" };
            for (var i = 0; i < suffixes.Length; i++)
                if (value <= Math.Pow(1000, i + 1))
                    return ThreeNonZeroDigits(value / Math.Pow(1000, i)) + " " + suffixes[i];

            return ThreeNonZeroDigits(value / Math.Pow(1000, suffixes.Length - 1)) + " " +
                   suffixes[suffixes.Length - 1];
        }

        private static string ThreeNonZeroDigits(double value)
        {
            if (value >= 10)
                return value.ToString("0.0");

            return value.ToString("0.00");
        }

        public static IEnumerable<string> SplitInParts(this string s, int partLength)
        {
            for (var i = 0; i < s.Length; i += partLength)
                yield return s.Substring(i, Math.Min(partLength, s.Length - i));
        }

        public static string ToHash(this string text)
        {
            var hash = string.Empty;
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(text);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                hash = sb.ToString();
            }
            return string.Join("-", hash.SplitInParts(11));
        }

        public static string ToEncrypt(this string text)
        {
            try
            {
                byte[] results;
                var utf8 = new UTF8Encoding();
                var hashProvider = new MD5CryptoServiceProvider();
                var computeHash = hashProvider.ComputeHash(utf8.GetBytes("JoyLive"));
                var tdesAlgorithm = new TripleDESCryptoServiceProvider()
                {
                    Key = computeHash,
                    Mode = CipherMode.ECB,
                    Padding = PaddingMode.PKCS7
                };
                var dataToEncrypt = utf8.GetBytes(text);
                try
                {
                    var encryptor = tdesAlgorithm.CreateEncryptor();
                    results = encryptor.TransformFinalBlock(dataToEncrypt, 0, dataToEncrypt.Length);
                }
                finally
                {
                    tdesAlgorithm.Clear();
                    hashProvider.Clear();
                }
                return Convert.ToBase64String(results);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static string ToDecrypt(this string text)
        {
            try
            {
                byte[] results;
                UTF8Encoding utf8 = new UTF8Encoding();
                MD5CryptoServiceProvider hashProvider = new MD5CryptoServiceProvider();
                byte[] computeHash = hashProvider.ComputeHash(utf8.GetBytes("JoyLive"));
                var tdesAlgorithm = new TripleDESCryptoServiceProvider()
                {
                    Key = computeHash,
                    Mode = CipherMode.ECB,
                    Padding = PaddingMode.PKCS7
                };
                byte[] dataToDecrypt = Convert.FromBase64String(text);
                try
                {
                    var decryptor = tdesAlgorithm.CreateDecryptor();
                    results = decryptor.TransformFinalBlock(dataToDecrypt, 0, dataToDecrypt.Length);
                }
                finally
                {
                    tdesAlgorithm.Clear();
                    hashProvider.Clear();
                }
                return utf8.GetString(results);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
