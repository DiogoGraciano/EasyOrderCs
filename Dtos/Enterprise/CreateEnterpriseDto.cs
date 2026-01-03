using System.ComponentModel.DataAnnotations;

namespace EasyOrderCs.Dtos.Enterprise;

public class CreateEnterpriseDto
{
    [Required(ErrorMessage = "O nome legal é obrigatório")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "O nome legal deve ter entre 1 e 255 caracteres")]
    public string LegalName { get; set; } = string.Empty;

    [Required(ErrorMessage = "O nome fantasia é obrigatório")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "O nome fantasia deve ter entre 1 e 255 caracteres")]
    public string TradeName { get; set; } = string.Empty;

    public string? Logo { get; set; }

    [Required(ErrorMessage = "A data de fundação deve ser uma data válida")]
    public DateTime FoundationDate { get; set; }

    [Required(ErrorMessage = "O CNPJ é obrigatório")]
    [StringLength(14, MinimumLength = 14, ErrorMessage = "O CNPJ deve ter exatamente 14 dígitos")]
    public string Cnpj { get; set; } = string.Empty;

    [Required(ErrorMessage = "O endereço é obrigatório")]
    public string Address { get; set; } = string.Empty;
}

