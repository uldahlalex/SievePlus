using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace dataccess;

public partial class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Computer> Computers { get; set; }
    public virtual DbSet<Brand> Brands { get; set; }
    public virtual DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Computer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("computer_pkey");
            entity.ToTable("computer", "pricerunner");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Processor).HasColumnName("processor");
            entity.Property(e => e.Price).HasColumnName("price").HasPrecision(10, 2);
            entity.Property(e => e.ScreenSize).HasColumnName("screensize").HasPrecision(4, 2);
            entity.Property(e => e.Ram).HasColumnName("ram");
            entity.Property(e => e.Storage).HasColumnName("storage");
            entity.Property(e => e.GraphicsCard).HasColumnName("graphicscard");
            entity.Property(e => e.InStock).HasColumnName("instock");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.Sales).HasColumnName("sales");
            entity.Property(e => e.CreatedAt).HasColumnName("createdat");
            entity.Property(e => e.BrandId).HasColumnName("brandid");
            entity.Property(e => e.CategoryId).HasColumnName("categoryid");

            entity.HasOne(d => d.Brand).WithMany(p => p.Computers)
                .HasForeignKey(d => d.BrandId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("computer_brandid_fkey");

            entity.HasOne(d => d.Category).WithMany(p => p.Computers)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("computer_categoryid_fkey");
        });

        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("brand_pkey");
            entity.ToTable("brand", "pricerunner");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.CreatedAt).HasColumnName("createdat");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("category_pkey");
            entity.ToTable("category", "pricerunner");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.CreatedAt).HasColumnName("createdat");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
