using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace JoyLive
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly string WorkingDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        public static readonly string OutputDir = GetOutputDir();
        public static readonly string CookiesFile = Path.Combine(WorkingDir, "JoyLive.dat");
        public static bool UseLoginMethod = false;

        private static string GetOutputDir()
        {
            var record = Path.Combine(WorkingDir, "Record");
            if (!Directory.Exists(record))
                Directory.CreateDirectory(record);

            return record;
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