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
    public class RoleControllerTests
    {
        private Mock<AppDbContext>? _mockDbContext;
        private RoleController? _roleController;

        [SetUp]
        public void Setup()
        {
            _mockDbContext = new Mock<AppDbContext>();
            _roleController = new RoleController(_mockDbContext.Object);
        }

        [Test]
        public async Task GetRoles_ReturnsOk()
        {
            // Arrange
            var roles = new List<Role> { new Role { Id = 1, Name = "Test Role" } }.AsQueryable();
            var mockDbSet = new Mock<DbSet<Role>>();
            mockDbSet.As<IQueryable<Role>>().Setup(m => m.Provider).Returns(roles.Provider);
            mockDbSet.As<IQueryable<Role>>().Setup(m => m.Expression).Returns(roles.Expression);
            mockDbSet.As<IQueryable<Role>>().Setup(m => m.ElementType).Returns(roles.ElementType);
            mockDbSet.As<IQueryable<Role>>().Setup(m => m.GetEnumerator()).Returns(roles.GetEnumerator());
            _mockDbContext.Setup(db => db.Roles).Returns(mockDbSet.Object);

            // Act
            var result = await _roleController.List();

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }
    }
} 