using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using InfrastructureApp.Data;
using InfrastructureApp.Models;
using InfrastructureApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace InfrastructureApp_Tests.Dashboard
{
    // SCRUM-142: Tests private Dashboard report status summary counts.
    [TestFixture]
    public class DashboardReportStatusSummaryRepositoryTests
    {
        private ApplicationDbContext _db = null!;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("DashboardReportStatusSummaryRepositoryTest_" + Guid.NewGuid())
                .Options;

            _db = new ApplicationDbContext(options);
        }

        [TearDown]
        public void TearDown()
        {
            _db.Dispose();
        }

        // TEST 1: Reports are counted by ReportIssue.Status for the logged-in user.
        [Test]
        public async Task GetDashboardSummaryAsync_WhenCurrentUserHasReports_CountsReportsByStatus()
        {
            // Arrange: create a logged-in user with multiple reports using the same status.
            var currentUser = CreateUser("current-user", "current@test.com");
            _db.Users.Add(currentUser);
            _db.ReportIssue.AddRange(
                CreateReport(currentUser.Id, "Approved report one", "Approved"),
                CreateReport(currentUser.Id, "Approved report two", "Approved"));
            await _db.SaveChangesAsync();

            var repo = CreateRepositoryForCurrentUser(currentUser);

            // Act: load the private Dashboard summary.
            var result = await repo.GetDashboardSummaryAsync();

            // Assert: the status summary contains one Approved row with the correct count.
            Assert.That(result.ReportStatusSummary, Has.Count.EqualTo(1));
            Assert.That(result.ReportStatusSummary[0].Status, Is.EqualTo("Approved"));
            Assert.That(result.ReportStatusSummary[0].Count, Is.EqualTo(2));
        }

        // TEST 2: Reports from other users are excluded from the logged-in user's status summary.
        [Test]
        public async Task GetDashboardSummaryAsync_WhenOtherUsersHaveReports_ExcludesOtherUsersStatusCounts()
        {
            // Arrange: create reports for the logged-in user and another user.
            var currentUser = CreateUser("current-user", "current@test.com");
            var otherUser = CreateUser("other-user", "other@test.com");
            _db.Users.AddRange(currentUser, otherUser);
            _db.ReportIssue.AddRange(
                CreateReport(currentUser.Id, "Current user report", "Approved"),
                CreateReport(otherUser.Id, "Other user report", "Resolved"));
            await _db.SaveChangesAsync();

            var repo = CreateRepositoryForCurrentUser(currentUser);

            // Act: load the private Dashboard summary.
            var result = await repo.GetDashboardSummaryAsync();

            // Assert: only the logged-in user's report status is counted.
            Assert.That(result.ReportStatusSummary, Has.Count.EqualTo(1));
            Assert.That(result.ReportStatusSummary[0].Status, Is.EqualTo("Approved"));
            Assert.That(result.ReportStatusSummary[0].Count, Is.EqualTo(1));
            Assert.That(result.ReportStatusSummary.Select(summary => summary.Status), Does.Not.Contain("Resolved"));
        }

        private DashboardRepositoryEf CreateRepositoryForCurrentUser(Users currentUser)
        {
            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, currentUser.Id)
                }, "TestAuth"))
            };

            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContextAccessor.SetupGet(x => x.HttpContext).Returns(httpContext);

            var userManager = CreateUserManager(currentUser);
            Mock.Get(userManager)
                .Setup(m => m.GetUserAsync(httpContext.User))
                .ReturnsAsync(currentUser);

            return new DashboardRepositoryEf(_db, userManager, httpContextAccessor.Object);
        }

        private static UserManager<Users> CreateUserManager(Users currentUser)
        {
            var store = new Mock<IUserStore<Users>>();
            var userManager = new Mock<UserManager<Users>>(
                store.Object,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!);

            userManager.Setup(m => m.FindByIdAsync(currentUser.Id))
                .ReturnsAsync(currentUser);

            return userManager.Object;
        }

        private static Users CreateUser(string userName, string email)
        {
            return new Users
            {
                Id = Guid.NewGuid().ToString(),
                UserName = userName,
                Email = email,
                EmailConfirmed = true
            };
        }

        private static ReportIssue CreateReport(string userId, string description, string status)
        {
            return new ReportIssue
            {
                UserId = userId,
                Description = description,
                Status = status,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
