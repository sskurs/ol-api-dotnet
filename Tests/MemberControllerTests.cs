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
    public class MemberControllerTests
    {
        private Mock<AppDbContext>? _mockDbContext;
        private MemberController? _memberController;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
            _mockDbContext = new Mock<AppDbContext>(options);
            _memberController = new MemberController(_mockDbContext.Object);
        }

        [Test]
        public async Task GetMembers_ReturnsOk()
        {
            // Arrange
            var members = new List<User> { new User { Id = 1, FirstName = "Test", LastName = "Member" } }.AsQueryable();
            var mockDbSet = new Mock<DbSet<User>>();
            mockDbSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(members.Provider);
            mockDbSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(members.Expression);
            mockDbSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(members.ElementType);
            mockDbSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(members.GetEnumerator());
            _mockDbContext.Setup(db => db.Users).Returns(mockDbSet.Object);

            // Act
            var result = await _memberController.List();

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetMember_MemberFound_ReturnsOk()
        {
            // Arrange
            var member = new User { Id = 1, FirstName = "Test", LastName = "Member" };
            var mockDbSet = new Mock<DbSet<User>>();
            mockDbSet.Setup(m => m.FindAsync(1)).ReturnsAsync(member);
            _mockDbContext.Setup(db => db.Users).Returns(mockDbSet.Object);

            // Act
            var result = await _memberController.Get(1);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetMember_MemberNotFound_ReturnsNotFound()
        {
            // Arrange
            var mockDbSet = new Mock<DbSet<User>>();
            mockDbSet.Setup(m => m.FindAsync(1)).ReturnsAsync((User)null);
            _mockDbContext.Setup(db => db.Users).Returns(mockDbSet.Object);

            // Act
            var result = await _memberController.Get(1);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }
    }
} 