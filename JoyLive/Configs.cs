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
        public static double GetHeight()
        {
            double height = 550;
            double.TryParse(ConfigurationManager.AppSettings["WindowHeight"], out height);
            return height;
        }

        public static void SaveHeight(double height)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["WindowHeight"].Value = height.ToString();
            config.Save(ConfigurationSaveMode.Modified);
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
    }
}
