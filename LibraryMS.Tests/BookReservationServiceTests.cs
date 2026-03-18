using System.Threading.Tasks;
using Moq;
using Xunit;
using LibraryMS.BLL.Services;
using LibraryMS.DAL.Repositories;
using static LibraryMS.DAL.Repositories.Dtos;

public class BookReservationServiceTests
{
    [Fact]
    public async Task CreateRequestAsync_WithValidDto_ShouldCallRepository()
    {
        var repo = new Mock<BookReservationRepository>();
        var service = new BookReservationService(repo.Object);

        var dto = new ReservationRequestDto
        {
            UserCode = "U001",
            BookCode = "B001",
            LocCode = "L001",
            Qty = 1,
            HoldDays = 7,
            Remark = "Reserve for test"
        };

        await service.CreateRequestAsync(dto);

        repo.Verify(x => x.CreateRequestAsync(dto), Times.Once);
    }
}
