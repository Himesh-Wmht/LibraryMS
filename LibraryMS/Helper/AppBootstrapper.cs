using System;
using System.IO;
using LibraryMS.BLL.Services;
using LibraryMS.DAL.Core;
using LibraryMS.DAL.Repositories;
using Microsoft.Extensions.Configuration;

namespace LibraryMS.Win.Helper
{
    public static class AppBootstrapper
    {
        public static async Task<ServiceContainer> BuildAsync()
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
            var groupRepo = new UserGroupRepository(db);
            var subRepo = new SubscriptionRepository(db);
            var menuRepo = new MenuRepository(db);
            var locRepo = new LocationRepository(db);
            var regRepo = new UserRegistrationRepository(db);
            var approvalRepo = new ApprovalRepository(db);
            var groupMenuRepo = new GroupMenuRepository(db);

            // Services (BLL)
            var auth = new AuthService(userRepo);
            var menuService = new MenuService(menuRepo);
            var approvalService = new ApprovalService(approvalRepo);
            var groupMenuService = new GroupMenuService(groupMenuRepo);
            //var groupMenuService = new GroupMenuService(groupMenuRepo);

            // NOTE: Your RegistrationService constructor must match this signature
            var regService = new RegistrationService(db, groupRepo, subRepo, locRepo, regRepo, approvalRepo);

            var loc = new LocationService(locRepo);
            groupMenuService.EnsureAsync().GetAwaiter().GetResult();
            return new ServiceContainer(
                Location: loc,
                Auth: auth,
                Menu: menuService,
                Registration: regService,
                Approval: approvalService,
                GroupMenus: groupMenuService,
                GroupRepo: groupRepo
            );
        }
    }

    public record ServiceContainer(
        LocationService Location,
        AuthService Auth,
        MenuService Menu,
        RegistrationService Registration,
        ApprovalService Approval,
        GroupMenuService GroupMenus,
        UserGroupRepository GroupRepo
    );
}
