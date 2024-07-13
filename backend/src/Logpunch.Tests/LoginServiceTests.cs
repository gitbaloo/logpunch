using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Persistence;
using Service.Login;
using Shared;
using Domain;
using Xunit;

public class LoginServiceTests
{
    [Fact]
    public async Task AuthorizeLogin_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c["JWT_KEY"]).Returns("my_secret_key_12345my_secret_key_12345");
        mockConfiguration.Setup(c => c["JWT_ISSUER"]).Returns("my_issuer");
        mockConfiguration.Setup(c => c["JWT_AUDIENCE"]).Returns("my_audience");

        using var context = new LogpunchDbContext(options);
        context.Users.Add(TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "test@test.com", "password", "FirstName", "LastName", null, UserRole.Employee));
        await context.SaveChangesAsync();

        var service = new LoginService(mockConfiguration.Object, context);

        // Act
        var token = await service.AuthorizeLogin("test@test.com", "password");

        // Assert
        Assert.NotNull(token);
    }

    [Fact]
    public async Task AuthorizeLogin_InvalidCredentials_ThrowsException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        var mockConfiguration = new Mock<IConfiguration>();

        using var context = new LogpunchDbContext(options);
        context.Users.Add(TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "test@test.com", "password", "FirstName", "LastName", null, UserRole.Employee));
        await context.SaveChangesAsync();

        var service = new LoginService(mockConfiguration.Object, context);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.AuthorizeLogin("test@test.com", "wrongpassword"));
    }

    [Fact]
    public async Task ValidateToken_ValidToken_ReturnsUser()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c["JWT_KEY"]).Returns("my_secret_key_12345my_secret_key_12345"); // Updated key length to 32 characters

        using var context = new LogpunchDbContext(options);
        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "test@test.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new LoginService(mockConfiguration.Object, context);
        var token = await service.AuthorizeLogin(user.Email, "password");

        // Act
        var validatedUser = await service.ValidateToken(token);

        // Assert
        Assert.Equal(user.Email, validatedUser.Email);
    }
}
