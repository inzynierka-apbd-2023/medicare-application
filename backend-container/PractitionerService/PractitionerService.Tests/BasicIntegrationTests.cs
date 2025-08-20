using Xunit;
using Microsoft.EntityFrameworkCore;
using PractitionerService.Data;
using PractitionerService.Models;

namespace PractitionerService.Tests;

public class BasicIntegrationTests
{
    [Fact]
    public async Task Database_Configuration_Works()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<PractitionerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new PractitionerDbContext(options);

        // Act
        var specialization = new Specialization
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Specialization"
        };

        context.Specializations.Add(specialization);
        await context.SaveChangesAsync();

        // Assert
        var savedSpecialization = await context.Specializations.FirstOrDefaultAsync();
        Assert.NotNull(savedSpecialization);
        Assert.Equal("Test Specialization", savedSpecialization.Name);
    }

    [Fact]
    public async Task Doctor_Entity_Can_Be_Created()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<PractitionerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new PractitionerDbContext(options);

        // Act
        var doctor = new Doctor
        {
            Id = Guid.NewGuid().ToString(),
            UserId = Guid.NewGuid().ToString(),
            Bio = "Test doctor biography",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Doctors.Add(doctor);
        await context.SaveChangesAsync();

        // Assert
        var savedDoctor = await context.Doctors.FirstOrDefaultAsync();
        Assert.NotNull(savedDoctor);
        Assert.Equal("Test doctor biography", savedDoctor.Bio);
    }

    [Fact]
    public async Task Receptionist_Entity_Can_Be_Created()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<PractitionerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new PractitionerDbContext(options);

        // Act
        var receptionist = new Receptionist
        {
            Id = Guid.NewGuid().ToString(),
            UserId = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Receptionists.Add(receptionist);
        await context.SaveChangesAsync();

        // Assert
        var savedReceptionist = await context.Receptionists.FirstOrDefaultAsync();
        Assert.NotNull(savedReceptionist);
        Assert.Equal(receptionist.Id, savedReceptionist.Id);
    }

    [Fact]
    public void Entity_Ids_Are_Strings()
    {
        // Arrange
        var doctor = new Doctor 
        { 
            Id = Guid.NewGuid().ToString(),
            UserId = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var receptionist = new Receptionist 
        { 
            Id = Guid.NewGuid().ToString(),
            UserId = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var specialization = new Specialization 
        { 
            Id = Guid.NewGuid().ToString(),
            Name = "Test Specialization"
        };
        var service = new MedicalService 
        { 
            Id = Guid.NewGuid().ToString(),
            Name = "Test Service"
        };

        // Act & Assert - Verify that all IDs are strings (not GUIDs)
        Assert.True(doctor.Id is string);
        Assert.True(receptionist.Id is string);
        Assert.True(specialization.Id is string);
        Assert.True(service.Id is string);
        
        // Verify the string IDs are valid GUIDs
        Assert.True(Guid.TryParse(doctor.Id, out _));
        Assert.True(Guid.TryParse(receptionist.Id, out _));
        Assert.True(Guid.TryParse(specialization.Id, out _));
        Assert.True(Guid.TryParse(service.Id, out _));
    }
}
