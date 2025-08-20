using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PractitionerService.Controllers;
using PractitionerService.Data;
using PractitionerService.Models;
using Xunit;

namespace PractitionerService.Tests.Controllers;

public class ReceptionistsControllerTests : IDisposable
{
    private readonly PractitionerDbContext _context;
    private readonly ReceptionistsController _controller;

    public ReceptionistsControllerTests()
    {
        var options = new DbContextOptionsBuilder<PractitionerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PractitionerDbContext(options);
        _controller = new ReceptionistsController(_context);
    }

    [Fact]
    public async Task Register_ValidRequest_ReturnsCreatedResult()
    {
        // Arrange
        var request = new RegisterReceptionistRequest(Guid.NewGuid());

        // Act
        var result = await _controller.Register(request);

        // Assert
        Assert.IsType<CreatedAtActionResult>(result);
        var createdResult = result as CreatedAtActionResult;
        Assert.NotNull(createdResult);
        Assert.Equal("GetReceptionistById", createdResult.ActionName);
    }

    [Fact]
    public async Task Register_EmptyUserId_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterReceptionistRequest(Guid.Empty);

        // Act
        var result = await _controller.Register(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Register_DuplicateUserId_ReturnsConflict()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request1 = new RegisterReceptionistRequest(userId);
        var request2 = new RegisterReceptionistRequest(userId);

        // Act
        await _controller.Register(request1);
        var result = await _controller.Register(request2);

        // Assert
        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Register_CreatesReceptionistWithCorrectTimestamps()
    {
        // Arrange
        var request = new RegisterReceptionistRequest(Guid.NewGuid());
        var beforeRegister = DateTime.UtcNow;

        // Act
        var result = await _controller.Register(request);

        // Assert
        var afterRegister = DateTime.UtcNow;
        Assert.IsType<CreatedAtActionResult>(result);
        
        // Verify a receptionist was created in the database
        var receptionist = await _context.Receptionists.FirstOrDefaultAsync(r => r.UserId == request.UserId.ToString());
        Assert.NotNull(receptionist);
        Assert.True(receptionist.CreatedAt >= beforeRegister && receptionist.CreatedAt <= afterRegister);
        Assert.True(receptionist.UpdatedAt >= beforeRegister && receptionist.UpdatedAt <= afterRegister);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
