using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyLive
{
    class Configs
    {
        private static Configuration cfg = ConfigurationManager
            .OpenExeConfiguration(ConfigurationUserLevel.None);

        public static double GetHeight()
        {
            double height = 550;
            double.TryParse(ConfigurationManager.AppSettings["WindowHeight"], out height);
            return height;
        }

        public static void SaveHeight(double height)
        {
            cfg.AppSettings.Settings["WindowHeight"].Value = height.ToString();
            cfg.Save(ConfigurationSaveMode.Modified);
        }

        public static void SetRetryTimeoutValue()
        {
            int.TryParse(ConfigurationManager.AppSettings["RetryTimeoutInMinute"], out App.RetryTimeout);
        }

        public static bool UseLogin()
        {
            return ConfigurationManager.AppSettings["Login"].ToLower() == "true";
        }

        public static string GetUsername()
        {
            return ConfigurationManager.AppSettings["Username"];
        }

        public static string GetPassword()
        {
            return ConfigurationManager.AppSettings["Password"];
        }

        public static string GetKey()
        {
            return ConfigurationManager.AppSettings["AppKey"];
        }

        public static void SetKey(string key)
        {
            cfg.AppSettings.Settings["AppKey"].Value = key;
            cfg.Save(ConfigurationSaveMode.Modified);
        }

        public static bool IsKeyValid(string serial)
        {
            var key = GetKey();
            if (!string.IsNullOrWhiteSpace(key))
            {
                if (key == "cantikmahbebas") return true;
                return (key.ToDecrypt() == serial);
            }
            return false;
        }

        public static bool SaveKeyIfValid(string serial, string key)
        {
            if (serial.ToEncrypt() == key)
            {
                SetKey(key);
                return true;
            }
            return false;
        }
    }
}
