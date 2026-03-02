using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryMS.BLL.Security;
using LibraryMS.DAL.Repositories;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.BLL.Services
{
    public sealed class PasswordResetService
    {
        private readonly PasswordResetRequestRepository _repo;
        public PasswordResetService(PasswordResetRequestRepository repo) => _repo = repo;

        public Task<List<PwdResetRowDto>> GetPendingAsync() => _repo.GetPendingAsync();
        public Task<List<PwdResetRowDto>> GetAllAsync() => _repo.GetAllAsync();

        // Request insert: NO hashing (as you requested)
        public Task CreateRequestAsync(string userCode, string plainNewPassword, string? requestedBy)
        {
            if (string.IsNullOrWhiteSpace(userCode)) throw new Exception("User Code required.");
            if (string.IsNullOrWhiteSpace(plainNewPassword) || plainNewPassword.Length < 6)
                throw new Exception("Password must be at least 6 characters.");
            return _repo.CreateRequestAsync(userCode.Trim(), plainNewPassword, requestedBy);
        }

        // Approve: fetch plain -> hash -> update user
        public async Task ApproveAsync(string prId, string adminUserCode)
        {
            var (_, plainPwd) = await _repo.GetForApproveAsync(prId);

            // ✅ Hash here using your class
            var hashed = PasswordHasher.HashPassword(plainPwd);

            await _repo.ApproveAsync(prId, adminUserCode, hashed);
        }

        public Task RejectAsync(string prId, string adminUserCode, string? remark)
            => _repo.RejectAsync(prId, adminUserCode, remark);
    }
}