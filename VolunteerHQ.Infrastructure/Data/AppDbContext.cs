using Microsoft.EntityFrameworkCore;
using VolunteerHQ.Core.Models;


namespace VolunteerHQ.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    #region DbSets
    public DbSet<UserModel> Users { get; set; }
    public DbSet<VolunteerProfileModel> VolunteerProfiles { get; set; }
    public DbSet<OrganizationModel> Organizations { get; set; }
    public DbSet<OrganizationMembershipModel> OrganizationMemberships { get; set; }
    public DbSet<JoinRequestModel> JoinRequests { get; set; }
    public DbSet<OrganizationRequestModel> OrganizationRequests { get; set; }
    public DbSet<MilitaryUnitModel> MilitaryUnits { get; set; }
    public DbSet<FundraiserModel> Fundraisers { get; set; }
    public DbSet<FundraiserAssignmentModel> FundraiserAssignments { get; set; }
    public DbSet<DonationModel> Donations { get; set; }
    public DbSet<CommentModel> Comments { get; set; }
    public DbSet<PrivateMessageModel> PrivateMessages { get; set; }
    public DbSet<SubscriptionModel> Subscriptions { get; set; }
    public DbSet<NotificationModel> Notifications { get; set; }
    public DbSet<ReportModel> Reports { get; set; }
    public DbSet<AuditLogModel> AuditLogs { get; set; }
    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserModel>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<MilitaryUnitModel>()
            .HasIndex(m => m.Login)
            .IsUnique();

        modelBuilder.Entity<OrganizationMembershipModel>()
            .HasIndex(m => new { m.UserId, m.OrganizationId })
            .IsUnique();

        #region FundraiserAssignmentModel
        modelBuilder.Entity<FundraiserAssignmentModel>()
            .HasIndex(f => f.UniqueCode)
            .IsUnique();

        modelBuilder.Entity<FundraiserAssignmentModel>()
            .Property(f => f.AmountRaised)
            .HasPrecision(18, 2);
        #endregion
        
        #region FundraiserModel
        modelBuilder.Entity<FundraiserModel>()
            .Property(f => f.TotalGoal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<FundraiserModel>()
            .Property(f => f.CurrentProgress)
            .HasPrecision(18, 2);
        #endregion

        modelBuilder.Entity<DonationModel>()
            .Property(d => d.Amount)
            .HasPrecision(18, 2);
        
        #region PrivateMessage
        modelBuilder.Entity<PrivateMessageModel>()
            .HasOne(m => m.UserSender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<PrivateMessageModel>()
            .HasOne(m => m.UserReceiver)
            .WithMany()
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.SetNull);
        #endregion
        
        #region ReportModel
        
        modelBuilder.Entity<ReportModel>()
            .HasOne(r => r.ReporterUser)
            .WithMany()
            .HasForeignKey(r => r.ReporterId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ReportModel>()
            .HasOne(r => r.ReportedUser)
            .WithMany()
            .HasForeignKey(r => r.ReportedId)
            .OnDelete(DeleteBehavior.SetNull);
        
        #endregion
        
        #region JoinRequestModel

        modelBuilder.Entity<JoinRequestModel>()
            .HasOne(j => j.User)
            .WithMany()
            .HasForeignKey(j => j.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<JoinRequestModel>()
            .HasOne(j => j.ReviewedByUser)
            .WithMany()
            .HasForeignKey(j => j.ReviewedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
        
        #endregion

        modelBuilder.Entity<VolunteerProfileModel>()
            .HasOne(v => v.User)
            .WithOne(u => u.VolunteerProfile)
            .HasForeignKey<VolunteerProfileModel>(v => v.UserId);


    }
}