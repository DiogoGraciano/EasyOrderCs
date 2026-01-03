using System.ComponentModel.DataAnnotations;

namespace EasyOrderCs.Dtos.Order;

public class CreateOrderItemDto
{
    [Required(ErrorMessage = "O ID do produto é obrigatório")]
    public Guid ProductId { get; set; }

    [Required(ErrorMessage = "O nome do produto é obrigatório")]
    [StringLength(255, ErrorMessage = "O nome do produto não pode ter mais de 255 caracteres")]
    public string ProductName { get; set; } = string.Empty;

    [Required(ErrorMessage = "A quantidade é obrigatória")]
    [Range(1, 100, ErrorMessage = "A quantidade deve ser pelo menos 1 e no máximo 100")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "O preço unitário é obrigatório")]
    [Range(0, double.MaxValue, ErrorMessage = "O preço unitário deve ser maior ou igual a zero")]
    public decimal UnitPrice { get; set; }

    [Required(ErrorMessage = "O subtotal é obrigatório")]
    [Range(0, double.MaxValue, ErrorMessage = "O subtotal deve ser maior ou igual a zero")]
    public decimal Subtotal { get; set; }
}

