using System.ComponentModel.DataAnnotations;

namespace EasyOrderCs.Models;

public class User
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Password { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    [MaxLength(50)]
    public string Role { get; set; } = "user";

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

