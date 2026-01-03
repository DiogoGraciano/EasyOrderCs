using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using EasyOrderCs.Data;
using EasyOrderCs.Models;
using EasyOrderCs.Dtos.Order;
using EasyOrderCs.Services.Interfaces;

namespace EasyOrderCs.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly IProductService _productService;

    public OrderService(ApplicationDbContext context, IProductService productService)
    {
        _context = context;
        _productService = productService;
    }

    public async Task<Order> CreateAsync(CreateOrderDto createOrderDto)
    {
        // Calcular total se não fornecido
        if (!createOrderDto.TotalAmount.HasValue)
        {
            createOrderDto.TotalAmount = createOrderDto.Items.Sum(item => item.Subtotal);
        }

        await ValidateOrderCreationAsync(createOrderDto);
        await ValidateUniqueOrderNumberAsync(createOrderDto.OrderNumber);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = createOrderDto.OrderNumber.Trim(),
            OrderDate = createOrderDto.OrderDate.Date,
            Status = createOrderDto.Status ?? OrderStatus.Pending,
            CustomerId = createOrderDto.CustomerId,
            EnterpriseId = createOrderDto.EnterpriseId,
            TotalAmount = createOrderDto.TotalAmount.Value,
            Notes = createOrderDto.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Criar itens do pedido
        var orderItems = createOrderDto.Items.Select(item => new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            ProductId = item.ProductId,
            ProductName = item.ProductName.Trim(),
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            Subtotal = item.Subtotal,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }).ToList();

        _context.OrderItems.AddRange(orderItems);
        await _context.SaveChangesAsync();

        // Reservar estoque
        foreach (var item in createOrderDto.Items)
        {
            await _productService.ReserveStockAsync(item.ProductId, item.Quantity);
        }

        return await GetByIdAsync(order.Id);
    }

    private async Task ValidateOrderCreationAsync(CreateOrderDto createOrderDto)
    {
        await ValidateCustomerExistsAsync(createOrderDto.CustomerId);
        await ValidateEnterpriseExistsAsync(createOrderDto.EnterpriseId);
        ValidateOrderItems(createOrderDto.Items);
        await ValidateProductsAndStockAsync(createOrderDto.Items);
        await ValidateProductsBelongToEnterpriseAsync(createOrderDto.Items, createOrderDto.EnterpriseId);
        ValidateOrderCalculations(createOrderDto);
        ValidateBusinessRules(createOrderDto);
    }

    private void ValidateOrderItems(List<CreateOrderItemDto> items)
    {
        if (items == null || items.Count == 0)
        {
            throw new ArgumentException("O pedido deve ter pelo menos um item");
        }

        ValidateNoDuplicateProducts(items);
        ValidateTotalQuantity(items);

        for (int i = 0; i < items.Count; i++)
        {
            ValidateOrderItem(items[i], i);
        }
    }

    private void ValidateNoDuplicateProducts(List<CreateOrderItemDto> items)
    {
        var productIds = items.Select(item => item.ProductId).ToList();
        var uniqueProductIds = productIds.Distinct().ToList();

        if (productIds.Count != uniqueProductIds.Count)
        {
            throw new ArgumentException("O pedido não pode conter produtos duplicados");
        }
    }

    private void ValidateTotalQuantity(List<CreateOrderItemDto> items, int maxQuantity = 50)
    {
        var totalQuantity = items.Sum(item => item.Quantity);

        if (totalQuantity > maxQuantity)
        {
            throw new ArgumentException(
                $"A quantidade total de itens não pode exceder {maxQuantity}. Total atual: {totalQuantity}");
        }
    }

    private void ValidateOrderItem(CreateOrderItemDto item, int index)
    {
        var itemPrefix = $"Item {index + 1}";

        if (item.Quantity <= 0)
        {
            throw new ArgumentException($"{itemPrefix}: A quantidade deve ser maior que zero");
        }

        if (item.Quantity > 100)
        {
            throw new ArgumentException($"{itemPrefix}: A quantidade não pode exceder 100 unidades");
        }

        if (item.UnitPrice < 0)
        {
            throw new ArgumentException($"{itemPrefix}: O preço unitário não pode ser negativo");
        }

        if (item.Subtotal < 0)
        {
            throw new ArgumentException($"{itemPrefix}: O subtotal não pode ser negativo");
        }

        var expectedSubtotal = item.Quantity * item.UnitPrice;
        if (Math.Abs(item.Subtotal - expectedSubtotal) > 0.01m)
        {
            throw new ArgumentException(
                $"{itemPrefix}: Subtotal incorreto. Esperado: R$ {expectedSubtotal:F2}, Informado: R$ {item.Subtotal:F2}");
        }

        if (string.IsNullOrWhiteSpace(item.ProductName))
        {
            throw new ArgumentException($"{itemPrefix}: O nome do produto é obrigatório");
        }

        if (item.ProductName.Length > 255)
        {
            throw new ArgumentException($"{itemPrefix}: O nome do produto não pode ter mais de 255 caracteres");
        }
    }

    private void ValidateBusinessRules(CreateOrderDto createOrderDto)
    {
        var totalAmount = createOrderDto.TotalAmount ?? createOrderDto.Items.Sum(item => item.Subtotal);
        ValidateMinOrderValue(totalAmount);
        ValidateOrderDate(createOrderDto.OrderDate);
        ValidateOrderNumber(createOrderDto.OrderNumber);
    }

    private void ValidateMinOrderValue(decimal totalAmount, decimal minValue = 5)
    {
        if (totalAmount < minValue)
        {
            throw new ArgumentException(
                $"O valor mínimo do pedido é R$ {minValue:F2}. Valor atual: R$ {totalAmount:F2}");
        }
    }

    private void ValidateOrderDate(DateTime orderDate)
    {
        var today = DateTime.UtcNow.Date;
        if (orderDate.Date > today)
        {
            throw new ArgumentException("A data do pedido não pode ser no futuro");
        }
    }

    private void ValidateOrderNumber(string orderNumber)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
        {
            throw new ArgumentException("O número do pedido é obrigatório");
        }

        if (orderNumber.Length > 50)
        {
            throw new ArgumentException("O número do pedido não pode ter mais de 50 caracteres");
        }

        var validFormat = new Regex(@"^[A-Za-z0-9-]+$");
        if (!validFormat.IsMatch(orderNumber))
        {
            throw new ArgumentException("O número do pedido deve conter apenas letras, números e hífens");
        }
    }

    private async Task ValidateCustomerExistsAsync(Guid customerId)
    {
        var customer = await _context.Customers.FindAsync(customerId);
        if (customer == null)
        {
            throw new KeyNotFoundException($"Cliente com ID {customerId} não encontrado");
        }
    }

    private async Task ValidateEnterpriseExistsAsync(Guid enterpriseId)
    {
        var enterprise = await _context.Enterprises.FindAsync(enterpriseId);
        if (enterprise == null)
        {
            throw new KeyNotFoundException($"Empresa com ID {enterpriseId} não encontrada");
        }
    }

    private async Task ValidateProductsAndStockAsync(List<CreateOrderItemDto> items)
    {
        foreach (var item in items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product == null)
            {
                throw new KeyNotFoundException($"Produto com ID {item.ProductId} não encontrado");
            }

            if (product.Stock < item.Quantity)
            {
                throw new ArgumentException(
                    $"Estoque insuficiente para o produto {product.Name}. " +
                    $"Disponível: {product.Stock}, Solicitado: {item.Quantity}");
            }

            if (Math.Abs(product.Price - item.UnitPrice) > 0.01m)
            {
                throw new ArgumentException(
                    $"Preço unitário incorreto para o produto {product.Name}. " +
                    $"Preço atual: R$ {product.Price:F2}, Informado: R$ {item.UnitPrice:F2}");
            }

            if (product.Name != item.ProductName.Trim())
            {
                throw new ArgumentException(
                    $"Nome do produto incorreto. Esperado: {product.Name}, Informado: {item.ProductName}");
            }
        }
    }

    private async Task ValidateProductsBelongToEnterpriseAsync(List<CreateOrderItemDto> items, Guid enterpriseId)
    {
        foreach (var item in items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product != null && product.EnterpriseId != enterpriseId)
            {
                throw new ArgumentException($"O produto {product.Name} não pertence à empresa especificada");
            }
        }
    }

    private void ValidateOrderCalculations(CreateOrderDto orderDto)
    {
        if (orderDto.Items == null)
            return;

        decimal calculatedTotal = 0;

        foreach (var item in orderDto.Items)
        {
            var expectedSubtotal = item.Quantity * item.UnitPrice;
            if (Math.Abs(item.Subtotal - expectedSubtotal) > 0.01m)
            {
                throw new ArgumentException(
                    $"Subtotal incorreto para o produto {item.ProductName}. " +
                    $"Esperado: R$ {expectedSubtotal:F2}, Informado: R$ {item.Subtotal:F2}");
            }

            calculatedTotal += item.Subtotal;
        }

        if (orderDto.TotalAmount.HasValue && Math.Abs(orderDto.TotalAmount.Value - calculatedTotal) > 0.01m)
        {
            throw new ArgumentException(
                $"Total do pedido incorreto. " +
                $"Esperado: R$ {calculatedTotal:F2}, Informado: R$ {orderDto.TotalAmount:F2}");
        }
    }

    private async Task ValidateUniqueOrderNumberAsync(string orderNumber)
    {
        var existingOrder = await _context.Orders
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

        if (existingOrder != null)
        {
            throw new InvalidOperationException($"Já existe um pedido com o número {orderNumber}");
        }
    }

    public async Task<List<Order>> GetAllAsync()
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Enterprise)
            .Include(o => o.Items)
            .ToListAsync();
    }

    public async Task<List<Order>> GetByCustomerAsync(Guid customerId)
    {
        await ValidateCustomerExistsAsync(customerId);

        return await _context.Orders
            .Where(o => o.CustomerId == customerId)
            .Include(o => o.Customer)
            .Include(o => o.Enterprise)
            .Include(o => o.Items)
            .ToListAsync();
    }

    public async Task<List<Order>> GetByEnterpriseAsync(Guid enterpriseId)
    {
        await ValidateEnterpriseExistsAsync(enterpriseId);

        return await _context.Orders
            .Where(o => o.EnterpriseId == enterpriseId)
            .Include(o => o.Customer)
            .Include(o => o.Enterprise)
            .Include(o => o.Items)
            .ToListAsync();
    }

    public async Task<Order> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("ID do pedido é obrigatório");
        }

        var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Enterprise)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            throw new KeyNotFoundException($"Pedido com ID {id} não encontrado");
        }

        return order;
    }

    public async Task<Order> UpdateAsync(Guid id, UpdateOrderDto updateOrderDto)
    {
        var order = await GetByIdAsync(id);
        await ValidateOrderUpdateAsync(order, updateOrderDto);

        if (!string.IsNullOrWhiteSpace(updateOrderDto.OrderNumber))
        {
            order.OrderNumber = updateOrderDto.OrderNumber.Trim();
        }

        if (updateOrderDto.OrderDate.HasValue)
        {
            order.OrderDate = updateOrderDto.OrderDate.Value.Date;
        }

        if (updateOrderDto.Status.HasValue)
        {
            order.Status = updateOrderDto.Status.Value;
        }

        if (updateOrderDto.CustomerId.HasValue)
        {
            order.CustomerId = updateOrderDto.CustomerId.Value;
        }

        if (updateOrderDto.EnterpriseId.HasValue)
        {
            order.EnterpriseId = updateOrderDto.EnterpriseId.Value;
        }

        if (updateOrderDto.TotalAmount.HasValue)
        {
            order.TotalAmount = updateOrderDto.TotalAmount.Value;
        }

        if (updateOrderDto.Notes != null)
        {
            order.Notes = updateOrderDto.Notes;
        }

        if (updateOrderDto.Items != null && updateOrderDto.Items.Any())
        {
            // Remover itens antigos
            var existingItems = await _context.OrderItems
                .Where(oi => oi.OrderId == order.Id)
                .ToListAsync();

            // Liberar estoque dos itens antigos
            foreach (var item in existingItems)
            {
                await _productService.ReleaseStockAsync(item.ProductId, item.Quantity);
            }

            _context.OrderItems.RemoveRange(existingItems);

            // Validar novos itens
            await ValidateProductsAndStockAsync(updateOrderDto.Items);
            await ValidateProductsBelongToEnterpriseAsync(
                updateOrderDto.Items,
                updateOrderDto.EnterpriseId ?? order.EnterpriseId);

            // Criar novos itens
            var newOrderItems = updateOrderDto.Items.Select(item => new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = item.ProductId,
                ProductName = item.ProductName.Trim(),
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Subtotal = item.Subtotal,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }).ToList();

            _context.OrderItems.AddRange(newOrderItems);

            // Reservar estoque dos novos itens
            foreach (var item in updateOrderDto.Items)
            {
                await _productService.ReserveStockAsync(item.ProductId, item.Quantity);
            }
        }

        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetByIdAsync(order.Id);
    }

    private async Task ValidateOrderUpdateAsync(Order existingOrder, UpdateOrderDto updateOrderDto)
    {
        if (existingOrder.Status == OrderStatus.Completed)
        {
            throw new InvalidOperationException("Não é possível alterar um pedido já completado");
        }

        if (existingOrder.Status == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException("Não é possível alterar um pedido cancelado");
        }

        if (!string.IsNullOrWhiteSpace(updateOrderDto.OrderNumber) &&
            updateOrderDto.OrderNumber != existingOrder.OrderNumber)
        {
            ValidateOrderNumber(updateOrderDto.OrderNumber);
            await ValidateUniqueOrderNumberAsync(updateOrderDto.OrderNumber);
        }

        if (updateOrderDto.CustomerId.HasValue && updateOrderDto.CustomerId.Value != existingOrder.CustomerId)
        {
            await ValidateCustomerExistsAsync(updateOrderDto.CustomerId.Value);
        }

        if (updateOrderDto.EnterpriseId.HasValue && updateOrderDto.EnterpriseId.Value != existingOrder.EnterpriseId)
        {
            await ValidateEnterpriseExistsAsync(updateOrderDto.EnterpriseId.Value);
        }

        if (updateOrderDto.Status.HasValue && updateOrderDto.Status.Value != existingOrder.Status)
        {
            ValidateStatusTransition(existingOrder.Status, updateOrderDto.Status.Value);
        }

        if (updateOrderDto.TotalAmount.HasValue)
        {
            ValidateMinOrderValue(updateOrderDto.TotalAmount.Value);
        }

        if (updateOrderDto.OrderDate.HasValue)
        {
            ValidateOrderDate(updateOrderDto.OrderDate.Value);
        }

        if (updateOrderDto.Items != null)
        {
            ValidateOrderItems(updateOrderDto.Items);
        }
    }

    public async Task<Order> UpdateStatusAsync(Guid id, OrderStatus status)
    {
        var order = await GetByIdAsync(id);

        if (order.Status == OrderStatus.Completed)
        {
            throw new InvalidOperationException("Não é possível alterar o status de um pedido já completado");
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException("Não é possível alterar o status de um pedido cancelado");
        }

        ValidateStatusTransition(order.Status, status);

        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return order;
    }

    private void ValidateStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        var validTransitions = new Dictionary<OrderStatus, List<OrderStatus>>
        {
            { OrderStatus.Pending, new List<OrderStatus> { OrderStatus.Completed, OrderStatus.Cancelled } },
            { OrderStatus.Completed, new List<OrderStatus>() },
            { OrderStatus.Cancelled, new List<OrderStatus>() }
        };

        if (!validTransitions.ContainsKey(currentStatus) ||
            !validTransitions[currentStatus].Contains(newStatus))
        {
            throw new ArgumentException($"Transição de status inválida: de {currentStatus} para {newStatus}");
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var order = await GetByIdAsync(id);

        if (order.Status == OrderStatus.Completed)
        {
            throw new InvalidOperationException("Não é possível excluir um pedido completado");
        }

        // Liberar estoque
        if (order.Items != null && order.Items.Any())
        {
            foreach (var item in order.Items)
            {
                await _productService.ReleaseStockAsync(item.ProductId, item.Quantity);
            }

            _context.OrderItems.RemoveRange(order.Items);
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
    }
}

