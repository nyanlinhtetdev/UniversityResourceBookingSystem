using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ResourceBookingSystem.Database.AppDbContextModels;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Resource> Resources { get; set; }

    public virtual DbSet<ResourceCategory> ResourceCategories { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("bookings_pkey");

            entity.ToTable("bookings");

            entity.Property(e => e.BookingId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("booking_id");
            entity.Property(e => e.CreatedDatetime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_datetime");
            entity.Property(e => e.EndDatetime).HasColumnName("end_datetime");
            entity.Property(e => e.ModifiedDatetime).HasColumnName("modified_datetime");
            entity.Property(e => e.ResourceId).HasColumnName("resource_id");
            entity.Property(e => e.StartDatetime).HasColumnName("start_datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Confirmed'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Resource).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.ResourceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_booking_resource");

            entity.HasOne(d => d.User).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_booking_user");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.RefreshTokenId).HasName("refresh_tokens_pkey");

            entity.ToTable("refresh_tokens");

            entity.HasIndex(e => e.TokenHash, "refresh_tokens_token_hash_key").IsUnique();

            entity.Property(e => e.RefreshTokenId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("refresh_token_id");
            entity.Property(e => e.CreatedDatetime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_datetime");
            entity.Property(e => e.ExpiredDatetime).HasColumnName("expired_datetime");
            entity.Property(e => e.RevokedDatetime).HasColumnName("revoked_datetime");
            entity.Property(e => e.TokenHash).HasColumnName("token_hash");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_refresh_token_user");
        });

        modelBuilder.Entity<Resource>(entity =>
        {
            entity.HasKey(e => e.ResourceId).HasName("resources_pkey");

            entity.ToTable("resources");

            entity.Property(e => e.ResourceId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("resource_id");
            entity.Property(e => e.Capacity).HasColumnName("capacity");
            entity.Property(e => e.Location)
                .HasMaxLength(20)
                .HasColumnName("location");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
            entity.Property(e => e.ResourceCategoryId).HasColumnName("resource_category_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Active'::character varying")
                .HasColumnName("status");

            entity.HasOne(d => d.ResourceCategory).WithMany(p => p.Resources)
                .HasForeignKey(d => d.ResourceCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_resource_category");
        });

        modelBuilder.Entity<ResourceCategory>(entity =>
        {
            entity.HasKey(e => e.ResourceCategoryId).HasName("resource_categories_pkey");

            entity.ToTable("resource_categories");

            entity.HasIndex(e => e.Name, "resource_categories_name_key").IsUnique();

            entity.Property(e => e.ResourceCategoryId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("resource_category_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.HasIndex(e => e.StudentId, "users_student_id_key").IsUnique();

            entity.Property(e => e.UserId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("user_id");
            entity.Property(e => e.ContactNumber)
                .HasMaxLength(30)
                .HasColumnName("contact_number");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(150)
                .HasColumnName("full_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Student'::character varying")
                .HasColumnName("role");
            entity.Property(e => e.StudentId)
                .HasMaxLength(50)
                .HasColumnName("student_id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
