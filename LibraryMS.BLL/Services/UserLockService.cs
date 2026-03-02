using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryMS.DAL.Repositories;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.BLL.Services
{
    public sealed class UserLockService
    {
        private readonly UserLockApprovalRepository _repo;
        public UserLockService(UserLockApprovalRepository repo) => _repo = repo;

        public Task<List<UserLockRowDto>> GetPendingAsync() => _repo.GetPendingAsync();
        public Task UnlockAsync(string ulId, string adminUserCode) => _repo.UnlockAsync(ulId, adminUserCode);
    }
}