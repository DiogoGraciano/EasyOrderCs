using Microsoft.EntityFrameworkCore;
using EasyOrderCs.Data;
using EasyOrderCs.Models;
using EasyOrderCs.Dtos.Enterprise;
using EasyOrderCs.Helpers;
using EasyOrderCs.Services.Interfaces;

namespace EasyOrderCs.Services;

public class EnterpriseService : IEnterpriseService
{
    private readonly ApplicationDbContext _context;
    private readonly IFileUploadService _fileUploadService;

    public EnterpriseService(ApplicationDbContext context, IFileUploadService fileUploadService)
    {
        _context = context;
        _fileUploadService = fileUploadService;
    }

    public async Task<Enterprise> CreateAsync(CreateEnterpriseDto createEnterpriseDto, IFormFile? file = null)
    {
        // Normaliza o CNPJ removendo formatação antes de validar e salvar
        var cleanCnpj = CnpjValidator.Clean(createEnterpriseDto.Cnpj);
        createEnterpriseDto.Cnpj = cleanCnpj;

        await ValidateEnterpriseCreationAsync(createEnterpriseDto);

        var enterprise = new Enterprise
        {
            Id = Guid.NewGuid(),
            LegalName = createEnterpriseDto.LegalName.Trim(),
            TradeName = createEnterpriseDto.TradeName.Trim(),
            Cnpj = cleanCnpj,
            Address = createEnterpriseDto.Address.Trim(),
            FoundationDate = createEnterpriseDto.FoundationDate.Date,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (file != null)
        {
            ValidateLogoFile(file);
            var logoUrl = await _fileUploadService.UploadFileAsync(file, "enterprises");
            enterprise.Logo = logoUrl;
        }

        _context.Enterprises.Add(enterprise);
        await _context.SaveChangesAsync();

        return enterprise;
    }

    private async Task ValidateEnterpriseCreationAsync(CreateEnterpriseDto createEnterpriseDto)
    {
        ValidateBasicData(createEnterpriseDto);
        ValidateCNPJ(createEnterpriseDto.Cnpj);
        ValidateFoundationDate(createEnterpriseDto.FoundationDate);
        await ValidateUniquenessAsync(createEnterpriseDto);
    }

    private void ValidateBasicData(CreateEnterpriseDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.LegalName))
        {
            var trimmed = dto.LegalName.Trim();
            if (trimmed.Length < 3)
            {
                throw new ArgumentException("A razão social deve ter pelo menos 3 caracteres");
            }

            if (trimmed.Length > 255)
            {
                throw new ArgumentException("A razão social não pode ter mais de 255 caracteres");
            }
        }

        if (!string.IsNullOrWhiteSpace(dto.TradeName))
        {
            var trimmed = dto.TradeName.Trim();
            if (trimmed.Length < 2)
            {
                throw new ArgumentException("O nome fantasia deve ter pelo menos 2 caracteres");
            }

            if (trimmed.Length > 255)
            {
                throw new ArgumentException("O nome fantasia não pode ter mais de 255 caracteres");
            }
        }

