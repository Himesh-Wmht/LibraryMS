using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryMS.DAL.Repositories;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.BLL.Services
{
    public sealed class BookInventoryService
    {
        private readonly BookInventoryRepository _repo;
        public BookInventoryService(BookInventoryRepository repo) => _repo = repo;

        public Task<List<InvRowDto>> SearchAsync(string locCode, string? text, bool activeOnly)
            => _repo.SearchAsync(locCode, text, activeOnly);

        public Task SaveAsync(InvUpsertDto dto) => _repo.UpsertAsync(dto);
    }
}
