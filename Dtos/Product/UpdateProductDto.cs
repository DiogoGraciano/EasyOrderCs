using System.ComponentModel.DataAnnotations;

namespace EasyOrderCs.Dtos.Product;

public class UpdateProductDto
{
    [StringLength(255, MinimumLength = 1, ErrorMessage = "O nome do produto deve ter entre 1 e 255 caracteres")]
    public string? Name { get; set; }

    public string? Description { get; set; }

    [Range(0.01, 1000000, ErrorMessage = "O preço deve ser maior que zero e menor que 1.000.000")]
    public decimal? Price { get; set; }

    [Range(0, 999999, ErrorMessage = "O estoque deve ser maior ou igual a zero e menor que 999.999")]
    public int? Stock { get; set; }

    public string? Photo { get; set; }

    public Guid? EnterpriseId { get; set; }
}

