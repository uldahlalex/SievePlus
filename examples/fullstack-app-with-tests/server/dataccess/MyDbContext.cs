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

    public virtual DbSet<Author> Authors { get; set; }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<Genre> Genres { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("author_pkey");

            entity.ToTable("author", "library");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdat).HasColumnName("createdat");
            entity.Property(e => e.Name).HasColumnName("name");

            entity.HasMany(d => d.Books).WithMany(p => p.Authors)
                .UsingEntity<Dictionary<string, object>>(
                    "Authorbookjunction",
                    r => r.HasOne<Book>().WithMany()
                        .HasForeignKey("Bookid")
                        .HasConstraintName("authorbookjunction_bookid_fkey"),
                    l => l.HasOne<Author>().WithMany()
                        .HasForeignKey("Authorid")
                        .HasConstraintName("authorbookjunction_authorid_fkey"),
                    j =>
                    {
                        j.HasKey("Authorid", "Bookid").HasName("authorbookjunction_pkey");
                        j.ToTable("authorbookjunction", "library");
                        j.IndexerProperty<string>("Authorid").HasColumnName("authorid");
                        j.IndexerProperty<string>("Bookid").HasColumnName("bookid");
                    });
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("book_pkey");

            entity.ToTable("book", "library");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdat).HasColumnName("createdat");
            entity.Property(e => e.Genreid).HasColumnName("genreid");
            entity.Property(e => e.Pages).HasColumnName("pages");
            entity.Property(e => e.Title).HasColumnName("title");

            entity.HasOne(d => d.Genre).WithMany(p => p.Books)
                .HasForeignKey(d => d.Genreid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("book_genreid_fkey");
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("genre_pkey");

            entity.ToTable("genre", "library");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdat).HasColumnName("createdat");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
