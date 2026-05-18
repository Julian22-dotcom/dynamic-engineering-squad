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
