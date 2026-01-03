using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EasyOrderCs.Dtos.Enterprise;
using EasyOrderCs.Services.Interfaces;

namespace EasyOrderCs.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnterpriseController : ControllerBase
{
    private readonly IEnterpriseService _enterpriseService;

    public EnterpriseController(IEnterpriseService enterpriseService)
    {
        _enterpriseService = enterpriseService;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult> Create([FromForm] CreateEnterpriseDto createEnterpriseDto, IFormFile? logo = null)
    {
        try
        {
            var enterprise = await _enterpriseService.CreateAsync(createEnterpriseDto, logo);
            return CreatedAtAction(nameof(GetById), new { id = enterprise.Id }, enterprise);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
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
    public async Task<ActionResult> GetAll()
    {
        try
        {
            var enterprises = await _enterpriseService.GetAllAsync();
            return Ok(enterprises);
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
            var enterprise = await _enterpriseService.GetByIdAsync(id);
            return Ok(enterprise);
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
    public async Task<ActionResult> Update(Guid id, [FromForm] UpdateEnterpriseDto updateEnterpriseDto, IFormFile? logo = null)
    {
        try
        {
            var enterprise = await _enterpriseService.UpdateAsync(id, updateEnterpriseDto, logo);
            return Ok(enterprise);
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
            return Conflict(new { message = ex.Message });
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
            await _enterpriseService.DeleteAsync(id);
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

    [HttpPost("{id}/logo")]
    [Authorize]
    public async Task<ActionResult> UploadLogo(Guid id, IFormFile logo)
    {
        try
        {
            var enterprise = await _enterpriseService.UploadLogoAsync(id, logo);
            return Ok(enterprise);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

