using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryMS.DAL.Repositories;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.BLL.Services
{
    public sealed class LocationLookupService
    {
        private readonly LocationLookupRepository _repo;
        public LocationLookupService(LocationLookupRepository repo) => _repo = repo;

        // ✅ matches UI call: _locs.LookupAsync(q)
        public Task<List<LookupItemDto>> LookupAsync(string? text)
            => _repo.LookupAsync(text);
    }
}