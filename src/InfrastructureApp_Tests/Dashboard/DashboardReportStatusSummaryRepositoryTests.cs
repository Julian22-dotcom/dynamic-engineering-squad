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
