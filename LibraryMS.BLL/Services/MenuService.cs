using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryMS.BLL.Models;
using LibraryMS.DAL.Repositories;

namespace LibraryMS.BLL.Services
{
    public class MenuService
    {
        private readonly MenuRepository _repo;
        public MenuService(MenuRepository repo) => _repo = repo;

        public async Task<List<MenuNode>> GetAllowedMenusAsync(string groupCode, string locCode)
        {
            var rows = await _repo.GetMenusByGroupAndLocationAsync(groupCode, locCode);

            return rows.Select(x => new MenuNode
            {
                Code = x.Code,
                Desc = x.Desc,
                Parent = x.Parent,
                ChildOrder = x.Child
            }).ToList();
        }
    }
}
