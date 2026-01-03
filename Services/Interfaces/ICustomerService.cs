using EasyOrderCs.Dtos.Customer;
using EasyOrderCs.Models;

namespace EasyOrderCs.Services.Interfaces;

public interface ICustomerService
{
    Task<Customer> CreateAsync(CreateCustomerDto createCustomerDto);
    Task<List<Customer>> GetAllAsync();
    Task<Customer> GetByIdAsync(Guid id);
    Task<Customer> UpdateAsync(Guid id, UpdateCustomerDto updateCustomerDto);
    Task DeleteAsync(Guid id);
    Task<Customer> UploadPhotoAsync(Guid id, IFormFile file);
}