        if (!string.IsNullOrWhiteSpace(dto.Address))
        {
            var trimmed = dto.Address.Trim();
            if (trimmed.Length < 10)
            {
                throw new ArgumentException("O endereço deve ter pelo menos 10 caracteres");
            }

            if (trimmed.Length > 500)
            {
                throw new ArgumentException("O endereço não pode ter mais de 500 caracteres");
            }
        }
    }

    private void ValidateCNPJ(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return;

        var cleanCNPJ = CnpjValidator.Clean(cnpj);

        if (cleanCNPJ.Length != 14)
        {
            throw new ArgumentException("CNPJ deve ter 14 dígitos");
        }

        if (!CnpjValidator.IsValid(cleanCNPJ))
        {
            throw new ArgumentException("CNPJ inválido");
        }
    }

    private void ValidateFoundationDate(DateTime foundationDate)
    {
        var today = DateTime.UtcNow.Date;

        if (foundationDate.Date > today)
        {
            throw new ArgumentException("A data de fundação não pode ser no futuro");
        }

        const int maxYearsOld = 200;
        var diffYears = today.Year - foundationDate.Year;

        if (diffYears > maxYearsOld)
        {
            throw new ArgumentException($"A data de fundação não pode ser anterior a {maxYearsOld} anos");
        }

        var diffDays = (today - foundationDate.Date).TotalDays;

        if (diffDays < 1)
        {
            throw new ArgumentException("A data de fundação deve ser pelo menos 1 dia no passado");
        }
    }

    private async Task ValidateUniquenessAsync(CreateEnterpriseDto enterpriseDto)
    {
        if (!string.IsNullOrWhiteSpace(enterpriseDto.Cnpj))
        {
            var existingByCNPJ = await _context.Enterprises
                .FirstOrDefaultAsync(e => e.Cnpj == enterpriseDto.Cnpj);

            if (existingByCNPJ != null)
            {
                throw new InvalidOperationException("Já existe uma empresa com este CNPJ");
            }
        }

        if (!string.IsNullOrWhiteSpace(enterpriseDto.LegalName))
        {
            var existingByLegalName = await _context.Enterprises
                .FirstOrDefaultAsync(e => e.LegalName == enterpriseDto.LegalName.Trim());

            if (existingByLegalName != null)
            {
                throw new InvalidOperationException("Já existe uma empresa com esta razão social");
            }
        }
    }

    public async Task<List<Enterprise>> GetAllAsync()
    {
        return await _context.Enterprises.ToListAsync();
    }

    public async Task<Enterprise> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("ID da empresa é obrigatório");
        }

        var enterprise = await _context.Enterprises
            .Include(e => e.Orders)
            .Include(e => e.Products)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (enterprise == null)
        {
            throw new KeyNotFoundException($"Empresa com ID {id} não encontrada");
        }

        return enterprise;
    }

    public async Task<Enterprise> UpdateAsync(Guid id, UpdateEnterpriseDto updateEnterpriseDto, IFormFile? file = null)
    {
        var enterprise = await GetByIdAsync(id);

        // Normaliza o CNPJ removendo formatação antes de validar e salvar
        if (!string.IsNullOrWhiteSpace(updateEnterpriseDto.Cnpj))
        {
            updateEnterpriseDto.Cnpj = CnpjValidator.Clean(updateEnterpriseDto.Cnpj);
        }

        await ValidateEnterpriseUpdateAsync(enterprise, updateEnterpriseDto);

        if (!string.IsNullOrWhiteSpace(updateEnterpriseDto.LegalName))
        {
            enterprise.LegalName = updateEnterpriseDto.LegalName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(updateEnterpriseDto.TradeName))
        {
            enterprise.TradeName = updateEnterpriseDto.TradeName.Trim();
        }

        if (updateEnterpriseDto.FoundationDate.HasValue)
        {
            enterprise.FoundationDate = updateEnterpriseDto.FoundationDate.Value.Date;
        }

        if (!string.IsNullOrWhiteSpace(updateEnterpriseDto.Cnpj))
        {
            enterprise.Cnpj = updateEnterpriseDto.Cnpj;
        }

        if (!string.IsNullOrWhiteSpace(updateEnterpriseDto.Address))
        {
            enterprise.Address = updateEnterpriseDto.Address.Trim();
        }

        if (file != null)
        {
            if (!string.IsNullOrEmpty(enterprise.Logo))
            {
                await _fileUploadService.DeleteFileAsync(enterprise.Logo);
            }

            ValidateLogoFile(file);
            var logoUrl = await _fileUploadService.UploadFileAsync(file, "enterprises");
            enterprise.Logo = logoUrl;
        }

        enterprise.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return enterprise;
    }

    private async Task ValidateEnterpriseUpdateAsync(Enterprise existingEnterprise, UpdateEnterpriseDto updateEnterpriseDto)
    {
        if (!string.IsNullOrWhiteSpace(updateEnterpriseDto.LegalName))
        {
            var trimmed = updateEnterpriseDto.LegalName.Trim();
            if (trimmed.Length < 3)
            {
                throw new ArgumentException("A razão social deve ter pelo menos 3 caracteres");
            }

            if (trimmed.Length > 255)
            {
                throw new ArgumentException("A razão social não pode ter mais de 255 caracteres");
            }
        }

        if (!string.IsNullOrWhiteSpace(updateEnterpriseDto.TradeName))
        {
            var trimmed = updateEnterpriseDto.TradeName.Trim();
            if (trimmed.Length < 2)
            {
                throw new ArgumentException("O nome fantasia deve ter pelo menos 2 caracteres");
            }

            if (trimmed.Length > 255)
            {
                throw new ArgumentException("O nome fantasia não pode ter mais de 255 caracteres");
            }
        }

        if (!string.IsNullOrWhiteSpace(updateEnterpriseDto.Address))
        {
            var trimmed = updateEnterpriseDto.Address.Trim();
            if (trimmed.Length < 10)
            {
                throw new ArgumentException("O endereço deve ter pelo menos 10 caracteres");
            }

            if (trimmed.Length > 500)
            {
                throw new ArgumentException("O endereço não pode ter mais de 500 caracteres");
            }
        }

        if (!string.IsNullOrWhiteSpace(updateEnterpriseDto.Cnpj) && 
            updateEnterpriseDto.Cnpj != existingEnterprise.Cnpj)
        {
            ValidateCNPJ(updateEnterpriseDto.Cnpj);
            var existingByCNPJ = await _context.Enterprises
                .FirstOrDefaultAsync(e => e.Cnpj == updateEnterpriseDto.Cnpj && e.Id != existingEnterprise.Id);

            if (existingByCNPJ != null)
            {
                throw new InvalidOperationException("Já existe uma empresa com este CNPJ");
            }
        }

        if (!string.IsNullOrWhiteSpace(updateEnterpriseDto.LegalName) && 
            updateEnterpriseDto.LegalName.Trim() != existingEnterprise.LegalName)
        {
            var existingByLegalName = await _context.Enterprises
                .FirstOrDefaultAsync(e => e.LegalName == updateEnterpriseDto.LegalName.Trim() && e.Id != existingEnterprise.Id);

            if (existingByLegalName != null)
            {
                throw new InvalidOperationException("Já existe uma empresa com esta razão social");
            }
        }

        if (updateEnterpriseDto.FoundationDate.HasValue)
        {
            ValidateFoundationDate(updateEnterpriseDto.FoundationDate.Value);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var enterprise = await GetByIdAsync(id);

        if (enterprise.Products != null && enterprise.Products.Any())
        {
            throw new InvalidOperationException("Não é possível excluir uma empresa que possui produtos");
        }

        if (enterprise.Orders != null && enterprise.Orders.Any())
        {
            throw new InvalidOperationException("Não é possível excluir uma empresa que possui pedidos");
        }

        if (!string.IsNullOrEmpty(enterprise.Logo))
        {
            await _fileUploadService.DeleteFileAsync(enterprise.Logo);
        }

        _context.Enterprises.Remove(enterprise);
        await _context.SaveChangesAsync();
    }

    public async Task<Enterprise> UploadLogoAsync(Guid id, IFormFile file)
    {
        var enterprise = await GetByIdAsync(id);

        ValidateLogoFile(file);

        if (!string.IsNullOrEmpty(enterprise.Logo))
        {
            await _fileUploadService.DeleteFileAsync(enterprise.Logo);
        }

        var logoUrl = await _fileUploadService.UploadFileAsync(file, "enterprises");
        enterprise.Logo = logoUrl;
        enterprise.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return enterprise;
    }

    private void ValidateLogoFile(IFormFile file)
    {
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType))
        {
            throw new ArgumentException("Tipo de arquivo inválido. Permitidos: JPEG, PNG, WebP");
        }

        const long maxSize = 10 * 1024 * 1024; // 10MB
        if (file.Length > maxSize)
        {
            throw new ArgumentException("Arquivo muito grande. Máximo: 10MB");
        }

        if (file.FileName.Length > 255)
        {
            throw new ArgumentException("Nome do arquivo muito longo. Máximo: 255 caracteres");
        }
    }
}

