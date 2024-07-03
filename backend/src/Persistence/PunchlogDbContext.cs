using Domain;
using Microsoft.EntityFrameworkCore;

namespace Persistence;

public class PunchlogDbContext(DbContextOptions<PunchlogDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<TimeRegistration> TimeRegistrations => Set<TimeRegistration>();
    public DbSet<Consultant> Consultants => Set<Consultant>();

    public DbSet<ConsultantCustomer> ConsultantCustomers => Set<ConsultantCustomer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("customer");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name")
                .IsRequired();
        });

        modelBuilder.Entity<Consultant>(entity =>
        {
            entity.ToTable("consultant");

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
        });

        modelBuilder.Entity<TimeRegistration>(entity =>
            {
                entity.ToTable("time_registration");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RegistrationDate).HasColumnType("registration_date");
                entity.Property(e => e.Hours).HasColumnName("hours");
                entity.Property(e => e.ConsultantCustomerId).HasColumnName("consultant_customerid");
            }
        );

        modelBuilder.Entity<ConsultantCustomer>(entity =>
        {
            entity.ToTable("consultant_customer");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ConsultantId).HasColumnName("consultantid");
            entity.Property(e => e.CustomerId).HasColumnName("customerid");
            entity.Property(e => e.Favorite).HasColumnName("favorite");

            entity.HasOne(cc => cc.Consultant)
                .WithMany(c => c.ConsultantCustomers)
                .HasForeignKey(cc => cc.ConsultantId);

            entity.HasOne(cc => cc.Customer)
                .WithMany(c => c.ConsultantCustomers)
                .HasForeignKey(cc => cc.CustomerId);
        });
    }
}
