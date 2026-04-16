using Microsoft.EntityFrameworkCore;
using VolunteerHQ.Core.Exceptions;
using VolunteerHQ.Core.Models;
using VolunteerHQ.Infrastructure.Data;


namespace VolunteerHQ.Infrastructure.Services;


public class MembershipValidatorService // this class only has a method for validation a member 
{
    private readonly AppDbContext _db;

    public MembershipValidatorService(AppDbContext db)
    {
        _db = db;
    }
    
    public async Task<OrganizationMembershipModel> GetUserOrThrow(int userId , int orgId)
    {
        var user = await _db.OrganizationMemberships.FirstOrDefaultAsync(m => m.UserId == userId && m.OrganizationId == orgId);
        
        if (user == null) throw new NotFoundException("You are not member of this Organization");
        return user;
    }
}