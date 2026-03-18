using System.Threading.Tasks;
using Moq;
using Xunit;
using LibraryMS.BLL.Services;
using LibraryMS.DAL.Repositories;

public class AuthServiceTests
{
    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsLoginGranted()
    {
        var userRepo = new Mock<UserRepository>();
        var lockRepo = new Mock<UserLockApprovalRepository>();

        // Setup user exists and not locked
        userRepo.Setup(x => x.GetLoginUserAsync("U001"))
            .ReturnsAsync(new LoginUserDb { UserCode = "U001", PasswordHash = "hashed" });
        lockRepo.Setup(x => x.IsLockedAsync("U001")).ReturnsAsync(false);

        var service = new AuthService(userRepo.Object, lockRepo.Object);

        var (result, session, msg) = await service.LoginAsync("U001", "correctPassword");

        Assert.Equal(AuthResult.LoginGranted, result);
        Assert.NotNull(session);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ReturnsFailed()
    {
        var userRepo = new Mock<UserRepository>();
        var lockRepo = new Mock<UserLockApprovalRepository>();

        userRepo.Setup(x => x.GetLoginUserAsync("U001"))
            .ReturnsAsync(new LoginUserDb { UserCode = "U001", PasswordHash = "hashed" });
        lockRepo.Setup(x => x.IsLockedAsync("U001")).ReturnsAsync(false);

        var service = new AuthService(userRepo.Object, lockRepo.Object);

        var (result, session, msg) = await service.LoginAsync("U001", "wrongPassword");

        Assert.NotEqual(AuthResult.LoginGranted, result);
        Assert.Null(session);
    }

    [Fact]
    public async Task LoginAsync_WhenAccountLocked_ReturnsAccountLocked()
    {
        var userRepo = new Mock<UserRepository>();
        var lockRepo = new Mock<UserLockApprovalRepository>();

        lockRepo.Setup(x => x.IsLockedAsync("U001")).ReturnsAsync(true);

        var service = new AuthService(userRepo.Object, lockRepo.Object);

        var (result, session, msg) = await service.LoginAsync("U001", "anyPassword");

        Assert.Equal(AuthResult.AccountLocked, result);
        Assert.Null(session);
    }

    [Fact]
    public async Task LoginAsync_WithUnknownUser_ReturnsFailed()
    {
        var userRepo = new Mock<UserRepository>();
        var lockRepo = new Mock<UserLockApprovalRepository>();

        userRepo.Setup(x => x.GetLoginUserAsync("U001")).ReturnsAsync((LoginUserDb?)null);

        var service = new AuthService(userRepo.Object, lockRepo.Object);

        var (result, session, msg) = await service.LoginAsync("U001", "anyPassword");

        Assert.NotEqual(AuthResult.LoginGranted, result);
        Assert.Null(session);
    }
}
