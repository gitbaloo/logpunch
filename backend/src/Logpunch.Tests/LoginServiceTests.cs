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
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c["JWT_KEY"]).Returns("my_secret_key_12345");
        mockConfiguration.Setup(c => c["JWT_ISSUER"]).Returns("my_issuer");
        mockConfiguration.Setup(c => c["JWT_AUDIENCE"]).Returns("my_audience");

        using var context = new LogpunchDbContext(options);
        context.Users.Add(new LogpunchUser { Email = "test@test.com", Password = "password" });
        await context.SaveChangesAsync();

        var service = new LoginService(mockConfiguration.Object, context);

        var token = await service.AuthorizeLogin("test@test.com", "password");

        Assert.NotNull(token);
    }

    [Fact]
    public async Task AuthorizeLogin_InvalidCredentials_ThrowsException()
    {
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        var mockConfiguration = new Mock<IConfiguration>();

        using var context = new LogpunchDbContext(options);
        context.Users.Add(new LogpunchUser { Id = Guid.NewGuid(), Email = "test@test.com", Password = "password" });
        await context.SaveChangesAsync();

        var service = new LoginService(mockConfiguration.Object, context);

        await Assert.ThrowsAsync<ArgumentException>(() => service.AuthorizeLogin("test@test.com", "wrongpassword"));
    }

    [Fact]
    public async Task ValidateToken_ValidToken_ReturnsUser()
    {
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c["JWT_KEY"]).Returns("my_secret_key_12345");

        using var context = new LogpunchDbContext(options);
        var user = new LogpunchUser { Id = Guid.NewGuid(), Email = "test@test.com" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new LoginService(mockConfiguration.Object, context);
        var token = service.AuthorizeLogin(user.Email, "password").Result;

        var validatedUser = await service.ValidateToken(token);

        Assert.Equal(user.Email, validatedUser.Email);
    }
}
