using api.DTOs;
using api.DTOs.Requests;
using dataccess;
using Sieve.Plus.Models;

namespace api.Services;

public interface ILibraryService
{
    
    Task<Book> CreateBook(CreateBookRequestDto dto);
    Task<Book> UpdateBook(UpdateBookRequestDto dto);
    Task<Book> DeleteBook(string id);
    Task<Author> CreateAuthor(CreateAuthorRequestDto dto);
    Task<Author> UpdateAuthor(UpdateAuthorRequestDto dto);
    Task<Author> DeleteAuthor(string authorId);
    Task<Genre> CreateGenre(CreateGenreDto dto);
    Task<Genre> DeleteGenre(string genreId);
    Task<Genre> UpdateGenre(UpdateGenreRequestDto dto);

    Task<List<Author>> GetAuthors(SievePlusModel sievePlusModel);
    Task<List<Book>> GetBooks(SievePlusModel sievePlusModel);
    Task<List<Genre>> GetGenres(SievePlusModel sievePlusModel);
}