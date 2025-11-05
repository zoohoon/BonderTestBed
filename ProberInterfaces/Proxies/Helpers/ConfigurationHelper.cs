using System.Configuration;

namespace ProberInterfaces.Proxies.Helpers
{
    public class ConfigurationHelper
    {
        public static string GetKey(string key, string defaultValue)
        {
            return ConfigurationManager.AppSettings[key] ?? defaultValue;
        }
    }
}
