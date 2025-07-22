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
using Microsoft.Extensions.Configuration;

namespace ol_api_dotnet.Tests
{
    [TestFixture]
    public class PartnersControllerTests
    {
        private Mock<AppDbContext>? _mockDbContext;
        private Mock<ILogger<PartnersController>>? _mockLogger;
        private Mock<IConfiguration>? _mockConfig;
        private PartnersController? _partnersController;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
            _mockDbContext = new Mock<AppDbContext>(options);
            _mockLogger = new Mock<ILogger<PartnersController>>();
            _mockConfig = new Mock<IConfiguration>();
            _partnersController = new PartnersController(_mockDbContext.Object, _mockLogger.Object, _mockConfig.Object);
        }

        [Test]
        public async Task GetPartners_ReturnsOk()
        {
            // Arrange
            var partners = new List<Merchant> { new Merchant { Id = System.Guid.NewGuid(), Name = "Test Partner" } }.AsQueryable();
            var mockDbSet = new Mock<DbSet<Merchant>>();
            mockDbSet.As<IQueryable<Merchant>>().Setup(m => m.Provider).Returns(partners.Provider);
            mockDbSet.As<IQueryable<Merchant>>().Setup(m => m.Expression).Returns(partners.Expression);
            mockDbSet.As<IQueryable<Merchant>>().Setup(m => m.ElementType).Returns(partners.ElementType);
            mockDbSet.As<IQueryable<Merchant>>().Setup(m => m.GetEnumerator()).Returns(partners.GetEnumerator());
            _mockDbContext.Setup(db => db.Merchants).Returns(mockDbSet.Object);

            // Act
            var result = await _partnersController.GetAll();

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetPartner_PartnerFound_ReturnsOk()
        {
            // Arrange
            var partnerId = System.Guid.NewGuid();
            var partner = new Merchant { Id = partnerId, Name = "Test Partner" };
            var mockDbSet = new Mock<DbSet<Merchant>>();
            mockDbSet.Setup(m => m.FindAsync(partnerId)).ReturnsAsync(partner);
            _mockDbContext.Setup(db => db.Merchants).Returns(mockDbSet.Object);

            // Act
            var result = await _partnersController.GetById(partnerId);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetPartner_PartnerNotFound_ReturnsNotFound()
        {
            // Arrange
            var partnerId = System.Guid.NewGuid();
            var mockDbSet = new Mock<DbSet<Merchant>>();
            mockDbSet.Setup(m => m.FindAsync(partnerId)).ReturnsAsync((Merchant)null);
            _mockDbContext.Setup(db => db.Merchants).Returns(mockDbSet.Object);

            // Act
            var result = await _partnersController.GetById(partnerId);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }
    }
} 