using InfrastructureApp.Data;
using InfrastructureApp.Models;
using InfrastructureApp.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace InfrastructureApp_Tests.Reports
{
    // SCRUM-157: NUnit coverage for Latest Reports server-side pagination.
    [TestFixture]
    public class LatestReportsPaginationTests
    {
        private SqliteConnection _connection = null!;
        private DbContextOptions<ApplicationDbContext> _dbOptions = null!;

        [SetUp]
        public void SetUp()
        {
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();

            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

            using var db = NewDb();
            db.Database.EnsureCreated();
        }

        [TearDown]
        public void TearDown()
        {
            _connection.Dispose();
        }

        // TEST 1: First page returns only the configured page size.
        [Test]
        public async Task GetPaginatedLatestReportsAsync_FirstPage_ReturnsOnlyPageSizeCount()
        {
            // Arrange
            using var db = NewDb();
            await AddUserAsync(db, "user-1");
            await SeedReportsAsync(db, count: 15, userId: "user-1");
            var repo = new ReportIssueRepositoryEf(db);

            // Act
            var result = await repo.GetPaginatedLatestReportsAsync(isAdmin: false, keyword: null, sort: "newest", pageNumber: 1, pageSize: 10);

            // Assert
            Assert.That(result.Count, Is.EqualTo(10));
            Assert.That(result.PageIndex, Is.EqualTo(1));
            Assert.That(result.TotalPages, Is.EqualTo(2));
            Assert.That(result.HasNextPage, Is.True);
            Assert.That(result.HasPreviousPage, Is.False);
        }

        // TEST 2: Second page returns the next set of reports.
        [Test]
        public async Task GetPaginatedLatestReportsAsync_SecondPage_ReturnsNextSetOfReports()
        {
            // Arrange
            using var db = NewDb();
            await AddUserAsync(db, "user-2");
            await SeedReportsAsync(db, count: 15, userId: "user-2");
            var repo = new ReportIssueRepositoryEf(db);

            // Act
            var result = await repo.GetPaginatedLatestReportsAsync(isAdmin: false, keyword: null, sort: "newest", pageNumber: 2, pageSize: 10);

            // Assert
            Assert.That(result.Count, Is.EqualTo(5));
            Assert.That(result.Select(r => r.Description).ToList(), Is.EqualTo(new[]
            {
                "Report 05",
                "Report 04",
                "Report 03",
                "Report 02",
                "Report 01"
            }));
            Assert.That(result.PageIndex, Is.EqualTo(2));
            Assert.That(result.HasNextPage, Is.False);
            Assert.That(result.HasPreviousPage, Is.True);
        }

        private ApplicationDbContext NewDb()
        {
            return new ApplicationDbContext(_dbOptions);
        }

        private static async Task AddUserAsync(ApplicationDbContext db, string userId)
        {
            db.Users.Add(new Users
            {
                Id = userId,
                UserName = userId,
                NormalizedUserName = userId.ToUpperInvariant(),
                Email = $"{userId}@test.local",
                NormalizedEmail = $"{userId}@test.local".ToUpperInvariant()
            });

            await db.SaveChangesAsync();
        }

        private static async Task SeedReportsAsync(ApplicationDbContext db, int count, string userId, string descriptionPrefix = "Report", string status = "Approved")
        {
            var startDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            for (var i = 1; i <= count; i++)
            {
                db.ReportIssue.Add(new ReportIssue
                {
                    Description = $"{descriptionPrefix} {i:00}",
                    Status = status,
                    CreatedAt = startDate.AddMinutes(i),
                    UserId = userId,
                    Latitude = 44.85m,
                    Longitude = -123.23m
                });
            }

            await db.SaveChangesAsync();
        }
    }
}
