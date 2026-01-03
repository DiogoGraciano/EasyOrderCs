using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using EasyOrderCs.Data;
using EasyOrderCs.Models;
using EasyOrderCs.Dtos.Customer;
using EasyOrderCs.Helpers;
using EasyOrderCs.Services.Interfaces;

namespace EasyOrderCs.Services;

public class CustomerService : ICustomerService
{
    private readonly ApplicationDbContext _context;
    private readonly IFileUploadService _fileUploadService;

    public CustomerService(ApplicationDbContext context, IFileUploadService fileUploadService)
    {
        _context = context;
        _fileUploadService = fileUploadService;
    }

    public async Task<Customer> CreateAsync(CreateCustomerDto createCustomerDto)
    {
        await ValidateCustomerCreationAsync(createCustomerDto);

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = createCustomerDto.Name.Trim(),
            Email = createCustomerDto.Email.Trim().ToLower(),
            Phone = createCustomerDto.Phone.Trim(),
            Cpf = CpfValidator.Clean(createCustomerDto.Cpf),
            Address = createCustomerDto.Address.Trim(),
            Photo = createCustomerDto.Photo,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return customer;
    }

    private async Task ValidateCustomerCreationAsync(CreateCustomerDto createCustomerDto)
    {
        ValidateBasicData(createCustomerDto);
        ValidateCPF(createCustomerDto.Cpf);
        ValidateEmail(createCustomerDto.Email);
        ValidatePhone(createCustomerDto.Phone);
        await ValidateUniquenessAsync(createCustomerDto);
    }

    private void ValidateBasicData(CreateCustomerDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            var trimmedName = dto.Name.Trim();
            if (trimmedName.Length < 2)
            {
                throw new ArgumentException("O nome deve ter pelo menos 2 caracteres");
            }

            if (trimmedName.Length > 255)
            {
                throw new ArgumentException("O nome não pode ter mais de 255 caracteres");
            }

            var namePattern = new Regex(@"^[a-zA-ZÀ-ÿ\s]+$");
            if (!namePattern.IsMatch(trimmedName))
            {
                throw new ArgumentException("O nome deve conter apenas letras e espaços");
            }
        }

