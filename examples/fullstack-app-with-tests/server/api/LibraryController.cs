using api.DTOs.Requests;
using api.Services;
using dataccess;
using Microsoft.AspNetCore.Mvc;
using Sieve.Plus.Models;

namespace api;

public class LibraryController(ILibraryService libraryService) : ControllerBase
{
 
    [HttpPost(nameof(GetAuthors))]
    public async Task<List<Author>> GetAuthors([FromBody]SievePlusModel sievePlusModel)
    {
        return await libraryService.GetAuthors(sievePlusModel);
    }




    [HttpPost(nameof(GetBooks))]
    public async Task<List<Book>> GetBooks([FromBody]SievePlusModel sievePlusModel)
    {
        return await libraryService.GetBooks(sievePlusModel);
    }

    [HttpPost(nameof(GetGenres))]
    public async Task<List<Genre>> GetGenres([FromBody] SievePlusModel sievePlusModel)
    {
        return await libraryService.GetGenres(sievePlusModel);
    }

    [HttpPost(nameof(CreateBook))]
    public async Task<Book> CreateBook([FromBody] CreateBookRequestDto dto)
    {
        return await libraryService.CreateBook(dto);
    }

    [HttpPut(nameof(UpdateBook))]
    public async Task<Book> UpdateBook([FromBody] UpdateBookRequestDto dto)
    {
        return await libraryService.UpdateBook(dto);
    }

    [HttpDelete(nameof(DeleteBook))]
    public async Task<Book> DeleteBook([FromQuery] string bookId)
    {
        return await libraryService.DeleteBook(bookId);
    }

    [HttpPost(nameof(CreateAuthor))]
    public async Task<Author> CreateAuthor([FromBody] CreateAuthorRequestDto dto)
    {
        return await libraryService.CreateAuthor(dto);
    }

    [HttpPut(nameof(UpdateAuthor))]
    public async Task<Author> UpdateAuthor([FromBody] UpdateAuthorRequestDto dto)
    {
        return await libraryService.UpdateAuthor(dto);
    }

    [HttpDelete(nameof(DeleteAuthor))]
    public async Task<Author> DeleteAuthor([FromQuery] string authorId)
    {
        return await libraryService.DeleteAuthor(authorId);
    }

    [HttpPost(nameof(CreateGenre))]
    public async Task<Genre> CreateGenre([FromBody] CreateGenreDto dto)
    {
        return await libraryService.CreateGenre(dto);
    }

    [HttpDelete(nameof(DeleteGenre))]
    public async Task<Genre> DeleteGenre([FromQuery] string genreId)
    {
        return await libraryService.DeleteGenre(genreId);
    }

    [HttpPut(nameof(UpdateGenre))]
    public async Task<Genre> UpdateGenre([FromBody] UpdateGenreRequestDto dto)
    {
        return await libraryService.UpdateGenre(dto);
    }
    
    

}