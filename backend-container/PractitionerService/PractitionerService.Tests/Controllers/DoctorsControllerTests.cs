using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PractitionerService.Controllers;
using PractitionerService.Data;
using PractitionerService.Models;
using Xunit;

namespace PractitionerService.Tests.Controllers;

public class DoctorsControllerTests : IDisposable
{
    private readonly PractitionerDbContext _context;
    private readonly DoctorsController _controller;

    public DoctorsControllerTests()
    {
        var options = new DbContextOptionsBuilder<PractitionerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PractitionerDbContext(options);
        _controller = new DoctorsController(_context);
    }

    [Fact]
    public async Task RegisterDoctor_ValidRequest_ReturnsCreatedResult()
    {
        // Arrange
        var request = new RegisterDoctorRequest(Guid.NewGuid(), "Test Bio");

        // Act
        var result = await _controller.RegisterDoctor(request);

        // Assert
        Assert.IsType<CreatedAtActionResult>(result);
        var createdResult = result as CreatedAtActionResult;
        Assert.NotNull(createdResult);
        Assert.Equal("GetDoctorById", createdResult.ActionName);
    }

    [Fact]
    public async Task RegisterDoctor_EmptyUserId_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterDoctorRequest(Guid.Empty, "Test Bio");

        // Act
        var result = await _controller.RegisterDoctor(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task RegisterDoctor_DuplicateUserId_ReturnsConflict()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request1 = new RegisterDoctorRequest(userId, "Test Bio 1");
        var request2 = new RegisterDoctorRequest(userId, "Test Bio 2");

        // Act
        await _controller.RegisterDoctor(request1);
        var result = await _controller.RegisterDoctor(request2);

        // Assert
        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task GetDoctorById_ValidId_ReturnsOkResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new RegisterDoctorRequest(userId, "Test Bio");
        var registerResult = await _controller.RegisterDoctor(request);
        var createdResult = registerResult as CreatedAtActionResult;
        var doctorId = createdResult?.RouteValues?["id"]?.ToString();

        // Act
        var result = await _controller.GetDoctorById(doctorId!);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetDoctorById_NonExistentId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = "non-existent-id";

        // Act
        var result = await _controller.GetDoctorById(nonExistentId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Search_WithNoFilters_ReturnsOkResult()
    {
        // Arrange & Act
        var result = await _controller.Search(null, null, null);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Search_WithQuery_ReturnsOkResult()
    {
        // Arrange & Act
        var result = await _controller.Search(null, null, "john");

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetAvailability_NonExistentDoctor_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = "non-existent-id";

        // Act
        var result = await _controller.GetAvailability(nonExistentId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    private PractitionerDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<PractitionerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new PractitionerDbContext(options);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
