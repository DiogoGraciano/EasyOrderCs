using System.ComponentModel.DataAnnotations;
using EasyOrderCs.Models;

namespace EasyOrderCs.Dtos.Order;

public class UpdateOrderDto
{
    [StringLength(50, ErrorMessage = "O número do pedido não pode ter mais de 50 caracteres")]
    public string? OrderNumber { get; set; }

    public DateTime? OrderDate { get; set; }

    public OrderStatus? Status { get; set; }

    public Guid? CustomerId { get; set; }

    public Guid? EnterpriseId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "O valor total deve ser maior ou igual a zero")]
    public decimal? TotalAmount { get; set; }

    [StringLength(1000, ErrorMessage = "As observações não podem ter mais de 1000 caracteres")]
    public string? Notes { get; set; }

    public List<CreateOrderItemDto>? Items { get; set; }
}

