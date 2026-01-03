using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EasyOrderCs.Dtos.Order;
using EasyOrderCs.Services.Interfaces;
using EasyOrderCs.Models;

namespace EasyOrderCs.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult> Create([FromBody] CreateOrderDto createOrderDto)
    {
        try
        {
            var order = await _orderService.CreateAsync(createOrderDto);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] Guid? customerId, [FromQuery] Guid? enterpriseId)
    {
        try
        {
            if (customerId.HasValue)
            {
                var orders = await _orderService.GetByCustomerAsync(customerId.Value);
                return Ok(orders);
            }

            if (enterpriseId.HasValue)
            {
                var orders = await _orderService.GetByEnterpriseAsync(enterpriseId.Value);
                return Ok(orders);
            }

            var allOrders = await _orderService.GetAllAsync();
            return Ok(allOrders);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(Guid id)
    {
        try
        {
            var order = await _orderService.GetByIdAsync(id);
            return Ok(order);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateOrderDto updateOrderDto)
    {
        try
        {
            var order = await _orderService.UpdateAsync(id, updateOrderDto);
            return Ok(order);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}/status")]
    [Authorize]
    public async Task<ActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusDto dto)
    {
        try
        {
            if (!Enum.TryParse<OrderStatus>(dto.Status, true, out var status))
            {
                return BadRequest(new { message = $"Status inválido. Status válidos: {string.Join(", ", Enum.GetNames<OrderStatus>())}" });
            }

            var order = await _orderService.UpdateStatusAsync(id, status);
            return Ok(order);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            await _orderService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class UpdateStatusDto
{
    public string Status { get; set; } = string.Empty;
}

