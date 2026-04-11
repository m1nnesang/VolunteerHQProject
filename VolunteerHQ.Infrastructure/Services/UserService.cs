using Microsoft.EntityFrameworkCore;
using VolunteerHQ.Core.DTOs.UserDTOs;
using VolunteerHQ.Core.Exceptions;
using VolunteerHQ.Infrastructure.Data;

namespace VolunteerHQ.Infrastructure.Services;

public class UserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db)
    {
        _db = db;
    }

public async Task<UserResponseDto> GetMe(int userId)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        
        if (user == null)
        {
            throw new NotFoundException("User with this id is not found");
        }

        return new UserResponseDto(user.Id, user.Email, user.FirstName, user.SecondName, user.BirthDate, user.Role);
    }
}