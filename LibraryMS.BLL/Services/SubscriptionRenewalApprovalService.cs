using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryMS.DAL.Repositories;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.BLL.Services
{
    public sealed class SubscriptionRenewalApprovalService
    {
        private readonly SubscriptionRenewalApprovalRepository _repo;

        public SubscriptionRenewalApprovalService(SubscriptionRenewalApprovalRepository repo)
        {
            _repo = repo;
        }

        public Task<List<SubscriptionRenewalApprovalRowDto>> GetPendingAsync()
            => _repo.GetPendingAsync();

        public Task<List<SubscriptionRenewalApprovalRowDto>> GetAllAsync()
            => _repo.GetAllAsync();

        public Task<(bool ok, string message)> ApproveAsync(int subId, string? remark = null, string? newUid = null)
            => _repo.ApproveAsync(subId, remark, newUid);

        public Task<(bool ok, string message)> RejectAsync(int subId, string? remark = null)
            => _repo.RejectAsync(subId, remark);
    }
}