using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyOrderCs.Models;

public class Order
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string OrderNumber { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "date")]
    public DateTime OrderDate { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [Required]
    public Guid CustomerId { get; set; }

    [Required]
    public Guid EnterpriseId { get; set; }

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalAmount { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey("CustomerId")]
    public Customer Customer { get; set; } = null!;

    [ForeignKey("EnterpriseId")]
    public Enterprise Enterprise { get; set; } = null!;

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}

