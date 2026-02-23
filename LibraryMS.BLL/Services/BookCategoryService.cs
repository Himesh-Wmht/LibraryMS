using LibraryMS.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.BLL.Services
{
    public sealed class BookCategoryService
    {
        private readonly BookCategoryRepository _repo;
        public BookCategoryService(BookCategoryRepository repo) => _repo = repo;

        public Task<List<BookCategoryRowDto>> SearchAsync(string? text, bool activeOnly)
            => _repo.SearchAsync(text, activeOnly);

        public Task SaveAsync(BookCategoryUpsertDto dto)
            => _repo.UpsertAsync(dto);
    }
}
