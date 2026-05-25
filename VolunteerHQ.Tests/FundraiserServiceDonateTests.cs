using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VolunteerHQ.Core.DTOs.DonationDTOs;
using VolunteerHQ.Core.Enums;
using VolunteerHQ.Core.Exceptions;
using VolunteerHQ.Core.Models;
using VolunteerHQ.Infrastructure.Data;
using VolunteerHQ.Infrastructure.Services.Classes;
using VolunteerHQ.Infrastructure.Services.Interfaces;

namespace VolunteerHQ.Tests;

public class FundraiserServiceDonateTests
{
    private static FundraiserService CreateService(AppDbContext db)
    {
        var ns = new Mock<INotificationService>();
        var logger = new Mock<ILogger<FundraiserService>>();
        return new FundraiserService(db, new ValidatorService(db), ns.Object, logger.Object);
    }

    private static async Task<FundraiserModel> SeedFundraiser(AppDbContext db, decimal goal, decimal alreadyRaised = 0m)
    {
        var fundraiser = new FundraiserModel
        {
            MilitaryUnitId = 1,
            Title = "T",
            Description = "D",
            TotalGoal = goal,
            Status = FundraiserStatus.Open,
            Importance = FundraiserImportance.Medium,
            Deadline = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            CreatedAt = DateTime.UtcNow
        };
        db.Fundraisers.Add(fundraiser);
        await db.SaveChangesAsync();

        if (alreadyRaised > 0)
        {
            db.Donations.Add(new DonationModel
            {
                FundraiserId = fundraiser.Id,
                Amount = alreadyRaised,
                IsAnonymous = true,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        return fundraiser;
    }

    [Fact]
    public async Task DirectDonate_WithinGoal_RecordsFullAmount()
    {
        using var db = TestDb.Create();
        var f = await SeedFundraiser(db, goal: 1000m);
        var sut = CreateService(db);

        var result = await sut.DirectDonate(userId: null, f.Id, new CreateDonationDto(300m, null));

        result.Amount.Should().Be(300m);
        db.Donations.Where(d => d.FundraiserId == f.Id).Sum(d => d.Amount).Should().Be(300m);
    }

    [Fact]
    public async Task DirectDonate_ExceedingRemaining_ClampsToRemaining()
    {
        using var db = TestDb.Create();
        var f = await SeedFundraiser(db, goal: 100m, alreadyRaised: 80m);
        var sut = CreateService(db);
        
        var result = await sut.DirectDonate(userId: null, f.Id, new CreateDonationDto(100m, null));

        result.Amount.Should().Be(20m);
        db.Donations.Where(d => d.FundraiserId == f.Id).Sum(d => d.Amount).Should().Be(100m);
    }

    [Fact]
    public async Task DirectDonate_WhenGoalAlreadyMet_Throws()
    {
        using var db = TestDb.Create();
        var f = await SeedFundraiser(db, goal: 100m, alreadyRaised: 100m);
        var sut = CreateService(db);

        var act = async () => await sut.DirectDonate(userId: null, f.Id, new CreateDonationDto(50m, null));

        await act.Should().ThrowAsync<ConflictException>();
        db.Donations.Where(d => d.FundraiserId == f.Id).Sum(d => d.Amount).Should().Be(100m);
    }

    [Fact]
    public async Task DirectDonate_TwoConcurrentDonations_DoNotOvershootGoal()
    {
        using var db = TestDb.Create();
        var f = await SeedFundraiser(db, goal: 100m);
        var sut = CreateService(db);
        
        await sut.DirectDonate(null, f.Id, new CreateDonationDto(100m, null));
        
        var act = async () => await sut.DirectDonate(null, f.Id, new CreateDonationDto(100m, null));
        await act.Should().ThrowAsync<ConflictException>();

        db.Donations.Where(d => d.FundraiserId == f.Id).Sum(d => d.Amount).Should().Be(100m);
    }

    [Fact]
    public async Task Donate_ViaValidCode_ClampsAndLinksAssignment()
    {
        using var db = TestDb.Create();
        var f = await SeedFundraiser(db, goal: 100m, alreadyRaised: 90m);
        var assignment = new FundraiserAssignmentModel
        {
            FundraiserId = f.Id,
            OrganizationId = 1,
            UniqueCode = "code-123",
            TakenAt = DateTime.UtcNow
        };
        db.FundraiserAssignments.Add(assignment);
        await db.SaveChangesAsync();

        var sut = CreateService(db);

        var result = await sut.Donate(userId: null, f.Id, "code-123", new CreateDonationDto(50m, null));

        result.Amount.Should().Be(10m);
        db.Donations.Single(d => d.FundraiserAssignmentId == assignment.Id).Amount.Should().Be(10m);
    }

    [Fact]
    public async Task Donate_InvalidCode_ThrowsNotFound()
    {
        using var db = TestDb.Create();
        var f = await SeedFundraiser(db, goal: 100m);
        var sut = CreateService(db);

        var act = async () => await sut.Donate(null, f.Id, "missing-code", new CreateDonationDto(10m, null));

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
