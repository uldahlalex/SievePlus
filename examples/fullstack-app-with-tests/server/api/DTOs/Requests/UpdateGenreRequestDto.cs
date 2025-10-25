using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Requests;

public record UpdateGenreRequestDto
{
    [Required] [MinLength(1)] public string IdToLookupBy { get; set; }

    [Required] [MinLength(1)] public string NewName { get; set; }
}