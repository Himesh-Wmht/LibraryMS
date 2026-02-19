using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace LibraryMS.Win.Helper
{
    public static class AppConfig
    {
        private static IConfigurationRoot? _config;

        public static IConfigurationRoot Config =>
            _config ??= new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

        public static T GetSection<T>(string sectionName) where T : new()
        {
            var obj = new T();
            Config.GetSection(sectionName).Bind(obj);
            return obj;
        }
    }
}
