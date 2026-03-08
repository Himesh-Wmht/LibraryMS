using System;
using System.IO;
using LibraryMS.BLL.Models;
using LibraryMS.BLL.Services;
using LibraryMS.DAL.Core;
using LibraryMS.DAL.Repositories;
using Microsoft.Extensions.Configuration;
using static LibraryMS.BLL.Models.Transaction;

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
            var rules = new LibraryRulesOptions();
            config.GetSection("LibraryRules").Bind(rules);

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
            var catRepo = new BookCategoryRepository(db);
            var bookRepo = new BookCatalogRepository(db);
            var invRepo = new BookInventoryRepository(db);
            var category = new BookCategoryRepository(db);
            var pwdReqRepo = new PasswordResetRequestRepository(db);
            var lockRepo = new UserLockApprovalRepository(db);
            var resRepo = new BookReservationRepository(db);
            var userLookupRepo = new UserLookupRepository(db);
            var transferRepo = new BookTransferRepository(db);
            var locLookupRepo = new LocationLookupRepository(db);
            var borrowRepo = new BookBorrowRepository(db);
            var returnRepo = new BookReturnRepository(db);
            var fineRepo = new FineCollectionRepository(db);

            // Services (BLL)
            var auth = new AuthService(userRepo, lockRepo);
            var menuService = new MenuService(menuRepo);
            var approvalService = new ApprovalService(approvalRepo);
            var groupMenuService = new GroupMenuService(groupMenuRepo);
            var bookCategoryService = new BookCategoryService(catRepo);
            var bookCatalogService = new BookCatalogService(bookRepo, catRepo);
            var bookInventoryService = new BookInventoryService(invRepo);
            var passwordResetService = new PasswordResetService(pwdReqRepo);
            var userLockService = new UserLockService(lockRepo);
            var resService = new BookReservationService(resRepo);
            var regService = new RegistrationService(db, groupRepo, subRepo, locRepo, regRepo, approvalRepo);
            var userLookupService = new UserLookupService(userLookupRepo);
            var loc = new LocationService(locRepo);
            var reservationsService = new BookReservationService(resRepo);
            var transferService = new BookTransferService(transferRepo);
            var locLookupService = new LocationLookupService(locLookupRepo);
            var fineCalculator = new FineCalculatorService(rules);
            var borrowService = new BookBorrowService(borrowRepo);
            var returnService = new BookReturnService(returnRepo, fineCalculator);
            var fineService = new FineCollectionService(fineRepo);

            groupMenuService.EnsureAsync().GetAwaiter().GetResult();
            return new ServiceContainer(
                Location: loc,
                Auth: auth,
                Menu: menuService,
                Registration: regService,
                Approval: approvalService,
                GroupMenus: groupMenuService,
                GroupRepo: groupRepo,
                BookCatalog: bookCatalogService,
                BookInventory: bookInventoryService,
                BookCategories: bookCategoryService,
                PasswordReset: passwordResetService,
                UserLocks: userLockService,
                UserLookup: userLookupService,
                Reservations: reservationsService,
                Transfers: transferService,
                LocationLookup: locLookupService,
                Borrows: borrowService,
                Returns: returnService,
                Fines: fineService
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
        UserGroupRepository GroupRepo,
        BookCatalogService BookCatalog,
        BookInventoryService BookInventory,
        BookCategoryService BookCategories,
        PasswordResetService PasswordReset,
        UserLockService UserLocks,
        BookReservationService Reservations,
        UserLookupService UserLookup,
        BookTransferService Transfers,
        LocationLookupService LocationLookup,
        BookBorrowService Borrows,
        BookReturnService Returns,
        FineCollectionService Fines
    );
}
