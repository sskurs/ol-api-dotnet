using Moq;
using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using ol_api_dotnet.Controllers;
using ol_api_dotnet.Data;
using ol_api_dotnet.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ol_api_dotnet.Tests
{
    [TestFixture]
    public class LoyaltyControllerTests
    {
        private Mock<AppDbContext>? _mockDbContext;
        private LoyaltyController? _loyaltyController;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
            _mockDbContext = new Mock<AppDbContext>(options);
            _loyaltyController = new LoyaltyController(_mockDbContext.Object);
        }

        [Test]
        public async Task GetUserData_UserFound_ReturnsOk()
        {
            // Arrange
            var user = new User { Id = 1 };
            var points = new Points { UserId = 1, Balance = 100 };
            var tier = new Tier { UserId = 1, Level = "Gold" };

            var mockUserDbSet = new Mock<DbSet<User>>();
            mockUserDbSet.Setup(m => m.FindAsync(1)).ReturnsAsync(user);
            _mockDbContext.Setup(db => db.Users).Returns(mockUserDbSet.Object);

            var mockPointsDbSet = new Mock<DbSet<Points>>();
            var pointsData = new List<Points> { points }.AsQueryable();
            mockPointsDbSet.As<IQueryable<Points>>().Setup(m => m.Provider).Returns(pointsData.Provider);
            mockPointsDbSet.As<IQueryable<Points>>().Setup(m => m.Expression).Returns(pointsData.Expression);
            mockPointsDbSet.As<IQueryable<Points>>().Setup(m => m.ElementType).Returns(pointsData.ElementType);
            mockPointsDbSet.As<IQueryable<Points>>().Setup(m => m.GetEnumerator()).Returns(pointsData.GetEnumerator());
            _mockDbContext.Setup(db => db.Points).Returns(mockPointsDbSet.Object);

            var mockTierDbSet = new Mock<DbSet<Tier>>();
            var tierData = new List<Tier> { tier }.AsQueryable();
            mockTierDbSet.As<IQueryable<Tier>>().Setup(m => m.Provider).Returns(tierData.Provider);
            mockTierDbSet.As<IQueryable<Tier>>().Setup(m => m.Expression).Returns(tierData.Expression);
            mockTierDbSet.As<IQueryable<Tier>>().Setup(m => m.ElementType).Returns(tierData.ElementType);
            mockTierDbSet.As<IQueryable<Tier>>().Setup(m => m.GetEnumerator()).Returns(tierData.GetEnumerator());
            _mockDbContext.Setup(db => db.Tiers).Returns(mockTierDbSet.Object);

            var rewards = new List<Reward>().AsQueryable();
            var mockRewardDbSet = new Mock<DbSet<Reward>>();
            mockRewardDbSet.As<IQueryable<Reward>>().Setup(m => m.Provider).Returns(rewards.Provider);
            mockRewardDbSet.As<IQueryable<Reward>>().Setup(m => m.Expression).Returns(rewards.Expression);
            mockRewardDbSet.As<IQueryable<Reward>>().Setup(m => m.ElementType).Returns(rewards.ElementType);
            mockRewardDbSet.As<IQueryable<Reward>>().Setup(m => m.GetEnumerator()).Returns(rewards.GetEnumerator());
            _mockDbContext.Setup(db => db.Rewards).Returns(mockRewardDbSet.Object);


            var transactions = new List<Transaction>().AsQueryable();
            var mockTransactionDbSet = new Mock<DbSet<Transaction>>();
            mockTransactionDbSet.As<IQueryable<Transaction>>().Setup(m => m.Provider).Returns(transactions.Provider);
            mockTransactionDbSet.As<IQueryable<Transaction>>().Setup(m => m.Expression).Returns(transactions.Expression);
            mockTransactionDbSet.As<IQueryable<Transaction>>().Setup(m => m.ElementType).Returns(transactions.ElementType);
            mockTransactionDbSet.As<IQueryable<Transaction>>().Setup(m => m.GetEnumerator()).Returns(transactions.GetEnumerator());
            _mockDbContext.Setup(db => db.Transactions).Returns(mockTransactionDbSet.Object);

            // Act
            var result = await _loyaltyController.GetUserLoyalty(1);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetUserData_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var mockDbSet = new Mock<DbSet<User>>();
            mockDbSet.Setup(m => m.FindAsync(1)).ReturnsAsync((User)null);
            _mockDbContext.Setup(db => db.Users).Returns(mockDbSet.Object);

            // Act
            var result = await _loyaltyController.GetUserLoyalty(1);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }
    }
} 