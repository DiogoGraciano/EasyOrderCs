using System.ComponentModel.DataAnnotations;
using EasyOrderCs.Models;

namespace EasyOrderCs.Dtos.Order;

public class CreateOrderDto
{
    [Required(ErrorMessage = "O número do pedido é obrigatório")]
    [StringLength(50, ErrorMessage = "O número do pedido não pode ter mais de 50 caracteres")]
    public string OrderNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "A data do pedido é obrigatória")]
    public DateTime OrderDate { get; set; }

    public OrderStatus? Status { get; set; }

    [Required(ErrorMessage = "O ID do cliente é obrigatório")]
    public Guid CustomerId { get; set; }

    [Required(ErrorMessage = "O ID da empresa é obrigatório")]
    public Guid EnterpriseId { get; set; }

    [Required(ErrorMessage = "O valor total é obrigatório")]
    [Range(0, double.MaxValue, ErrorMessage = "O valor total deve ser maior ou igual a zero")]
    public decimal? TotalAmount { get; set; }

    [StringLength(1000, ErrorMessage = "As observações não podem ter mais de 1000 caracteres")]
    public string? Notes { get; set; }

    [Required(ErrorMessage = "Os itens são obrigatórios")]
    [MinLength(1, ErrorMessage = "O pedido deve ter pelo menos 1 item")]
    public List<CreateOrderItemDto> Items { get; set; } = new();
}

