using System.ComponentModel.DataAnnotations;
using api.DTOs.Requests;
using dataccess;
using Microsoft.EntityFrameworkCore;
using Sieve.Plus.Models;
using Sieve.Plus.Services;

namespace api.Services;

public class LibraryService(MyDbContext ctx, ISievePlusProcessor sievePlusProcessor) : ILibraryService
{
    

    public Task<List<Book>> GetBooks(SievePlusModel sievePlusModel)
    {
        IQueryable<Book> query =  ctx.Books;

        // Apply Sieve.Plus FIRST (filtering, sorting, pagination)
        query = sievePlusProcessor.Apply(sievePlusModel, query);

        // Then include related data - but DON'T nest further to avoid cycles
        return query
            .Include(b => b.Genre)  // Genre won't include its Books
            .Include(b => b.Authors) // Authors won't include their Books
            .AsSplitQuery()
            .ToListAsync();
    }

    public Task<List<Genre>> GetGenres(SievePlusModel sievePlusModel)
    {
        IQueryable<Genre> query =  ctx.Genres;

        // Apply Sieve.Plus FIRST (filtering, sorting, pagination)
        query = sievePlusProcessor.Apply(sievePlusModel, query);

        // Include Books but DON'T include nested Authors to avoid cycles
        return query
            .Include(g => g.Books) 
            .AsSplitQuery()
            .ToListAsync();
    }
    
    public async Task<List<Author>> GetAuthors(SievePlusModel sievePlusModel)
    {
        IQueryable<Author> query = ctx.Authors;

            // Normal path: Apply Sieve.Plus FIRST (filtering, sorting, pagination)
            query = sievePlusProcessor.Apply(sievePlusModel, query);

            // Include Books but DON'T include nested Genre/Authors to avoid cycles
            return await query
                .Include(a => a.Books)
                .AsSplitQuery()
                .ToListAsync();
        
    }

    public async Task<Book> CreateBook(CreateBookRequestDto dto)
    {
        Validator.ValidateObject(dto, new ValidationContext(dto), true);

        var book = new Book
        {
            Pages = dto.Pages,
            Createdat = DateTime.UtcNow,
            Id = Guid.NewGuid().ToString(),
            Title = dto.Title
        };
        ctx.Books.Add(book);
        await ctx.SaveChangesAsync();
        return book;
    }

    public async Task<Book> UpdateBook(UpdateBookRequestDto dto)
    {
        Validator.ValidateObject(dto, new ValidationContext(dto), true);
        var book = ctx.Books.First(b => b.Id == dto.BookIdForLookupReference);
        await ctx.Entry(book).Collection(b => b.Authors).LoadAsync();

        book.Pages = dto.NewPageCount;
        book.Title = dto.NewTitle;
        book.Genre = dto.GenreId != null ? ctx.Genres.First(g => g.Id == dto.GenreId) : null;

        book.Authors.Clear();
        dto.AuthorsIds.ForEach(id => book.Authors.Add(ctx.Authors.First(a => a.Id == id)));

        await ctx.SaveChangesAsync();
        return book;
    }

    public async Task<Book> DeleteBook(string bookId)
    {
        var book = ctx.Books.First(b => b.Id == bookId);
        ctx.Books.Remove(book);
        await ctx.SaveChangesAsync();
        return book;
    }

    public async Task<Author> CreateAuthor(CreateAuthorRequestDto dto)
    {
        Validator.ValidateObject(dto, new ValidationContext(dto), true);

        var author = new Author
        {
            Id = Guid.NewGuid().ToString(),
            Createdat = DateTime.UtcNow,
            Name = dto.Name
        };
        ctx.Authors.Add(author);
        await ctx.SaveChangesAsync();
        return author;
    }

    public async Task<Author> UpdateAuthor(UpdateAuthorRequestDto dto)
    {
        Validator.ValidateObject(dto, new ValidationContext(dto), true);
        var author = ctx.Authors.First(a => a.Id == dto.AuthorIdForLookup);
        await ctx.Entry(author).Collection(e => e.Books).LoadAsync();
        author.Books.Clear();
        dto.BooksIds.ForEach(id => author.Books.Add(ctx.Books.First(b => b.Id == id)));
        author.Name = dto.NewName;
        await ctx.SaveChangesAsync();
        return author;
    }

    public async Task<Author> DeleteAuthor(string authorId)
    {
        var author = ctx.Authors.First(a => a.Id == authorId);
        ctx.Authors.Remove(author);
        await ctx.SaveChangesAsync();
        return author;
    }

    public async Task<Genre> CreateGenre(CreateGenreDto dto)
    {
        Validator.ValidateObject(dto, new ValidationContext(dto), true);

        var genre = new Genre
        {
            Id = Guid.NewGuid().ToString(),
            Createdat = DateTime.UtcNow,
            Name = dto.Name
        };
        ctx.Genres.Add(genre);
        await ctx.SaveChangesAsync();
        return genre;
    }

    public async Task<Genre> DeleteGenre(string genreId)
    {
        var genre = ctx.Genres.First(a => a.Id == genreId);
        ctx.Genres.Remove(genre);
        await ctx.SaveChangesAsync();
        return genre;
    }

    public async Task<Genre> UpdateGenre(UpdateGenreRequestDto dto)
    {
        Validator.ValidateObject(dto, new ValidationContext(dto), true);

        var genre = ctx.Genres.First(g => g.Id == dto.IdToLookupBy);
        genre.Name = dto.NewName;
        await ctx.SaveChangesAsync();
        return genre;
    }
    

 

    
}