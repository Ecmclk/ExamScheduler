using Microsoft.Extensions.Configuration;

using System.IO;
namespace ExamScheduler.Core
{
    public static class ConfigManager
    {
        private static IConfigurationRoot _configuration;

        static ConfigManager()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();
        }

        public static string GetConnectionString(string name = "DefaultConnection")
        {
            return _configuration.GetConnectionString(name);
        }
    }
}
