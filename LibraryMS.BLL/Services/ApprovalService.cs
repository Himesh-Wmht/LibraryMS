using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryMS.DAL.Repositories;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.BLL.Services
{
    public sealed class ApprovalService
    {
        private readonly ApprovalRepository _repo;
        public ApprovalService(ApprovalRepository repo) => _repo = repo;

        public Task<List<ApprovalRowDto>> GetPendingAsync() => _repo.GetPendingAsync();
        public Task<List<ApprovalRowDto>> GetAllAsync() => _repo.GetAllAsync();

        public Task<(bool ok, string message)> ApproveAsync(string apId, string approvedBy)
            => _repo.ApproveAsync(apId, approvedBy);

        public Task<(bool ok, string message)> RejectAsync(string apId, string rejectedBy)
            => _repo.RejectAsync(apId, rejectedBy);
    }
}
