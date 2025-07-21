using Moq;
using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using ol_api_dotnet.Controllers;
using ol_api_dotnet.Data;
using ol_api_dotnet.Models;
using ol_api_dotnet.Services;
using System.Threading.Tasks;
using static ol_api_dotnet.Controllers.EarningRulesController;
using Microsoft.EntityFrameworkCore;

namespace ol_api_dotnet.Tests
{
    [TestFixture]
    public class EarningRulesControllerTests
    {
        private Mock<AppDbContext>? _mockDbContext;
        private Mock<EarningRuleEngineService>? _mockEarningRuleEngineService;
        private EarningRulesController? _earningRulesController;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
            _mockDbContext = new Mock<AppDbContext>(options);
            _mockEarningRuleEngineService = new Mock<EarningRuleEngineService>();
            _earningRulesController = new EarningRulesController(_mockDbContext.Object, _mockEarningRuleEngineService.Object);
        }

        [Test]
        public async Task CreateRule_ValidRequest_ReturnsOk()
        {
            // Arrange
            var request = new CreateEarningRuleRequest { Name = "Test Rule", RuleJson = "{}" };
            _mockEarningRuleEngineService.Setup(s => s.ValidateRuleJson(It.IsAny<string>())).Returns(true);
            _mockDbContext.Setup(db => db.EarningRules.Add(It.IsAny<EarningRule>()));
            _mockDbContext.Setup(db => db.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            var result = await _earningRulesController.CreateRule(request);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task CreateRule_InvalidJson_ReturnsBadRequest()
        {
            // Arrange
            var request = new CreateEarningRuleRequest { Name = "Test Rule", RuleJson = "invalid" };
            _mockEarningRuleEngineService.Setup(s => s.ValidateRuleJson(It.IsAny<string>())).Returns(false);

            // Act
            var result = await _earningRulesController.CreateRule(request);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
        }
    }
} 