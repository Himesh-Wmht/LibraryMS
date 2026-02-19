using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryMS.DAL.Repositories;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.BLL.Services
{
    public sealed class GroupMenuService
    {
        private readonly GroupMenuRepository _repo;

        public GroupMenuService(GroupMenuRepository repo) => _repo = repo;

        public Task EnsureAsync()
            => _repo.EnsureDefaultsAsync();

        public Task<List<GroupMenuRowDto>> GetMenusForGroupAsync(string groupCode, string locCode)
            => _repo.GetMenusForGroupAsync(groupCode, locCode);

        public Task SaveAsync(string groupCode, string locCode, List<GroupMenuUpdateDto> updates)
            => _repo.SaveGroupMenusAsync(groupCode, locCode, updates);
    }
}
