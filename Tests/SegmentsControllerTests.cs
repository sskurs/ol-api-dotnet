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
    public class SegmentsControllerTests
    {
        private Mock<AppDbContext>? _mockDbContext;
        private SegmentsController? _segmentsController;

        [SetUp]
        public void Setup()
        {
            _mockDbContext = new Mock<AppDbContext>();
            _segmentsController = new SegmentsController(_mockDbContext.Object);
        }

        [Test]
        public async Task GetSegments_ReturnsOk()
        {
            // Arrange
            var segments = new List<Segment> { new Segment { Id = System.Guid.NewGuid(), Name = "Test Segment" } }.AsQueryable();
            var mockDbSet = new Mock<DbSet<Segment>>();
            mockDbSet.As<IQueryable<Segment>>().Setup(m => m.Provider).Returns(segments.Provider);
            mockDbSet.As<IQueryable<Segment>>().Setup(m => m.Expression).Returns(segments.Expression);
            mockDbSet.As<IQueryable<Segment>>().Setup(m => m.ElementType).Returns(segments.ElementType);
            mockDbSet.As<IQueryable<Segment>>().Setup(m => m.GetEnumerator()).Returns(segments.GetEnumerator());
            _mockDbContext.Setup(db => db.Segments).Returns(mockDbSet.Object);

            // Act
            var result = await _segmentsController.List();

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetSegment_SegmentFound_ReturnsOk()
        {
            // Arrange
            var segmentId = System.Guid.NewGuid();
            var segment = new Segment { Id = segmentId, Name = "Test Segment" };
            var mockDbSet = new Mock<DbSet<Segment>>();
            mockDbSet.Setup(m => m.FindAsync(segmentId)).ReturnsAsync(segment);
            _mockDbContext.Setup(db => db.Segments).Returns(mockDbSet.Object);

            // Act
            var result = await _segmentsController.Get(segmentId);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetSegment_SegmentNotFound_ReturnsNotFound()
        {
            // Arrange
            var segmentId = System.Guid.NewGuid();
            var mockDbSet = new Mock<DbSet<Segment>>();
            mockDbSet.Setup(m => m.FindAsync(segmentId)).ReturnsAsync((Segment)null);
            _mockDbContext.Setup(db => db.Segments).Returns(mockDbSet.Object);

            // Act
            var result = await _segmentsController.Get(segmentId);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }
    }
} 