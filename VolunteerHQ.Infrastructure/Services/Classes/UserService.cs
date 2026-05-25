using Microsoft.EntityFrameworkCore;
using VolunteerHQ.Core.DTOs.UserDTOs;
using VolunteerHQ.Infrastructure.Data;
using VolunteerHQ.Infrastructure.Services.Interfaces;

namespace VolunteerHQ.Infrastructure.Services.Classes;

public class UserService : IUserService
{
    private readonly AppDbContext _db;
    private readonly ValidatorService _vs; // validation service!
    

    public UserService(AppDbContext db , ValidatorService vs)
    {
        _db = db;
        _vs = vs;
    }

    public async Task<UserResponseDto> GetMe(int userId, CancellationToken ct = default)
    {
        var user = await _vs.GetUserByIdOrThrow(userId, ct);

        return new UserResponseDto(user.Id, user.Email, user.FirstName, user.SecondName, user.BirthDate, user.Role);
    }

    public async Task<UserStatsDto> GetMyStats(int userId, CancellationToken ct = default)
    {
        var totalDonated = await _db.Donations
            .Where(d => d.UserId == userId)
            .SumAsync(d => (decimal?)d.Amount, ct) ?? 0m;

        var donationsCount = await _db.Donations
            .CountAsync(d => d.UserId == userId, ct);

        var fundraisersSupported = await _db.Donations
            .Where(d => d.UserId == userId)
            .Select(d => d.FundraiserId)
            .Distinct()
            .CountAsync(ct);

        return new UserStatsDto(totalDonated, donationsCount, fundraisersSupported);
    }
}