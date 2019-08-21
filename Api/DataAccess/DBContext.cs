using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DataAccess
{
    public partial class DBContext : DbContext
    {
        public DBContext()
        {
        }

        public DBContext(DbContextOptions<DBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Tenant> Tenant { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<UserEvent> UserEvent { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseNpgsql("Host=ec2-23-21-156-171.compute-1.amazonaws.com;Port=5432;Database=dd4vp538g93rcc;Username=yxqdckafksapfi;Password=83a6e32d3d8adc5f4584f92ae73525d5572c862ff42d02d884581ec7ed7c05d7;SSL Mode=Require;Trust Server Certificate=true");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("uuid-ossp")
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.ToTable("tenant");

                entity.HasIndex(e => e.ClientId)
                    .HasName("tenant_clientid_uindex")
                    .IsUnique();

                entity.HasIndex(e => e.Id)
                    .HasName("tenant_id_uindex")
                    .IsUnique();

                entity.HasIndex(e => e.ManagementToken)
                    .HasName("tenant_management_token_uindex")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.AdminId).HasColumnName("admin_id");

                entity.Property(e => e.ClientId)
                    .IsRequired()
                    .HasColumnName("client_id")
                    .HasMaxLength(1024);

                entity.Property(e => e.JwtDuration).HasColumnName("jwt_duration");

                entity.Property(e => e.JwtSigningKey)
                    .IsRequired()
                    .HasColumnName("jwt_signing_key")
                    .HasMaxLength(2048)
                    .HasDefaultValueSql("''::character varying");

                entity.Property(e => e.ManagementToken)
                    .IsRequired()
                    .HasColumnName("management_token")
                    .HasMaxLength(1024);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("user");

                entity.HasIndex(e => e.Email)
                    .HasName("user_email_index");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasColumnName("email")
                    .HasMaxLength(100);

                entity.Property(e => e.FullName)
                    .IsRequired()
                    .HasColumnName("full_name")
                    .HasMaxLength(100);

                entity.Property(e => e.Metadata)
                    .HasColumnName("metadata")
                    .HasColumnType("jsonb");

                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasColumnName("password_hash")
                    .HasMaxLength(1024);

                entity.Property(e => e.PasswordSalt)
                    .IsRequired()
                    .HasColumnName("password_salt")
                    .HasMaxLength(1024);

                entity.Property(e => e.TenantId).HasColumnName("tenant_id");

                entity.HasOne(d => d.Tenant)
                    .WithMany(p => p.User)
                    .HasForeignKey(d => d.TenantId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("user_tenant_id_fk");
            });

            modelBuilder.Entity<UserEvent>(entity =>
            {
                entity.ToTable("user_event");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.Event)
                    .IsRequired()
                    .HasColumnName("event")
                    .HasMaxLength(1024);

                entity.Property(e => e.Info)
                    .HasColumnName("info")
                    .HasColumnType("jsonb");

                entity.Property(e => e.Tenantid).HasColumnName("tenantid");

                entity.Property(e => e.Userid).HasColumnName("userid");

                entity.Property(e => e.When).HasColumnName("when");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserEvent)
                    .HasForeignKey(d => d.Userid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("user_event_user_id_fk");
            });
        }
    }
}
