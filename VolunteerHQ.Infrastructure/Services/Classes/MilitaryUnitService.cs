using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using VolunteerHQ.Core.DTOs.MilitaryDTOs;
using VolunteerHQ.Core.Enums;
using VolunteerHQ.Core.Exceptions;
using VolunteerHQ.Core.Models;
using VolunteerHQ.Infrastructure.Data;
using VolunteerHQ.Infrastructure.Services.Interfaces;

namespace VolunteerHQ.Infrastructure.Services.Classes;

public class MilitaryUnitService : IMilitaryUnitService
{
    private readonly AppDbContext _db;
    private readonly ValidatorService _vs;
    private readonly IConfiguration _configuration;

    public MilitaryUnitService(AppDbContext db, ValidatorService vs, IConfiguration iConfiguration)
    {
        _db = db;
        _vs = vs;
         _configuration = iConfiguration;
    }

    public async Task<MilitaryUnitResponseDto> CreateUnit(RegisterMilitaryUnitDto dto, int adminId,
        CancellationToken ct = default)
    {
        await _vs.AdminOrThrow(adminId, ct);

        var checkLogin = await _db.MilitaryUnits.FirstOrDefaultAsync(u => u.Login == dto.Login, ct);
        if (checkLogin != null) throw new ConflictException("Unit with this login already exist");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var unit = new MilitaryUnitModel()
        {
            UnitName = dto.UnitName,
            ContactPersonName = dto.ContactPerson,
            Login = dto.Login,
            PasswordHash = passwordHash,
            IsNameHidden = dto.IsNameHidden,
            CreatedAt = DateTime.UtcNow,
        };

        _db.MilitaryUnits.Add(unit);
        await _db.SaveChangesAsync(ct);

        return new MilitaryUnitResponseDto(unit.Id, unit.UnitName, unit.CreatedAt, unit.ContactPersonName);
    }

    public async Task<string> Login(LogMilitaryUnitDto dto, CancellationToken ct = default)
    {
        var unit = await _db.MilitaryUnits.FirstOrDefaultAsync(u => u.Login == dto.Login, ct);

        if (unit == null) throw new NotFoundException("Unit with this login is not found");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, unit.PasswordHash))
            throw new UnauthorizedException("Invalid Password");

        var token = GenerateMilitaryToken(unit);
        
        return token;
    }

    public async Task<MilitaryUnitResponseDto> GetUnit(int unitId , int? userId, CancellationToken ct = default)
    {
        var unit = await _db.MilitaryUnits.FirstOrDefaultAsync(u => u.Id == unitId, ct);
        if (unit == null) throw new NotFoundException("Unit with this id is not found");

        var userDeputyOrLead = await _db.OrganizationMemberships
            .FirstOrDefaultAsync(m => m.UserId == userId && 
                                      (m.MemberRole == OrganizationMemberRole.Deputy || 
                                       m.MemberRole == OrganizationMemberRole.Leader), ct);
        
        var unitName = unit.IsNameHidden ? "********" : unit.UnitName;
        
        var contactPerson = userDeputyOrLead != null ? unit.ContactPersonName : "********";
        
        return new MilitaryUnitResponseDto(unit.Id, unitName, unit.CreatedAt, contactPerson);
    }



    private string GenerateMilitaryToken(MilitaryUnitModel unit)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, unit.Id.ToString()),
            new Claim(ClaimTypes.Role, "MilitaryUnit")
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken
        (
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
