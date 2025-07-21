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
using Microsoft.Extensions.Logging;

namespace ol_api_dotnet.Tests
{
    [TestFixture]
    public class MerchantsControllerTests
    {
        private Mock<AppDbContext>? _mockDbContext;
        private Mock<ILogger<MerchantsController>>? _mockLogger;
        private MerchantsController? _merchantsController;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
            _mockDbContext = new Mock<AppDbContext>(options);
            _mockLogger = new Mock<ILogger<MerchantsController>>();
            _merchantsController = new MerchantsController(_mockDbContext.Object, _mockLogger.Object);
        }

        [Test]
        public async Task GetMerchants_ReturnsOk()
        {
            // Arrange
            var merchants = new List<Merchant> { new Merchant { Id = System.Guid.NewGuid(), Name = "Test Merchant" } }.AsQueryable();
            var mockDbSet = new Mock<DbSet<Merchant>>();
            mockDbSet.As<IQueryable<Merchant>>().Setup(m => m.Provider).Returns(merchants.Provider);
            mockDbSet.As<IQueryable<Merchant>>().Setup(m => m.Expression).Returns(merchants.Expression);
            mockDbSet.As<IQueryable<Merchant>>().Setup(m => m.ElementType).Returns(merchants.ElementType);
            mockDbSet.As<IQueryable<Merchant>>().Setup(m => m.GetEnumerator()).Returns(merchants.GetEnumerator());
            _mockDbContext.Setup(db => db.Merchants).Returns(mockDbSet.Object);

            // Act
            var result = await _merchantsController.GetAll();

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetMerchant_MerchantFound_ReturnsOk()
        {
            // Arrange
            var merchantId = System.Guid.NewGuid();
            var merchant = new Merchant { Id = merchantId, Name = "Test Merchant" };
            var mockDbSet = new Mock<DbSet<Merchant>>();
            mockDbSet.Setup(m => m.FindAsync(merchantId)).ReturnsAsync(merchant);
            _mockDbContext.Setup(db => db.Merchants).Returns(mockDbSet.Object);

            // Act
            var result = await _merchantsController.GetById(merchantId);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetMerchant_MerchantNotFound_ReturnsNotFound()
        {
            // Arrange
            var merchantId = System.Guid.NewGuid();
            var mockDbSet = new Mock<DbSet<Merchant>>();
            mockDbSet.Setup(m => m.FindAsync(merchantId)).ReturnsAsync((Merchant)null);
            _mockDbContext.Setup(db => db.Merchants).Returns(mockDbSet.Object);

            // Act
            var result = await _merchantsController.GetById(merchantId);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }
    }
} 