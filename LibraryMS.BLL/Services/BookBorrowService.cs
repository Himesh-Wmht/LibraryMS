using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryMS.DAL.Repositories;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.BLL.Services
{
    public sealed class BookBorrowService
    {
        private readonly BookBorrowRepository _repo;
        public BookBorrowService(BookBorrowRepository repo) => _repo = repo;

        public Task<string> CreateAsync(BorrowCreateDto dto) => _repo.CreateAsync(dto);

        public Task<List<BorrowOpenRowDto>> SearchOpenAsync(string locCode, string? text)
            => _repo.SearchOpenAsync(locCode, text);

        public Task<List<BorrowOpenDetailRowDto>> GetOpenDetailsAsync(string docNo)
            => _repo.GetOpenDetailsAsync(docNo);
    }
}