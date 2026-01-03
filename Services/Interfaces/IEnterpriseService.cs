using EasyOrderCs.Dtos.Enterprise;
using EasyOrderCs.Models;

namespace EasyOrderCs.Services.Interfaces;

public interface IEnterpriseService
{
    Task<Enterprise> CreateAsync(CreateEnterpriseDto createEnterpriseDto, IFormFile? file = null);
    Task<List<Enterprise>> GetAllAsync();
    Task<Enterprise> GetByIdAsync(Guid id);
    Task<Enterprise> UpdateAsync(Guid id, UpdateEnterpriseDto updateEnterpriseDto, IFormFile? file = null);
    Task DeleteAsync(Guid id);
    Task<Enterprise> UploadLogoAsync(Guid id, IFormFile file);
}

