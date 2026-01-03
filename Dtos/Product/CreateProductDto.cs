using System.ComponentModel.DataAnnotations;

namespace EasyOrderCs.Dtos.Product;

public class CreateProductDto
{
    [Required(ErrorMessage = "O nome do produto é obrigatório")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "O nome do produto deve ter entre 1 e 255 caracteres")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "A descrição do produto é obrigatória")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "O preço é obrigatório")]
    [Range(0.01, 1000000, ErrorMessage = "O preço deve ser maior que zero e menor que 1.000.000")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "O estoque é obrigatório")]
    [Range(0, 999999, ErrorMessage = "O estoque deve ser maior ou igual a zero e menor que 999.999")]
    public int Stock { get; set; }

    public string? Photo { get; set; }

    [Required(ErrorMessage = "O ID da empresa é obrigatório")]
    public Guid EnterpriseId { get; set; }
}

