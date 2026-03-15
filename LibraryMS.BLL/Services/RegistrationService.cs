using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryMS.BLL.Models;
using LibraryMS.BLL.Security;
using LibraryMS.DAL.Core;
using LibraryMS.DAL.Repositories;
using static LibraryMS.BLL.Models.UserUpdateModels;
using static LibraryMS.DAL.Repositories.Dtos;
using static LibraryMS.DAL.Repositories.UserRegistrationRepository;

namespace LibraryMS.BLL.Services
{
    public class RegistrationService
    {
        private readonly SqlDb _db;
        private readonly UserGroupRepository _groups;
        private readonly SubscriptionRepository _subs;
        private readonly LocationRepository _locs;
        private readonly UserRegistrationRepository _reg;
        private readonly ApprovalRepository _approvals;
        private readonly UserLookupRepository _userLookup;

        public RegistrationService(SqlDb db, UserGroupRepository groups, SubscriptionRepository subs, LocationRepository locs, UserRegistrationRepository reg, ApprovalRepository approvals, UserLookupRepository userLookup)
        {
            _db = db;
            _groups = groups;
            _subs = subs;
            _locs = locs;
            _reg = reg;
            _approvals = approvals;
            _userLookup = userLookup;
        }

        public async Task<List<UserGroupItem>> GetGroupsAsync()
            => (await _groups.GetActiveGroupsAsync())
                .Select(x => new UserGroupItem { Code = x.Code, Name = x.Name, MembershipFee = x.MembershipFee })
                .ToList();

        public async Task<List<SubscriptionItem>> GetSubscriptionsAsync()
            => (await _subs.GetActiveSubscriptionsAsync())
                .Select(x => new SubscriptionItem { Id = x.Id, Desc = x.Desc, Days = x.Days })
                .ToList();

        public async Task<List<LocationItem>> GetLocationsAsync()
            => (await _locs.GetActiveLocationsAsync())
                .Select(x => new LocationItem { Code = x.Code, Desc = x.Desc })
                .ToList();

        public async Task<List<UserSearchItem>> SearchUsersAsync(string query)
            => (await _reg.SearchUsersAsync(query))
                .Select(x => new UserSearchItem(x.Code, x.Name))
                .ToList();

        public Task<List<LookupItemDto>> LookupUsersAsync(string? text)
            => _userLookup.LookupUsersAsync(text);
        public async Task<(bool ok, string message)> RegisterAsync(UserRegistrationRequest req, string? registeredBy)
        {
            // Validate mandatory
            if (string.IsNullOrWhiteSpace(req.Code)) return (false, "U_CODE is required.");
            if (string.IsNullOrWhiteSpace(req.Code)) return (false, "U_UID is required.");
            if (string.IsNullOrWhiteSpace(req.Name)) return (false, "U_NAME is required.");
            if (string.IsNullOrWhiteSpace(req.GroupCode)) return (false, "U_GROUP is required.");
            if (string.IsNullOrWhiteSpace(req.Mobile)) return (false, "U_MOBILE is required.");
            if (string.IsNullOrWhiteSpace(req.Password)) return (false, "U_PASSWORD is required.");

            if (!req.AllLocations && string.IsNullOrWhiteSpace(req.LocationCode))
                return (false, "Select a Location or ALL.");

            if (req.SubscriptionStatus)
            {
                if (string.IsNullOrWhiteSpace(req.SubscriptionId) || req.SubscriptionDays is null)
                    return (false, "Subscription Type is required when subscribed.");
            }

            // Rule: USER group must go for approval => inactive
            bool isUserGroup = req.GroupCode.Equals("USER", StringComparison.OrdinalIgnoreCase);
            bool isAdminOrSuper =req.GroupCode.Equals("ADMIN", StringComparison.OrdinalIgnoreCase) || req.GroupCode.Equals("SUPERADMIN", StringComparison.OrdinalIgnoreCase) || req.GroupCode.Equals("SADM", StringComparison.OrdinalIgnoreCase); // you used SADM earlier

            bool activeForDb = isUserGroup ? false : req.Active;

            var now = DateTime.Now;
            DateTime? expiry = null;

            if (req.SubscriptionStatus && req.SubscriptionDays.HasValue)
                expiry = now.AddDays(req.SubscriptionDays.Value);

            var passHash = PasswordHasher.HashPassword(req.Password);

            await using var con = _db.CreateConnection();
            await con.OpenAsync();
            await using var tx = await con.BeginTransactionAsync();

            try
            {
                var row = new UserInsertRow(
                    Code: req.Code.Trim(),
                    Name: req.Name.Trim(),
                    Active: activeForDb,
                    GroupCode: req.GroupCode.Trim(),
                    Mobile: req.Mobile.Trim(),
                    Dob: req.Dob,
                    Address: req.Address?.Trim(),
                    PasswordHash: passHash,
                    Nic: req.Nic?.Trim(),
                    Uid: req.Uid.Trim(),
                    Gender: req.Gender?.Trim(),
                    MemberStatus: req.MemberStatus,
                    SubscriptionStatus: req.SubscriptionStatus,
                    Email: req.Email?.Trim(),
                    RegisterDate: now,
                    SubscriptionType: req.SubscriptionStatus ? req.SubscriptionId : null,
                    ExpireDate: expiry,
                    MaxBorrow: req.MaxBorrow
                );

                await _reg.InsertUserAsync((SqlConnection)con, (SqlTransaction)tx, row);

                if (req.AllLocations)
                    await _reg.InsertUserLocationsAllAsync((SqlConnection)con, (SqlTransaction)tx, req.Code.Trim(), true);
                else
                    await _reg.InsertUserLocationOneAsync((SqlConnection)con, (SqlTransaction)tx, req.Code.Trim(), req.LocationCode!.Trim(), true);

                // Insert into approval table:
                // - USER => pending
                // - ADMIN/SUPERADMIN => already processed (approved) for tracking + settlement
                if (isUserGroup || isAdminOrSuper)
                {
                    var due = Math.Max(0m, req.MembershipFee - req.PaidAmt);

                    var approval = new ApprovalInsertDto(
                    UserCode: req.Code.Trim(),
                    ApId: "", // you can pass empty, repository can still generate (or change repo to use this)
                    UserUid: req.Uid.Trim(),
                    Name: req.Name.Trim(),
                    Mobile: req.Mobile.Trim(),
                    GroupCode: req.GroupCode.Trim(),
                    SubType: req.SubscriptionId,
                    PaidAmt: req.PaidAmt,
                    DueAmt: due,
                    PaymentMethod: req.PaymentMethod,
                    ReferenceNo: req.ReferenceNo,
                    ApprovedBy: null,
                    ApDate: DateTime.Now,
                    Processed: false,
                    Canceled: false
                    );

                    var apId = await _approvals.InsertPendingAsync((SqlConnection)con, (SqlTransaction)tx, approval);

                    if (isUserGroup)
                    {
                        await tx.CommitAsync();
                        return (true, $"Saved. Pending approval (AP_ID: {apId}). User stays INACTIVE until approved.");
                    }
                }

                await tx.CommitAsync();
                return (true, "Saved successfully.");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return (false, $"Save failed: {ex.Message}");
            }
        }

