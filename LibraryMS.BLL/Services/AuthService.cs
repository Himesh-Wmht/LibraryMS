using System;
using System.Threading.Tasks;
using LibraryMS.BLL.Models;
using LibraryMS.BLL.Security;
using LibraryMS.DAL.Repositories;

namespace LibraryMS.BLL.Services
{
    public enum AuthResult
    {
        LoginGranted,
        InvalidId,
        InvalidPassword,
        InactiveUser,
        AccountLocked
    }

    public sealed class AuthService
    {
        private readonly UserRepository _users;
        private readonly UserLockApprovalRepository _locks;

        public AuthService(UserRepository users, UserLockApprovalRepository locks)
        {
            _users = users ?? throw new ArgumentNullException(nameof(users));
            _locks = locks ?? throw new ArgumentNullException(nameof(locks));
        }

        public async Task<(AuthResult Result, UserSession? Session, string Message)> LoginAsync(string userCode, string password)
        {
            userCode = (userCode ?? "").Trim();
            password = password ?? "";

            if (string.IsNullOrWhiteSpace(userCode))
                return (AuthResult.InvalidId, null, "Invalid User ID");

            // ✅ Use your login query that includes fail/locked
            var login = await _users.GetLoginUserAsync(userCode);
            if (login == null)
                return (AuthResult.InvalidId, null, "Invalid User ID");

            if (!login.Active)
                return (AuthResult.InactiveUser, null, "Inactive User");

            if (login.Locked)
                return (AuthResult.AccountLocked, null, "Account locked. Please contact Admin.");

            // ✅ Verify password (hashed OR legacy plaintext)
            bool ok;
            string? upgradedHash = null;

            if (PasswordHasher.LooksHashed(login.PasswordStored))
            {
                ok = PasswordHasher.VerifyPassword(password, login.PasswordStored);
            }
            else
            {
                ok = string.Equals(password, login.PasswordStored);

                // ✅ Auto-upgrade legacy plaintext to hash on success
                if (ok)
                    upgradedHash = PasswordHasher.HashPassword(password);
            }

            if (!ok)
            {
                // ✅ increment fail count + lock on 4
                var (failCount, lockedNow) = await _users.OnLoginFailAsync(userCode);

                if (lockedNow)
                {
                    // ✅ create pending unlock request
                    await _locks.EnsurePendingUnlockRequestAsync(userCode);
                    return (AuthResult.AccountLocked, null, "Account locked after 4 failed attempts. Contact Admin.");
                }

                return (AuthResult.InvalidPassword, null, $"Incorrect Password. Attempt {failCount}/4");
            }

            // ✅ success: reset fail count, unlock, and upgrade password if needed
            await _users.OnLoginSuccessAsync(userCode, upgradedHash);

            var session = new UserSession
            {
                UserCode = login.UserCode,
                UserName = login.UserName,
                GroupCode = login.GroupCode
            };

            return (AuthResult.LoginGranted, session, "OK");
        }

        // For Admin create user / change password features
        public Task<string> HashPasswordAsync(string plain) =>
            Task.FromResult(PasswordHasher.HashPassword(plain));
    }
}