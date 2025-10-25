using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Requests;

public record UpdateAuthorRequestDto
{
    [Required] [MinLength(1)] public string AuthorIdForLookup { get; set; }

    [Required] [MinLength(1)] public string NewName { get; set; }

    [Required]
    /// <summary>
    /// Refers to all book IDs the author has written (idempotent style update)
    /// </summary>
    public List<string> BooksIds { get; set; } = new();
}