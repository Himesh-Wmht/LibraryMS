using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryMS.DAL.Repositories;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.BLL.Services
{
    public sealed class BookTransferService
    {
        private readonly BookTransferRepository _repo;
        public BookTransferService(BookTransferRepository repo) => _repo = repo;

        // ✅ for your UI: _svc.LookupBooksInLocAsync(fromLoc, q)
        public Task<List<LookupItemDto>> LookupBooksInLocAsync(string fromLoc, string? text)
            => _repo.LookupBooksInLocAsync(fromLoc, text);

        // optional fallback
        public Task<List<LookupItemDto>> LookupBooksAsync(string? text)
            => _repo.LookupBooksAsync(text);

        public Task<string> CreateAsync(TransferCreateDto dto) => _repo.CreateAsync(dto);

        public Task<List<TransferHeaderRowDto>> GetPendingAsync(string toLoc, bool loadAll)
            => _repo.GetPendingAsync(toLoc, loadAll);

        public Task<List<TransferDetailRowDto>> GetDetailsAsync(string docNo)
            => _repo.GetDetailsAsync(docNo);

        public Task ApproveAsync(string docNo, string approvedBy)
            => _repo.ApproveAsync(docNo, approvedBy);

        public Task RejectAsync(string docNo, string rejectedBy, string? remark)
            => _repo.RejectAsync(docNo, rejectedBy, remark);

        public Task<int> GetAvailableQtyAsync(string locCode, string bookCode)
    => _repo.GetAvailableQtyAsync(locCode, bookCode);
    }
}