
using CraneFileManager.Domain.Entities.Configurations;
using CraneFileManager.Domain.Entities.IdentityAuth;
using CraneFileManager.Domain.Entities.Models;
using CraneFileManager.Domain.EntityFrameworkConfigurations;
using CraneFileManager.Persistence.ServiceExtensions;
using Microsoft.EntityFrameworkCore;
namespace CraneFileManager.Persistence.Contexts.CraneFileManagerDbContext;

public partial class CraneFileManagerContext : DbContext
{
    private readonly AppSettings _appSettings;
    public CraneFileManagerContext()
    {
    }

    public CraneFileManagerContext(DbContextOptions<CraneFileManagerContext> options, AppSettings appSettings)
        : base(options)
    {
        _appSettings = appSettings;
    }

    public virtual DbSet<CraneFileManager.Domain.Entities.Models.File> Files { get; set; }

    public virtual DbSet<CraneFileManager.Domain.Entities.Models.FileShare> FileShares { get; set; }

    public virtual DbSet<FileTrashCan> FileTrashCans { get; set; }

    public virtual DbSet<FileType> FileTypes { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RoleClaim> RoleClaims { get; set; }

    public virtual DbSet<RolePermission> RolePermissions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserClaim> UserClaims { get; set; }

    public virtual DbSet<UserFile> UserFiles { get; set; }

    public virtual DbSet<UserNotification> UserNotifications { get; set; }

    public virtual DbSet<UserPermission> UserPermissions { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(_appSettings.ConnectionStrings.CustomDbConnection);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new FileConfiguration());
        modelBuilder.ApplyConfiguration(new FileShareConfiguration());
        modelBuilder.ApplyConfiguration(new FileTrashCanConfiguration());
        modelBuilder.ApplyConfiguration(new FileTypeConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new RoleClaimConfiguration());
        modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new UserClaimConfiguration());
        modelBuilder.ApplyConfiguration(new UserFileConfiguration());
        modelBuilder.ApplyConfiguration(new UserNotificationConfiguration());
        modelBuilder.ApplyConfiguration(new UserPermissionConfiguration());
        modelBuilder.ApplyConfiguration(new UserRoleConfiguration());

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
