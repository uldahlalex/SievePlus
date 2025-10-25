using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Requests;

public record CreateAuthorRequestDto
{
    [MinLength(1)] [Required] public string Name { get; set; } = null!;
}