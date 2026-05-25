using FluentAssertions;
using Moq;
using VolunteerHQ.Core.DTOs.JoinRequestDTOs;
using VolunteerHQ.Core.Enums;
using VolunteerHQ.Core.Exceptions;
using VolunteerHQ.Core.Models;
using VolunteerHQ.Infrastructure.Data;
using VolunteerHQ.Infrastructure.Services.Classes;
using VolunteerHQ.Infrastructure.Services.Interfaces;

namespace VolunteerHQ.Tests;

public class JoinRequestServiceTests
{
    private static JoinRequestService CreateService(
        AppDbContext db,
        out Mock<INotificationService> ns,
        out Mock<IAuditLogService> als)
    {
        ns = new Mock<INotificationService>();
        als = new Mock<IAuditLogService>();
        return new JoinRequestService(db, new ValidatorService(db), ns.Object, als.Object);
    }

    private static async Task<(int userId, int orgId)> SeedUserAndOrg(AppDbContext db)
    {
        var user = new UserModel { Email = "u@test.com", PasswordHash = "x", FirstName = "A", SecondName = "B" };
        var org = new OrganizationModel { OrganizationName = "Org", City = "Kyiv", Description = "d", CreatedAt = DateTime.UtcNow };
        db.Users.Add(user);
        db.Organizations.Add(org);
        await db.SaveChangesAsync();
        return (user.Id, org.Id);
    }

    [Fact]
    public async Task CreateJoinRequest_Persists_PendingRequest()
    {
        using var db = TestDb.Create();
        var (userId, orgId) = await SeedUserAndOrg(db);
        var sut = CreateService(db, out _, out _);

        var dto = new CreateJoinRequestDto("bio", "skills", "exp", "");

        var result = await sut.CreateJoinRequest(userId, orgId, dto);

        result.Status.Should().Be(RequestStatus.Pending);
        db.JoinRequests.Should().ContainSingle(r => r.UserId == userId && r.OrganizationId == orgId);
    }

    [Fact]
    public async Task CreateJoinRequest_WhenAlreadyMember_Throws()
    {
        using var db = TestDb.Create();
        var (userId, orgId) = await SeedUserAndOrg(db);
        db.OrganizationMemberships.Add(new OrganizationMembershipModel
        {
            UserId = userId,
            OrganizationId = orgId,
            MemberRole = OrganizationMemberRole.Member,
            JoinedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var sut = CreateService(db, out _, out _);

        var act = async () => await sut.CreateJoinRequest(userId, orgId, new CreateJoinRequestDto("b", "s", "e", ""));

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CreateJoinRequest_WhenPendingExists_Throws()
    {
        using var db = TestDb.Create();
        var (userId, orgId) = await SeedUserAndOrg(db);
        var sut = CreateService(db, out _, out _);

        await sut.CreateJoinRequest(userId, orgId, new CreateJoinRequestDto("b", "s", "e", ""));

        var act = async () => await sut.CreateJoinRequest(userId, orgId, new CreateJoinRequestDto("b", "s", "e", ""));

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task ReviewJoinRequest_Approved_AddsMember_AndPromotesUser_AndNotifies()
    {
        using var db = TestDb.Create();
        var (userId, orgId) = await SeedUserAndOrg(db);

        var reviewer = new UserModel { Email = "lead@test.com", PasswordHash = "x", FirstName = "L", SecondName = "D" };
        db.Users.Add(reviewer);
        await db.SaveChangesAsync();
        db.OrganizationMemberships.Add(new OrganizationMembershipModel
        {
            UserId = reviewer.Id,
            OrganizationId = orgId,
            MemberRole = OrganizationMemberRole.Leader,
            JoinedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var sut = CreateService(db, out var ns, out var als);
        var created = await sut.CreateJoinRequest(userId, orgId, new CreateJoinRequestDto("b", "s", "e", ""));

        var result = await sut.ReviewJoinRequest(
            new ReviewJoinRequestDto(RequestStatus.Approved, null), reviewer.Id, orgId, created.Id);

        result.Status.Should().Be(RequestStatus.Approved);
        db.OrganizationMemberships.Should().Contain(m => m.UserId == userId && m.OrganizationId == orgId);
        db.Users.Single(u => u.Id == userId).Role.Should().Be(UserRoles.Volunteer);
        ns.Verify(x => x.SendNotification(userId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        als.Verify(x => x.Log(reviewer.Id, "Approved", "JoinRequest", created.Id, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReviewJoinRequest_ByNonManager_ThrowsNotEnoughRights()
    {
        using var db = TestDb.Create();
        var (userId, orgId) = await SeedUserAndOrg(db);

        var outsider = new UserModel { Email = "out@test.com", PasswordHash = "x", FirstName = "O", SecondName = "S" };
        db.Users.Add(outsider);
        await db.SaveChangesAsync();

        var sut = CreateService(db, out _, out _);
        var created = await sut.CreateJoinRequest(userId, orgId, new CreateJoinRequestDto("b", "s", "e", ""));

        var act = async () => await sut.ReviewJoinRequest(
            new ReviewJoinRequestDto(RequestStatus.Approved, null), outsider.Id, orgId, created.Id);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
