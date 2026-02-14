using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryMS.BLL.Models;
using LibraryMS.DAL.Repositories;

namespace LibraryMS.BLL.Services
{
    public class LocationService
    {
        private readonly LocationRepository _repo;
        public LocationService(LocationRepository repo) => _repo = repo;

        public async Task<List<LocationItem>> GetUserLocationsAsync(string userCode)
        {
            if (string.IsNullOrWhiteSpace(userCode))
                return new List<LocationItem>();

            var rows = await _repo.GetActiveLocationsForUserAsync(userCode.Trim());

            return rows.Select(x => new LocationItem { Code = x.Code, Desc = x.Desc }).ToList();
        }

        public Task<bool> UserHasLocationAsync(string userCode, string locCode) =>
            _repo.UserHasLocationAsync(userCode, locCode);
    }
}
