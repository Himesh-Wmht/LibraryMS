using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryMS.DAL.Repositories;

namespace LibraryMS.BLL.Services
{
    public sealed class UserContactService
    {
        private readonly UserContactRepository _repo;
        public UserContactService(UserContactRepository repo) => _repo = repo;

        public Task<UserContactDto?> GetByUserCodeAsync(string userCode)
            => _repo.GetByUserCodeAsync(userCode);
    }
}
