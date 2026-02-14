using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        InactiveUser
    }
    public class AuthService
    {
        private readonly UserRepository _users;

        public AuthService(UserRepository users)
        {
            _users = users;
        }
        public async Task<(AuthResult Result, UserSession? Session, string Message)> LoginAsync(string userCode, string password)
        {
            if (string.IsNullOrWhiteSpace(userCode))
                return (AuthResult.InvalidId, null, "Invalid User ID");

            var u = await _users.GetUserByCodeAsync(userCode.Trim());
            if (u == null)
                return (AuthResult.InvalidId, null, "Invalid User ID");

            if (!u.U_ACTIVE)
                return (AuthResult.InactiveUser, null, "Inactive User");

            // Hashed password path
            if (PasswordHasher.LooksHashed(u.U_PASSWORD))
            {
                if (!PasswordHasher.VerifyPassword(password, u.U_PASSWORD))
                    return (AuthResult.InvalidPassword, null, "Incorrect Password");
            }
            else
            {
                // Legacy plaintext fallback (NOT recommended)
                if (!string.Equals(password, u.U_PASSWORD))
                    return (AuthResult.InvalidPassword, null, "Incorrect Password");

                // Optional: auto-upgrade to hash after successful login
                var newHash = PasswordHasher.HashPassword(password);
                await _users.UpdatePasswordAsync(u.U_CODE, newHash);
            }

            var session = new UserSession
            {
                UserCode = u.U_CODE,
                UserName = u.U_NAME,
                GroupCode = u.U_GROUP
            };

            return (AuthResult.LoginGranted, session, "OK");
        }

        // For Admin create user / change password features
        public Task<string> HashPasswordAsync(string plain) =>
            Task.FromResult(PasswordHasher.HashPassword(plain));
    }
}

