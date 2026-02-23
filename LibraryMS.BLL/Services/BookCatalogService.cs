using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryMS.DAL.Repositories;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.BLL.Services
{
    public sealed class BookCatalogService
    {
        private readonly BookCatalogRepository _books;
        private readonly BookCategoryRepository _cats;

        public BookCatalogService(BookCatalogRepository books, BookCategoryRepository cats)
        {
            _books = books;
            _cats = cats;
        }

        public Task<List<BookCategoryDto>> GetCategoriesAsync() => _cats.GetActiveAsync();

        public Task<List<BookRowDto>> SearchAsync(string? text, string? category, bool activeOnly)
            => _books.SearchAsync(text, category, activeOnly);

        public Task SaveAsync(BookUpsertDto dto) => _books.UpsertAsync(dto);
    }
}
