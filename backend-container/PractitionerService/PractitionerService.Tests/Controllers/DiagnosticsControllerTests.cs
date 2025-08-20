using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PractitionerService.Controllers;
using PractitionerService.Data;
using Xunit;

namespace PractitionerService.Tests.Controllers;

public class DiagnosticsControllerTests : IDisposable
{
    private readonly PractitionerDbContext _context;
    private readonly DiagnosticsController _controller;

    public DiagnosticsControllerTests()
    {
        var options = new DbContextOptionsBuilder<PractitionerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PractitionerDbContext(options);
        _controller = new DiagnosticsController(_context);
    }

    private PractitionerDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<PractitionerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new PractitionerDbContext(options);
    }

    [Fact]
    public void DiagnosticsController_CanBeInstantiated()
    {
        // Arrange
        var context = GetDbContext();

        // Act
        var controller = new DiagnosticsController(context);

        // Assert
        Assert.NotNull(controller);
    }

    [Fact]
    public void DiagnosticsController_HasCorrectDependencies()
    {
        // Arrange
        var context = GetDbContext();

        // Act & Assert
        var exception = Record.Exception(() => new DiagnosticsController(context));
        Assert.Null(exception);
    }    [Fact]
    public void Migrations_ReturnsExpectedStructure()
    {
        // Act
        var result = _controller.Migrations();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = okResult.Value;
        Assert.NotNull(value);

        // Verify the response has all expected properties
        var responseType = value.GetType();
        Assert.NotNull(responseType.GetProperty("all"));
        Assert.NotNull(responseType.GetProperty("applied"));
        Assert.NotNull(responseType.GetProperty("pending"));
        Assert.NotNull(responseType.GetProperty("historyTable"));
    }

    [Fact]
    public async Task Schema_ReturnsOkResult()
    {
        // Act
        var result = await _controller.Schema();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = okResult.Value;
        Assert.NotNull(value);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
