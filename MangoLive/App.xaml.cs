using System;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;

namespace MangoLive
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly string WorkingDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        public static readonly string OutputDir = GetOutputDir();
        public static int RetryTimeout = 15;
        public static bool CheckBeforeRecording = Configs.CheckBeforeRecording();

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

        public static string GetSID()
        {
            var sid = UserPrincipal.Current.Sid.ToString();
            var username = Environment.UserName;

            Match match = Regex.Match(sid, @"-\d{2}-([\d-]+)-");
            if (match.Success)
            {
                return match.Groups[1].Value.ToUpper();
            }
            else
            {
                return username.ToHash().ToUpper();
            }
        }
    }
}