using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace trnservice.Services.Utils
{
    // Class to fetch AppSettings data on startup
    public class AppSettings
    {
        private static IConfiguration _configuration;
        private static Dictionary<string, string> _appSettings;

        // Initialize the configuration and read all appsettings data
        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
            _appSettings = new Dictionary<string, string>();

            // Read all appsettings data
            foreach (var config in _configuration.GetChildren())
            {
                _appSettings[config.Key] = config.Value;
            }
        }

        // Accessor to get appsettings value by key
        public static string GetAppSetting(string key)
        {
            if (_appSettings.TryGetValue(key, out var value))
            {
                return value;
            }

            return null; // Or throw an exception or handle as needed
        }

        // Accessor to get all appsettings data
        public static Dictionary<string, string> GetAllAppSettings()
        {
            return new Dictionary<string, string>(_appSettings);
        }
    }
}
