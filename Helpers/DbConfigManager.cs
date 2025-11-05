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
        }

        public static string GetConnectionString()
        {
            return File.Exists(ConfigFile) ? File.ReadAllText(ConfigFile) : string.Empty;
        }

        public static void ClearConnectionString()
        {
            if (File.Exists(ConfigFile))
                File.Delete(ConfigFile);
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
    }
}