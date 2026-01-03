using EasyOrderCs.Dtos.Order;
using EasyOrderCs.Models;

namespace EasyOrderCs.Services.Interfaces;

public interface IOrderService
{
    Task<Order> CreateAsync(CreateOrderDto createOrderDto);
    Task<List<Order>> GetAllAsync();
    Task<List<Order>> GetByCustomerAsync(Guid customerId);
    Task<List<Order>> GetByEnterpriseAsync(Guid enterpriseId);
    Task<Order> GetByIdAsync(Guid id);
    Task<Order> UpdateAsync(Guid id, UpdateOrderDto updateOrderDto);
    Task<Order> UpdateStatusAsync(Guid id, OrderStatus status);
    Task DeleteAsync(Guid id);
}

