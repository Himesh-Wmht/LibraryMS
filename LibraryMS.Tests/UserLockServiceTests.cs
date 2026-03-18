using System.Threading.Tasks;
using Moq;
using Xunit;
using LibraryMS.BLL.Services;
using LibraryMS.DAL.Repositories;

public class UserLockServiceTests
{
    [Fact]
    public async Task RegisterFailedAttemptAsync_WhenAttemptsReachFour_ShouldCreateLock()
    {
        var lockRepo = new Mock<UserLockApprovalRepository>();
        lockRepo.Setup(x => x.GetFailedAttemptsAsync("U001")).ReturnsAsync(4);
        lockRepo.Setup(x => x.IsLockedAsync("U001")).ReturnsAsync(false);

        var service = new UserLockService(lockRepo.Object);

        await service.RegisterFailedAttemptAsync("U001");

        lockRepo.Verify(x => x.CreateLockRequestAsync("U001"), Times.Once);
    }
}
