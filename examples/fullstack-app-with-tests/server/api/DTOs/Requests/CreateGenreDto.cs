using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Requests;

public record CreateGenreDto
{
    [Required] [MinLength(1)] public string Name { get; set; }
}