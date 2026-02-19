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
using static LibraryMS.DAL.Repositories.Dtos;

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

        public RegistrationService(SqlDb db,UserGroupRepository groups, SubscriptionRepository subs,LocationRepository locs, UserRegistrationRepository reg, ApprovalRepository approvals)
        {
            _db = db;
            _groups = groups;
            _subs = subs;
            _locs = locs;
            _reg = reg;
            _approvals = approvals;
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

        public async Task<(bool ok, string message)> RegisterAsync(UserRegistrationRequest req)
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

                // ✅ NEW: if USER group -> insert into approval table (PENDING)
                if (isUserGroup)
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

                    await tx.CommitAsync();
                    return (true, $"Saved. Pending approval (AP_ID: {apId}). User stays INACTIVE until approved.");
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

    }
}