        if (!string.IsNullOrWhiteSpace(dto.Address))
        {
            var trimmedAddress = dto.Address.Trim();
            if (trimmedAddress.Length < 10)
            {
                throw new ArgumentException("O endereço deve ter pelo menos 10 caracteres");
            }

            if (trimmedAddress.Length > 500)
            {
                throw new ArgumentException("O endereço não pode ter mais de 500 caracteres");
            }
        }
    }

    private void ValidateCPF(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return;

        var cleanCPF = CpfValidator.Clean(cpf);

        if (cleanCPF.Length != 11)
        {
            throw new ArgumentException("CPF deve ter 11 dígitos");
        }

        if (!CpfValidator.IsValid(cleanCPF))
        {
            throw new ArgumentException("CPF inválido");
        }
    }

    private void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return;

        var emailPattern = new Regex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$");
        if (!emailPattern.IsMatch(email))
        {
            throw new ArgumentException("Formato de email inválido");
        }

        if (email.Length > 255)
        {
            throw new ArgumentException("Email não pode ter mais de 255 caracteres");
        }
    }

    private void ValidatePhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return;

        if (!PhoneValidator.IsValid(phone))
        {
            throw new ArgumentException("Telefone inválido");
        }
    }

    private async Task ValidateUniquenessAsync(CreateCustomerDto createCustomerDto)
    {
        if (!string.IsNullOrWhiteSpace(createCustomerDto.Email))
        {
            var existingByEmail = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == createCustomerDto.Email.Trim().ToLower());

            if (existingByEmail != null)
            {
                throw new InvalidOperationException("Já existe um cliente com este email");
            }
        }

        if (!string.IsNullOrWhiteSpace(createCustomerDto.Cpf))
        {
            var cleanCpf = CpfValidator.Clean(createCustomerDto.Cpf);
            var existingByCPF = await _context.Customers
                .FirstOrDefaultAsync(c => c.Cpf == cleanCpf);

            if (existingByCPF != null)
            {
                throw new InvalidOperationException("Já existe um cliente com este CPF");
            }
        }
    }

    public async Task<List<Customer>> GetAllAsync()
    {
        return await _context.Customers
            .Include(c => c.Orders)
            .ToListAsync();
    }

    public async Task<Customer> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("ID do cliente é obrigatório");
        }

        var customer = await _context.Customers
            .Include(c => c.Orders)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null)
        {
            throw new KeyNotFoundException($"Cliente com ID {id} não encontrado");
        }

        return customer;
    }

    public async Task<Customer> UpdateAsync(Guid id, UpdateCustomerDto updateCustomerDto)
    {
        var customer = await GetByIdAsync(id);

        await ValidateCustomerUpdateAsync(customer, updateCustomerDto);

        if (!string.IsNullOrWhiteSpace(updateCustomerDto.Name))
        {
            customer.Name = updateCustomerDto.Name.Trim();
        }

        if (!string.IsNullOrWhiteSpace(updateCustomerDto.Email))
        {
            customer.Email = updateCustomerDto.Email.Trim().ToLower();
        }

        if (!string.IsNullOrWhiteSpace(updateCustomerDto.Phone))
        {
            customer.Phone = updateCustomerDto.Phone.Trim();
        }

        if (!string.IsNullOrWhiteSpace(updateCustomerDto.Cpf))
        {
            customer.Cpf = CpfValidator.Clean(updateCustomerDto.Cpf);
        }

        if (!string.IsNullOrWhiteSpace(updateCustomerDto.Address))
        {
            customer.Address = updateCustomerDto.Address.Trim();
        }

        if (updateCustomerDto.Photo != null)
        {
            customer.Photo = updateCustomerDto.Photo;
        }

        customer.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return customer;
    }

    private async Task ValidateCustomerUpdateAsync(Customer existingCustomer, UpdateCustomerDto updateCustomerDto)
    {
        if (!string.IsNullOrWhiteSpace(updateCustomerDto.Name))
        {
            var trimmedName = updateCustomerDto.Name.Trim();
            if (trimmedName.Length < 2)
            {
                throw new ArgumentException("O nome deve ter pelo menos 2 caracteres");
            }

            if (trimmedName.Length > 255)
            {
                throw new ArgumentException("O nome não pode ter mais de 255 caracteres");
            }

            var namePattern = new Regex(@"^[a-zA-ZÀ-ÿ\s]+$");
            if (!namePattern.IsMatch(trimmedName))
            {
                throw new ArgumentException("O nome deve conter apenas letras e espaços");
            }
        }

        if (!string.IsNullOrWhiteSpace(updateCustomerDto.Address))
        {
            var trimmedAddress = updateCustomerDto.Address.Trim();
            if (trimmedAddress.Length < 10)
            {
                throw new ArgumentException("O endereço deve ter pelo menos 10 caracteres");
            }

            if (trimmedAddress.Length > 500)
            {
                throw new ArgumentException("O endereço não pode ter mais de 500 caracteres");
            }
        }

        if (!string.IsNullOrWhiteSpace(updateCustomerDto.Cpf) && 
            CpfValidator.Clean(updateCustomerDto.Cpf) != existingCustomer.Cpf)
        {
            ValidateCPF(updateCustomerDto.Cpf);
            var cleanCpf = CpfValidator.Clean(updateCustomerDto.Cpf);
            var existingByCPF = await _context.Customers
                .FirstOrDefaultAsync(c => c.Cpf == cleanCpf && c.Id != existingCustomer.Id);

            if (existingByCPF != null)
            {
                throw new InvalidOperationException("Já existe um cliente com este CPF");
            }
        }

        if (!string.IsNullOrWhiteSpace(updateCustomerDto.Email) && 
            updateCustomerDto.Email.Trim().ToLower() != existingCustomer.Email)
        {
            ValidateEmail(updateCustomerDto.Email);
            var existingByEmail = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == updateCustomerDto.Email.Trim().ToLower() && c.Id != existingCustomer.Id);

            if (existingByEmail != null)
            {
                throw new InvalidOperationException("Já existe um cliente com este email");
            }
        }

        if (!string.IsNullOrWhiteSpace(updateCustomerDto.Phone))
        {
            ValidatePhone(updateCustomerDto.Phone);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var customer = await GetByIdAsync(id);

        if (customer.Orders != null && customer.Orders.Any())
        {
            throw new InvalidOperationException("Não é possível excluir um cliente que possui pedidos");
        }

        if (!string.IsNullOrEmpty(customer.Photo))
        {
            await _fileUploadService.DeleteFileAsync(customer.Photo);
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
    }

    public async Task<Customer> UploadPhotoAsync(Guid id, IFormFile file)
    {
        var customer = await GetByIdAsync(id);

        ValidatePhotoFile(file);

        if (!string.IsNullOrEmpty(customer.Photo))
        {
            await _fileUploadService.DeleteFileAsync(customer.Photo);
        }

        var photoUrl = await _fileUploadService.UploadFileAsync(file, "customers");
        customer.Photo = photoUrl;
        customer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return customer;
    }

    private void ValidatePhotoFile(IFormFile file)
    {
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType))
        {
            throw new ArgumentException("Tipo de arquivo inválido. Permitidos: JPEG, PNG, WebP");
        }

        const long maxSize = 5 * 1024 * 1024; // 5MB
        if (file.Length > maxSize)
        {
            throw new ArgumentException("Arquivo muito grande. Máximo: 5MB");
        }
    }
}

