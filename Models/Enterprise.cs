using System.ComponentModel.DataAnnotations;

namespace EasyOrderCs.Models;

public class Enterprise
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string LegalName { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string TradeName { get; set; } = string.Empty;

    public string? Logo { get; set; }

    [Required]
    public DateTime FoundationDate { get; set; }

    [Required]
    [MaxLength(14)]
    public string Cnpj { get; set; } = string.Empty;

    [Required]
    public string Address { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

