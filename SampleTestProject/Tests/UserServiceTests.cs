using NUnit.Framework;
using Moq;
using SampleTestProject.Models;
using SampleTestProject.Services;

namespace SampleTestProject.Tests;

[TestFixture]
public class UserServiceTests
{
    // ❌ Test MALO - Assertion trivial
    [Test]
    public void Test1()
    {
        Assert.That(true, Is.True);
    }

    // ❌ Test MALO - Solo mocks, no valida comportamiento real
    [Test]
    public void TestUser()
    {
        var mock = new Mock<IUserService>();
        mock.Setup(x => x.GetUser(1)).Returns(new User { Id = 1, Name = "Test" });
        mock.Verify(x => x.GetUser(1), Times.Once());
    }

    // ❌ Test MALO - Nunca falla
    [Test]
    public void TestNeverFails()
    {
        try
        {
            var service = new UserService();
            var result = service.GetUser(999);
        }
        catch (Exception)
        {
            // Silently catch - test never fails
        }
        Assert.That(true, Is.True);
    }

    // ❌ Test MALO - Múltiples responsabilidades, complejo
    [Test]
    public void TestComplexMethod()
    {
        var service = new UserService();

        // Test create
        var userData = new UserData { Name = "Test", Email = "test@test.com" };
        var user = service.CreateUser(userData);
        Assert.That(user, Is.Not.Null);
        Assert.That(user.Name, Is.EqualTo("Test"));

        // Test get
        var retrievedUser = service.GetUser(user.Id);
        Assert.That(retrievedUser.Id, Is.EqualTo(user.Id));

        // Test update
        var updateData = new UserData { Name = "Updated", Email = "updated@test.com" };
        service.UpdateUser(user.Id, updateData);
        var updatedUser = service.GetUser(user.Id);
        Assert.That(updatedUser.Name, Is.EqualTo("Updated"));

        // Test delete
        service.DeleteUser(user.Id);
        Assert.Throws<ArgumentException>(() => service.GetUser(user.Id));
    }

    // ✅ Test BUENO - Completitud alta
    [Test]
    public void Should_CreateUser_When_ValidData()
    {
        // Arrange
        var userService = new UserService();
        var userData = new UserData { Name = "Test User", Email = "test@example.com" };

        // Act
        var result = userService.CreateUser(userData);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("Test User"));
        Assert.That(result.Email, Is.EqualTo("test@example.com"));
        Assert.That(result.Id, Is.GreaterThan(0));
        Assert.That(result.CreatedAt, Is.LessThanOrEqualTo(DateTime.Now));
    }

    [Test]
    public void Should_ThrowValidationException_When_EmptyName()
    {
        // Arrange
        var userService = new UserService();
        var userData = new UserData { Name = "", Email = "test@example.com" };

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => userService.CreateUser(userData));
        Assert.That(exception.Message, Is.EqualTo("Name is required"));
    }

    [Test]
    public void Should_ThrowValidationException_When_InvalidEmail()
    {
        // Arrange
        var userService = new UserService();
        var userData = new UserData { Name = "Test User", Email = "invalid-email" };

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => userService.CreateUser(userData));
        Assert.That(exception.Message, Is.EqualTo("Invalid email format"));
    }

    [Test]
    public void Should_ThrowValidationException_When_NullEmail()
    {
        // Arrange
        var userService = new UserService();
        var userData = new UserData { Name = "Test User", Email = null! };

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => userService.CreateUser(userData));
        Assert.That(exception.Message, Is.EqualTo("Email is required"));
    }

    [Test]
    public void Should_GetUser_When_ValidId()
    {
        // Arrange
        var userService = new UserService();
        var userData = new UserData { Name = "Test User", Email = "test@example.com" };
        var createdUser = userService.CreateUser(userData);

        // Act
        var result = userService.GetUser(createdUser.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(createdUser.Id));
        Assert.That(result.Name, Is.EqualTo("Test User"));
        Assert.That(result.Email, Is.EqualTo("test@example.com"));
    }

    [Test]
    public void Should_ThrowArgumentException_When_UserNotFound()
    {
        // Arrange
        var userService = new UserService();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => userService.GetUser(999));
        Assert.That(exception.Message, Does.Contain("User with id 999 not found"));
    }
}
