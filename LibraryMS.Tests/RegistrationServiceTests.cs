using System.Threading.Tasks;
using Moq;
using Xunit;
using LibraryMS.BLL.Services;
using LibraryMS.DAL.Repositories;
using LibraryMS.BLL.Models;

public class RegistrationServiceTests
{
    [Fact]
    public async Task RegisterAsync_WithValidRequest_ShouldReturnSuccess()
    {
        var db = new Mock<SqlDb>();
        var groups = new Mock<UserGroupRepository>();
        var subs = new Mock<SubscriptionRepository>();
        var locs = new Mock<LocationRepository>();
        var reg = new Mock<UserRegistrationRepository>();
        var approvals = new Mock<ApprovalRepository>();
        var userLookup = new Mock<UserLookupRepository>();

        var service = new RegistrationService(
            db.Object, groups.Object, subs.Object, locs.Object, reg.Object, approvals.Object, userLookup.Object);

        var req = new UserRegistrationRequest
        {
            Code = "U001",
            Name = "Test User",
            GroupCode = "USER",
            Mobile = "1234567890",
            Password = "password",
            Uid = "U001",
            MemberStatus = true,
            SubscriptionStatus = false,
            AllLocations = true,
            MaxBorrow = 5
        };

        var (ok, message) = await service.RegisterAsync(req, null);

        Assert.True(ok);
        Assert.Contains("Saved", message);
    }
}
