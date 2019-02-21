using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
