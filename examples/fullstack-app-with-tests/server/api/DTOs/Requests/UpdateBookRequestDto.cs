using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Requests;

public record UpdateBookRequestDto
{
    /// <summary>
    ///     BookId is used for lookup
    /// </summary>
    [Required]
    [MinLength(1)]
    public string BookIdForLookupReference { get; set; }

    [Required] [Range(1, int.MaxValue)] public int NewPageCount { get; set; }
    [Required] [MinLength(1)] public string NewTitle { get; set; }
    [Required] public List<string> AuthorsIds { get; set; }
    public string? GenreId { get; set; }
}