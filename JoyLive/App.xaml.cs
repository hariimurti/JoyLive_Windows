using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace JoyLive
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly string WorkingDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        public static readonly string CacheDir = GetCacheDir();

        private static string GetCacheDir()
        {
            var cache = Path.Combine(WorkingDir, "cache");
            if (!Directory.Exists(cache))
                Directory.CreateDirectory(cache);

            return cache;
        }

        public static string GetBuildVersion()
        {
            var attrb = typeof(MainWindow)
                .GetTypeInfo()
                .Assembly
                .CustomAttributes
                .ToList()
                .FirstOrDefault(x => x.AttributeType.Name == "AssemblyFileVersionAttribute");
            var version = attrb?.ConstructorArguments[0].Value.ToString();
            if (string.IsNullOrWhiteSpace(version)) version = "0.0";
            return version;
        }
    }
}