        public async Task<UserEditModel?> GetUserForEditAsync(string userCode)
        {
            var u = await _reg.GetUserDetailsAsync(userCode);
            if (u == null) return null;

            var locs = await _reg.GetUserLocationCodesAsync(userCode);
            var activeLocCount = await _reg.GetActiveLocationCountAsync();

            bool all = activeLocCount > 0 && locs.Count >= activeLocCount;
            string? oneLoc = all ? null : locs.FirstOrDefault();

            // subscription days: you already have subscriptions lookup in UI, so we’ll keep days null here
            return new UserEditModel(
                Code: u.Code,
                Name: u.Name,
                Mobile: u.Mobile,
                GroupCode: u.GroupCode,
                Active: u.Active,
                MemberStatus: u.MemberStatus,
                SubscriptionStatus: u.SubscriptionStatus,
                SubscriptionId: u.SubscriptionType,
                SubscriptionDays: null,
                Email: u.Email,
                Nic: u.Nic,
                Address: u.Address,
                Dob: u.Dob,
                Gender: u.Gender,
                MaxBorrow: u.MaxBorrow,
                AllLocations: all,
                LocationCode: oneLoc
            );
        }

        public async Task<(bool ok, string message)> UpdateUserAsync(UserUpdateRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Code)) return (false, "User Code is required.");
            if (string.IsNullOrWhiteSpace(req.Name)) return (false, "Full Name is required.");
            if (string.IsNullOrWhiteSpace(req.GroupCode)) return (false, "User Group is required.");
            if (string.IsNullOrWhiteSpace(req.Mobile)) return (false, "Mobile is required.");

            if (!req.AllLocations && string.IsNullOrWhiteSpace(req.LocationCode))
                return (false, "Select a Location or ALL.");

            // USER group rule: must remain inactive (no approval insert here — just enforce inactive)
            bool isUserGroup = req.GroupCode.Equals("USER", StringComparison.OrdinalIgnoreCase);
            bool activeForDb = isUserGroup ? false : req.Active;

            var now = DateTime.Now;
            DateTime? expiry = null;
            if (req.SubscriptionStatus && req.SubscriptionDays.HasValue)
                expiry = now.AddDays(req.SubscriptionDays.Value);

            await using var con = _db.CreateConnection();
            await con.OpenAsync();
            await using var tx = (SqlTransaction)await con.BeginTransactionAsync();

            try
            {
                var row = new UserUpdateRow(
                    Code: req.Code.Trim(),
                    Name: req.Name.Trim(),
                    Active: activeForDb,
                    GroupCode: req.GroupCode.Trim(),
                    Mobile: req.Mobile.Trim(),
                    Dob: req.Dob,
                    Address: req.Address?.Trim(),
                    Nic: req.Nic?.Trim(),
                    Email: req.Email?.Trim(),
                    Uid: req.Code.Trim(), // if you don’t have separate UID input, keep same as code
                    Gender: req.Gender?.Trim(),
                    MemberStatus: req.MemberStatus,
                    SubscriptionStatus: req.SubscriptionStatus,
                    SubscriptionType: req.SubscriptionStatus ? req.SubscriptionId : null,
                    ExpireDate: expiry,
                    MaxBorrow: req.MaxBorrow
                );

                await _reg.UpdateUserAsync((SqlConnection)con, tx, row);

                // locations: easiest = replace
                await _reg.DeleteUserLocationsAsync((SqlConnection)con, tx, req.Code.Trim());
                if (req.AllLocations)
                    await _reg.InsertUserLocationsAllAsync((SqlConnection)con, tx, req.Code.Trim(), true);
                else
                    await _reg.InsertUserLocationOneAsync((SqlConnection)con, tx, req.Code.Trim(), req.LocationCode!.Trim(), true);

                // password optional
                if (!string.IsNullOrWhiteSpace(req.NewPassword))
                {
                    var passHash = PasswordHasher.HashPassword(req.NewPassword);
                    await _reg.UpdateUserPasswordAsync((SqlConnection)con, tx, req.Code.Trim(), passHash);
                }

                await tx.CommitAsync();
                return (true, "User updated successfully.");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return (false, $"Update failed: {ex.Message}");
            }

        }
    }
}
