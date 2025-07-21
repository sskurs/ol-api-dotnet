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
using System;

namespace ol_api_dotnet.Tests
{
    [TestFixture]
    public class AdminControllerTests
    {
        private AppDbContext? _dbContext;
        private AdminController? _adminController;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new AppDbContext(options);
            _dbContext.Admins.RemoveRange(_dbContext.Admins);
            _dbContext.SaveChanges();
            _adminController = new AdminController(_dbContext);
        }

        [Test]
        public async Task GetAdmins_ReturnsOk()
        {
            // Arrange
            _dbContext.Admins.Add(new Admin { Id = 1, FirstName = "Test", LastName = "Admin" });
            _dbContext.SaveChanges();

            // Act
            var result = await _adminController.List();

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetAdmin_AdminFound_ReturnsOk()
        {
            // Arrange
            _dbContext.Admins.Add(new Admin { Id = 1, FirstName = "Test", LastName = "Admin" });
            _dbContext.SaveChanges();

            // Act
            var result = await _adminController.Get(1);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetAdmin_AdminNotFound_ReturnsNotFound()
        {
            // Act
            var result = await _adminController.Get(1);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }
    }
} 