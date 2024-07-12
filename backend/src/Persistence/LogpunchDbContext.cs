using Domain;
using Microsoft.EntityFrameworkCore;

namespace Persistence;

public class LogpunchDbContext(DbContextOptions<LogpunchDbContext> options) : DbContext(options)
{
    public DbSet<LogpunchClient> Clients => Set<LogpunchClient>();
    public DbSet<LogpunchRegistration> Registrations => Set<LogpunchRegistration>();
    public DbSet<LogpunchUser> Users => Set<LogpunchUser>();

    public DbSet<EmployeeClientRelation> EmployeeClientRelations => Set<EmployeeClientRelation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LogpunchClient>(entity =>
        {
            entity.ToTable("logpunch_clients");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name")
                .IsRequired();
        });

        modelBuilder.Entity<EmployeeClientRelation>(entity =>
        {
            entity.ToTable("logpunch_employee_client_relations");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EmployeeId).HasColumnName("employeeid");
            entity.Property(e => e.ClientId).HasColumnName("clientid");

            entity.HasOne(cc => cc.Employee)
                .WithMany(c => c.EmployeeClientRelations)
                .HasForeignKey(cc => cc.EmployeeId);

            entity.HasOne(cc => cc.Client)
                .WithMany(c => c.EmployeeClientRelations)
                .HasForeignKey(cc => cc.ClientId);
        });

        modelBuilder.Entity<LogpunchRegistration>(entity =>
        {
            entity.ToTable("logpunch_registrations");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EmployeeId).HasColumnName("employeeid")
                .IsRequired();
            entity.Property(e => e.Type).HasColumnName("registration_type")
                .IsRequired();
            entity.Property(e => e.Amount).HasColumnName("amount")
                .IsRequired(false);
            entity.Property(e => e.Start).HasColumnName("registration_start")
                .IsRequired();
            entity.Property(e => e.End).HasColumnName("registration_end")
                .IsRequired(false);
            entity.Property(e => e.CreatorId).HasColumnName("creatorid")
                .IsRequired();
            entity.Property(e => e.ClientId).HasColumnName("clientid")
                .IsRequired(false);
            entity.Property(e => e.CreationTime).HasColumnName("creation_time")
                .IsRequired();
            entity.Property(e => e.Status).HasColumnName("status_type")
                .IsRequired();
            entity.Property(e => e.FirstComment).HasColumnName("first_comment")
                .IsRequired(false);
            entity.Property(e => e.SecondComment).HasColumnName("second_comment")
                .IsRequired(false);
            entity.Property(e => e.CorrectionOfId).HasColumnName("correctionof_id")
                .IsRequired(false);
        });

        modelBuilder.Entity<LogpunchUser>(entity =>
        {
            entity.ToTable("logpunch_users");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).HasColumnName("first_name")
                .HasMaxLength(50);
            entity.Property(e => e.LastName).HasColumnName("last_name")
                .HasMaxLength(50);
            entity.Property(e => e.Email).HasColumnName("email")
                .HasMaxLength(255)
                .IsRequired();
            entity.Property(e => e.Password).HasColumnName("password")
                .HasMaxLength(255);
            entity.Property(e => e.DefaultQuery).HasColumnName("default_query")
                .HasMaxLength(255)
                .IsRequired(false);
            entity.Property(e => e.Role).HasColumnName("role")
                .IsRequired();
        });
    }
}
