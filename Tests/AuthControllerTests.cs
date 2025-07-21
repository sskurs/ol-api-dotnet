using Moq;
using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ol_api_dotnet.Controllers;
using ol_api_dotnet.Data;
using ol_api_dotnet.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ol_api_dotnet.Tests
{
    [TestFixture]
    public class AuthControllerTests
    {
        private Mock<AppDbContext>? _mockDbContext;
        private Mock<IConfiguration>? _mockConfiguration;
        private AuthController? _authController;

        [SetUp]
        public void Setup()
        {
            _mockDbContext = new Mock<AppDbContext>();
            _mockConfiguration = new Mock<IConfiguration>();
            _authController = new AuthController(_mockDbContext.Object, _mockConfiguration.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
            }, "mock"));

            _authController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Test]
        public async Task GetProfile_UserFound_ReturnsOk()
        {
            // Arrange
            var user = new User { Id = 1, Email = "test@example.com", FirstName = "Test", LastName = "User" };
            var mockDbSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<User>>();
            mockDbSet.Setup(m => m.FindAsync(1)).ReturnsAsync(user);
            _mockDbContext.Setup(db => db.Users).Returns(mockDbSet.Object);

            // Act
            var result = await _authController.GetProfile();

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult.Value, Is.Not.Null);
        }

        [Test]
        public async Task GetProfile_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var mockDbSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<User>>();
            mockDbSet.Setup(m => m.FindAsync(1)).ReturnsAsync((User)null);
            _mockDbContext.Setup(db => db.Users).Returns(mockDbSet.Object);

            // Act
            var result = await _authController.GetProfile();

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }
    }
} 