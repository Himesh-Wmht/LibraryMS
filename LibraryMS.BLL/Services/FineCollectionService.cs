using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryMS.DAL.Repositories;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.BLL.Services
{
    public sealed class FineCollectionService
    {
        private readonly FineCollectionRepository _repo;
        public FineCollectionService(FineCollectionRepository repo) => _repo = repo;

        public Task<List<MemberFineRowDto>> SearchOpenAsync(string? text)
            => _repo.SearchOpenAsync(text);

        public Task PayAsync(FinePaymentDto dto)
            => _repo.PayAsync(dto);

        public Task RefundAsync(FineRefundDto dto)
            => _repo.RefundAsync(dto);
        public Task<List<FineDetailRowDto>> GetDetailsAsync(string fineDocNo)
            => _repo.GetDetailsAsync(fineDocNo);
    }
}