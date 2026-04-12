using Microsoft.EntityFrameworkCore;
using VolunteerHQ.Core.DTOs.AuthDTOs;
using VolunteerHQ.Infrastructure.Data;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using VolunteerHQ.Core.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using VolunteerHQ.Core.DTOs.UserDTOs;
using VolunteerHQ.Core.Exceptions;

namespace VolunteerHQ.Infrastructure.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;

    public  AuthService(AppDbContext db, IConfiguration iConfiguration)
    {
        _db = db;
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
        
        return new AuthResponseDto(user.Id, user.Role, token);
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

        return new AuthResponseDto(user.Id , user.Role , token);
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
}