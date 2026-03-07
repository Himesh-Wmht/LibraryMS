using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryMS.DAL.Repositories;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.BLL.Services
{
    public sealed class BookReservationService
    {
        private readonly BookReservationRepository _repo;
        public BookReservationService(BookReservationRepository repo) => _repo = repo;

        public Task<List<BookAvailRowDto>> SearchAvailableAsync(string locCode, string? text)
            => _repo.SearchAvailableAsync(locCode, text);

        public Task<List<ResMyRowDto>> GetMyAsync(string userCode, string locCode)
            => _repo.GetMyAsync(userCode, locCode);

        public Task CreateRequestAsync(ReservationRequestDto dto)
            => _repo.CreateRequestAsync(dto);

        public Task<int> CancelAsync(int resId, string userCode)
            => _repo.CancelAsync(resId, userCode);

        public Task<List<ResPendingRowDto>> GetPendingAsync(string locCode, bool loadAll)
            => _repo.GetPendingAsync(locCode, loadAll);

        public Task ApproveAsync(int resId, string adminUserCode)
            => _repo.ApproveAsync(resId, adminUserCode);

        public Task RejectAsync(int resId, string adminUserCode, string? remark)
            => _repo.RejectAsync(resId, adminUserCode, remark);

        public Task ProcessAsync(int resId, string adminUserCode)
            => _repo.ProcessAsync(resId, adminUserCode);

        public Task<int> CancelByUserAsync(int resId, string userCode)
    => _repo.CancelByUserAsync(resId, userCode);

        public Task<int> CancelByAdminAsync(int resId, string adminUserCode, string? remark)
            => _repo.CancelByAdminAsync(resId, adminUserCode, remark);

    }
}