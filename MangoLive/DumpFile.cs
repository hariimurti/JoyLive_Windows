using System;
using System.IO;

namespace MangoLive
{
    class DumpFile
    {
        public static void Write(string filename, string content)
        {
            try
            {
                if (!Configs.GetDumpFile())
                    return;

                var dumpDir = Path.Combine(App.WorkingDir, "DumpJson");
                if (!Directory.Exists(dumpDir))
                    Directory.CreateDirectory(dumpDir);

                var dumpFile = Path.Combine(dumpDir, filename);
                File.WriteAllText(dumpFile, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
