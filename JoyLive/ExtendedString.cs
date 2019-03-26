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

        public static string ToDecrypt(this string encrypted)
        {
            try
            {
                using (var csp = new AesCryptoServiceProvider())
                {
                    csp.Mode = CipherMode.CBC;
                    csp.Padding = PaddingMode.PKCS7;
                    var spec = new Rfc2898DeriveBytes(
                        Encoding.UTF8.GetBytes("C0L1-T3RU5-S4MP3-P3G3L"),
                        Encoding.UTF8.GetBytes("CR4CK3R?FUCK-Y0U-KAL14N"),
                        65536);
                    csp.Key = spec.GetBytes(16);
                    csp.IV = Encoding.UTF8.GetBytes("2019032662309102");
                    var decryptor = csp.CreateDecryptor();
                    byte[] output = Convert.FromBase64String(encrypted);
                    byte[] decryptedOutput = decryptor.TransformFinalBlock(output, 0, output.Length);
                    return Encoding.UTF8.GetString(decryptedOutput);
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
