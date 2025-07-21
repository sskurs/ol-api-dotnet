using Moq;
using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using ol_api_dotnet.Controllers;
using ol_api_dotnet.Data;
using ol_api_dotnet.Models;
using ol_api_dotnet.Services;
using System.Threading.Tasks;

namespace ol_api_dotnet.Tests
{
    [TestFixture]
    public class TransactionControllerTests
    {
        private Mock<AppDbContext>? _mockDbContext;
        private Mock<CustomEventService>? _mockCustomEventService;
        private Mock<EarningRuleEngineService>? _mockEarningRuleEngineService;
        private TransactionController? _transactionController;

        [SetUp]
        public void Setup()
        {
            _mockDbContext = new Mock<AppDbContext>();
            var mockServiceProvider = new Mock<System.IServiceProvider>();
            _mockCustomEventService = new Mock<CustomEventService>(mockServiceProvider.Object);
            _mockEarningRuleEngineService = new Mock<EarningRuleEngineService>(mockServiceProvider.Object);
            _transactionController = new TransactionController(_mockDbContext.Object, _mockCustomEventService.Object, _mockEarningRuleEngineService.Object);
        }

        [Test]
        public async Task SimulatePurchase_ValidDto_ReturnsOk()
        {
            // Arrange
            var dto = new TransactionDto { UserId = 1, Amount = 100, Type = "purchase" };
            _mockEarningRuleEngineService.Setup(s => s.EvaluateTransaction(It.IsAny<Transaction>())).ReturnsAsync(10);
            var mockDbSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<User>>();
            var user = new User { Id = 1 };
            mockDbSet.Setup(m => m.FindAsync(1)).ReturnsAsync(user);
            _mockDbContext.Setup(db => db.Users).Returns(mockDbSet.Object);


            // Act
            var result = await _transactionController.SimulatePurchase(dto);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult.Value, Is.Not.Null);
        }

        [Test]
        public async Task Create_ValidDto_ReturnsOk()
        {
            // Arrange
            var dto = new TransactionDto { UserId = 1, Amount = 100, Type = "purchase" };
            _mockEarningRuleEngineService.Setup(s => s.EvaluateTransaction(It.IsAny<Transaction>())).ReturnsAsync(10);
            var mockUserDbSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<User>>();
            var user = new User { Id = 1 };
            mockUserDbSet.Setup(m => m.FindAsync(1)).ReturnsAsync(user);
            _mockDbContext.Setup(db => db.Users).Returns(mockUserDbSet.Object);
            _mockDbContext.Setup(db => db.Transactions.Add(It.IsAny<Transaction>()));
            _mockDbContext.Setup(db => db.Points.Add(It.IsAny<Points>()));
            _mockDbContext.Setup(db => db.SaveChangesAsync(default)).ReturnsAsync(1);


            // Act
            var result = await _transactionController.Create(dto);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            _mockDbContext.Verify(db => db.Points.Add(It.Is<Points>(p => p.UserId == 1 && p.Balance == 10)), Times.Once);
        }
    }
} 