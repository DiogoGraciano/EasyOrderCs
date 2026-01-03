using System.ComponentModel.DataAnnotations;

namespace EasyOrderCs.Dtos.Customer;

public class CreateCustomerDto
{
    [Required(ErrorMessage = "O nome do cliente é obrigatório")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "O nome do cliente deve ter entre 1 e 255 caracteres")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "O email é obrigatório")]
    [EmailAddress(ErrorMessage = "O email deve ser um endereço de email válido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "O telefone é obrigatório")]
    [StringLength(20, MinimumLength = 1, ErrorMessage = "O telefone deve ter entre 1 e 20 caracteres")]
    public string Phone { get; set; } = string.Empty;

    public string? Photo { get; set; }

    [Required(ErrorMessage = "O CPF é obrigatório")]
    [StringLength(14, MinimumLength = 11, ErrorMessage = "O CPF deve ter entre 11 e 14 caracteres")]
    public string Cpf { get; set; } = string.Empty;

    [Required(ErrorMessage = "O endereço é obrigatório")]
    public string Address { get; set; } = string.Empty;
}

