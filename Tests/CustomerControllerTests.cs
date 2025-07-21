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
    public class CustomerControllerTests
    {
        private Mock<AppDbContext>? _mockDbContext;
        private CustomerController? _customerController;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
            _mockDbContext = new Mock<AppDbContext>(options);
            _customerController = new CustomerController(_mockDbContext.Object);
        }

        [Test]
        public async Task GetCustomers_ReturnsOk()
        {
            // Arrange
            var customers = new List<User> { new User { Id = 1, FirstName = "Test", LastName = "Customer" } }.AsQueryable();
            var mockDbSet = new Mock<DbSet<User>>();
            mockDbSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(customers.Provider);
            mockDbSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(customers.Expression);
            mockDbSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(customers.ElementType);
            mockDbSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(customers.GetEnumerator());
            _mockDbContext.Setup(db => db.Users).Returns(mockDbSet.Object);

            // Act
            var result = await _customerController.GetAll();

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }
    }
} 