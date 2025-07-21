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
    public class CampaignControllerTests
    {
        private Mock<AppDbContext>? _mockDbContext;
        private CampaignController? _campaignController;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
            _mockDbContext = new Mock<AppDbContext>(options);
            _campaignController = new CampaignController(_mockDbContext.Object);
        }

        [Test]
        public async Task GetCampaigns_ReturnsOk()
        {
            // Arrange
            var campaigns = new List<Campaign> { new Campaign { CampaignId = System.Guid.NewGuid(), Name = "Test Campaign" } }.AsQueryable();
            var mockDbSet = new Mock<DbSet<Campaign>>();
            mockDbSet.As<IQueryable<Campaign>>().Setup(m => m.Provider).Returns(campaigns.Provider);
            mockDbSet.As<IQueryable<Campaign>>().Setup(m => m.Expression).Returns(campaigns.Expression);
            mockDbSet.As<IQueryable<Campaign>>().Setup(m => m.ElementType).Returns(campaigns.ElementType);
            mockDbSet.As<IQueryable<Campaign>>().Setup(m => m.GetEnumerator()).Returns(campaigns.GetEnumerator());
            _mockDbContext.Setup(db => db.Campaigns).Returns(mockDbSet.Object);

            // Act
            var result = await _campaignController.List();

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetCampaign_CampaignFound_ReturnsOk()
        {
            // Arrange
            var campaignId = System.Guid.NewGuid();
            var campaign = new Campaign { CampaignId = campaignId, Name = "Test Campaign" };
            var mockDbSet = new Mock<DbSet<Campaign>>();
            mockDbSet.Setup(m => m.FindAsync(campaignId)).ReturnsAsync(campaign);
            _mockDbContext.Setup(db => db.Campaigns).Returns(mockDbSet.Object);

            // Act
            var result = await _campaignController.Get(campaignId);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetCampaign_CampaignNotFound_ReturnsNotFound()
        {
            // Arrange
            var campaignId = System.Guid.NewGuid();
            var mockDbSet = new Mock<DbSet<Campaign>>();
            mockDbSet.Setup(m => m.FindAsync(campaignId)).ReturnsAsync((Campaign)null);
            _mockDbContext.Setup(db => db.Campaigns).Returns(mockDbSet.Object);

            // Act
            var result = await _campaignController.Get(campaignId);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }
    }
} 