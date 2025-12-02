using GymAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GymAPI.Data;

public partial class GymDbContext : DbContext
{
    public GymDbContext()
    {
    }

    public GymDbContext(DbContextOptions<GymDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Chip> Chips { get; set; }

    public virtual DbSet<Member> Members { get; set; }

    public virtual DbSet<Membership> Memberships { get; set; }

    public virtual DbSet<Visit> Visits { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Chip>(entity =>
        {
            entity.HasKey(e => e.ChipId).HasName("PK__Chip__687B2D0AE2E25384");

            entity.ToTable("Chip");

            entity.HasIndex(e => e.ChipNumber, "UQ__Chip__7460557DA9916DDD").IsUnique();

            entity.Property(e => e.ChipId).HasColumnName("ChipID");
            entity.Property(e => e.ChipNumber).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MemberId).HasColumnName("MemberID");
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(e => e.MemberId).HasName("PK__Member__0CF04B38CA5825FA");

            entity.ToTable("Member");

            entity.HasIndex(e => e.PhoneNumber, "UQ__Member__85FB4E381D5FF421").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Member__A9D1053443EF70CA").IsUnique();

            entity.Property(e => e.MemberId).HasColumnName("MemberID");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.RegistrationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<Membership>(entity =>
        {
            entity.HasKey(e => e.MembershipId).HasName("PK__Membersh__92A78599D9055976");

            entity.ToTable("Membership");

            entity.Property(e => e.MembershipId).HasColumnName("MembershipID");
            entity.Property(e => e.MemberId).HasColumnName("MemberID");
        });

        modelBuilder.Entity<Visit>(entity =>
        {
            entity.HasKey(e => e.VisitId).HasName("PK__Visit__4D3AA1BEFA5F8581");

            entity.ToTable("Visit");

            entity.Property(e => e.VisitId).HasColumnName("VisitID");
            entity.Property(e => e.ChipId).HasColumnName("ChipID");
            entity.Property(e => e.VisitDateTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
