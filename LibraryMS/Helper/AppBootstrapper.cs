using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryMS.BLL.Services;
using LibraryMS.DAL.Core;
using LibraryMS.DAL.Repositories;
using Microsoft.Extensions.Configuration;

namespace LibraryMS.Win.Helper
{
    public static class AppBootstrapper
    {
        public static ServiceContainer Build()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (!File.Exists(path))
                throw new FileNotFoundException($"Missing appsettings.json at: {path}");

            var config = new ConfigurationBuilder()
                .AddJsonFile(path, optional: false)
                .Build();
            var cs = config.GetConnectionString("DefaultConnection")
                     ?? throw new Exception("Missing DefaultConnection in appsettings.json");

            // DAL core
            var db = new SqlDb(new DbConfig(cs));

            // Repositories
            var userRepo = new UserRepository(db);
            //var groupRepo = new UserGroupRepository(db);
            var menuRepo = new MenuRepository(db);
            var menuService = new MenuService(menuRepo);
            var locRepo = new LocationRepository(db);

            // Services (BLL)
            var auth = new AuthService(userRepo);
            //var menu = new MenuService(menuRepo);
            var loc = new LocationService(locRepo);

           // return new ServiceContainer(auth, menu, loc);
            return new ServiceContainer(loc, auth, menuService);
        }
    }

    //public record ServiceContainer(AuthService Auth, MenuService Menu, LocationService Location);
    public record ServiceContainer( LocationService Location, AuthService Auth, MenuService Menu);
}
