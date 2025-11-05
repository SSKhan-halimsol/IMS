using System.Configuration;
using System.IO;

namespace IMS.Helpers
{
    public static class DbConfigManager
    {
        private static readonly string ConfigFile = "dbconfig.txt";

        public static void SaveConnectionString(string connStr)
        {
            File.WriteAllText(ConfigFile, connStr);
            SetIsConfigured(true);
        }

        public static string GetConnectionString()
        {
            return File.Exists(ConfigFile) ? File.ReadAllText(ConfigFile) : string.Empty;
        }

        public static void ClearConnectionString()
        {
            if (File.Exists(ConfigFile))
                File.Delete(ConfigFile);

            SetIsConfigured(false);
        }

        public static void ApplyToAppConfig(string connStr)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var section = (ConnectionStringsSection)config.GetSection("connectionStrings");

            if (section.ConnectionStrings["IMSConnection"] != null)
            {
                section.ConnectionStrings["IMSConnection"].ConnectionString = connStr;
            }
            else
            {
                section.ConnectionStrings.Add(
                    new ConnectionStringSettings("IMSConnection", connStr, "System.Data.SqlClient"));
            }

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("connectionStrings");
        }

        // ✅ Added helper methods
        public static bool IsConfigured()
        {
            var val = ConfigurationManager.AppSettings["IsDbConfigured"];
            return val != null && val.ToLower() == "true";
        }

        public static void SetIsConfigured(bool value)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (config.AppSettings.Settings["IsDbConfigured"] == null)
                config.AppSettings.Settings.Add("IsDbConfigured", value.ToString().ToLower());
            else
                config.AppSettings.Settings["IsDbConfigured"].Value = value.ToString().ToLower();

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}