using System.ComponentModel.DataAnnotations;

namespace EasyOrderCs.Dtos.Enterprise;

public class UpdateEnterpriseDto
{
    [StringLength(255, MinimumLength = 1, ErrorMessage = "O nome legal deve ter entre 1 e 255 caracteres")]
    public string? LegalName { get; set; }

    [StringLength(255, MinimumLength = 1, ErrorMessage = "O nome fantasia deve ter entre 1 e 255 caracteres")]
    public string? TradeName { get; set; }

    public string? Logo { get; set; }

    public DateTime? FoundationDate { get; set; }

    [StringLength(14, MinimumLength = 14, ErrorMessage = "O CNPJ deve ter exatamente 14 dígitos")]
    public string? Cnpj { get; set; }

    public string? Address { get; set; }
}

