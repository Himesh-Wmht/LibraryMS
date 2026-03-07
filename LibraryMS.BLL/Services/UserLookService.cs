using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryMS.DAL.Repositories;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.BLL.Services
{
    public sealed class UserLookupService
    {
        private readonly UserLookupRepository _repo;
        public UserLookupService(UserLookupRepository repo) => _repo = repo;

        public Task<List<LookupItemDto>> LookupUsersAsync(string? text) => _repo.LookupUsersAsync(text);
    }
}