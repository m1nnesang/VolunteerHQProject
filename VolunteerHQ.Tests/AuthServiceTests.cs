using FluentAssertions;
using Microsoft.Extensions.Configuration;
using VolunteerHQ.Core.DTOs.AuthDTOs;
using VolunteerHQ.Core.Enums;
using VolunteerHQ.Core.Exceptions;
using VolunteerHQ.Core.Models;
using VolunteerHQ.Infrastructure.Data;
using VolunteerHQ.Infrastructure.Services.Classes;

namespace VolunteerHQ.Tests;

public class AuthServiceTests
{
    private static IConfiguration BuildConfig() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "super-secret-test-signing-key-which-is-long-enough-1234567890",
                ["Jwt:Issuer"] = "VolunteerHQ.Tests",
                ["Jwt:Audience"] = "VolunteerHQ.Tests",
                ["Jwt:ExpiresInMinutes"] = "60"
            })
            .Build();

    private static AuthService CreateService(AppDbContext db) =>
        new(db, new ValidatorService(db), BuildConfig());

    [Fact]
    public async Task Register_CreatesUser_AndReturnsToken()
    {
        using var db = TestDb.Create();
        var sut = CreateService(db);

        var dto = new RegisterDto("user@test.com", "Password123", "Ivan", "Petrenko", null);

        var result = await sut.Register(dto);

        result.Token.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.Role.Should().Be(UserRoles.User);

        var stored = db.Users.Single();
        stored.Email.Should().Be("user@test.com");
        stored.PasswordHash.Should().NotBe("Password123", "password must be hashed");
    }

    [Fact]
    public async Task Register_DuplicateEmail_Throws()
    {
        using var db = TestDb.Create();
        db.Users.Add(new UserModel
        {
            Email = "dup@test.com",
            PasswordHash = "x",
            FirstName = "A",
            SecondName = "B"
        });
        await db.SaveChangesAsync();

        var sut = CreateService(db);
        var dto = new RegisterDto("dup@test.com", "Password123", "Ivan", "Petrenko", null);

        var act = async () => await sut.Register(dto);

        await act.Should().ThrowAsync<ConflictEmailException>();
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        using var db = TestDb.Create();
        var sut = CreateService(db);
        await sut.Register(new RegisterDto("login@test.com", "MyPass123", "Ivan", "Petrenko", null));

        var result = await sut.Login(new LoginDto("login@test.com", "MyPass123"));

        result.Token.Should().NotBeNullOrWhiteSpace();
        result.UserId.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Login_WrongPassword_ThrowsUnauthorized()
    {
        using var db = TestDb.Create();
        var sut = CreateService(db);
        await sut.Register(new RegisterDto("login2@test.com", "MyPass123", "Ivan", "Petrenko", null));

        var act = async () => await sut.Login(new LoginDto("login2@test.com", "WrongPass"));

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Login_UnknownEmail_ThrowsNotFound()
    {
        using var db = TestDb.Create();
        var sut = CreateService(db);

        var act = async () => await sut.Login(new LoginDto("ghost@test.com", "whatever"));

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
