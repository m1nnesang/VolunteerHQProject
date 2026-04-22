using Microsoft.EntityFrameworkCore;
using VolunteerHQ.Core.DTOs.AuthDTOs;
using VolunteerHQ.Infrastructure.Data;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using VolunteerHQ.Core.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using VolunteerHQ.Core.Exceptions;
using VolunteerHQ.Infrastructure.Services.Interfaces;


namespace VolunteerHQ.Infrastructure.Services.Classes;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly ValidatorService _vs;
    private readonly IConfiguration _configuration;

    public  AuthService(AppDbContext db,ValidatorService vs ,  IConfiguration iConfiguration)
    {
        _db = db;
        _vs = vs;
        _configuration = iConfiguration;
    }
    
    
    #region AuthResponseDto
    public async Task<AuthResponseDto> Register(RegisterDto dto)
    {
        var emailExist = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email); //email check

        if (emailExist != null)
        {
            throw new ConflictEmailException("Email is already in use");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var user = new UserModel
        {
            Email = dto.Email,
            PasswordHash = passwordHash,
            FirstName = dto.FirstName,
            SecondName = dto.SecondName,
            BirthDate = dto.BirthDate,
            Role = 0,
            CreatedAt = DateTime.UtcNow,
        };

        await _db.AddAsync(user);
        await _db.SaveChangesAsync();

        var token = GenerateToken(user);
        var refreshToken = await CreateRefreshToken(user.Id);
       
        await _db.SaveChangesAsync();
        
        return new AuthResponseDto(user.Id, user.Role, token , refreshToken);
    }

    public async Task<AuthResponseDto> Login(LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null)
        {
            throw new NotFoundException("User with this email is not found");
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid Password");
        }
        
        var token = GenerateToken(user);
        var refreshToken = await CreateRefreshToken(user.Id);
        
        await _db.SaveChangesAsync();

        return new AuthResponseDto(user.Id , user.Role , token , refreshToken);
    }

    public async Task<AuthResponseDto> Refresh(string refreshToken)
    {
        var token = await _vs.GetRefreshTokenOrThrow(refreshToken);

        if (token.IsRevoked)
            throw new UnauthorizedException("Refresh token has been revoked");

        if (token.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedException("Refresh token has expired");

        token.IsRevoked = true;

        var user = await _vs.GetUserByIdOrThrow(token.UserId);
        var accessToken = GenerateToken(user);
        var newRefreshToken = await CreateRefreshToken(token.UserId);

        await _db.SaveChangesAsync();

        return new AuthResponseDto(user.Id, user.Role, accessToken, newRefreshToken);
    }
    
    
    
    #endregion
    
    private string GenerateToken(UserModel user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));


        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken
        (
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpiresInMinutes"]!)),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    private string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }
    
    private async Task<string> CreateRefreshToken(int userId)
    {
        var token = GenerateRefreshToken();
    
        var model = new RefreshTokenModel
        {
            Token = token,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };
    
        await _db.AddAsync(model);
        return token;
    }
}