using Microsoft.EntityFrameworkCore;
using VolunteerHQ.Core.DTOs.UserDTOs;
using VolunteerHQ.Core.Exceptions;
using VolunteerHQ.Infrastructure.Data;

namespace VolunteerHQ.Infrastructure.Services;

public class UserService
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
}