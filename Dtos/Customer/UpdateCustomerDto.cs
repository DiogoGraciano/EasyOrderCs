using System.ComponentModel.DataAnnotations;

namespace EasyOrderCs.Dtos.Customer;

public class UpdateCustomerDto
{
    [StringLength(255, MinimumLength = 1, ErrorMessage = "O nome do cliente deve ter entre 1 e 255 caracteres")]
    public string? Name { get; set; }

    [EmailAddress(ErrorMessage = "O email deve ser um endereço de email válido")]
    public string? Email { get; set; }

    [StringLength(20, MinimumLength = 1, ErrorMessage = "O telefone deve ter entre 1 e 20 caracteres")]
    public string? Phone { get; set; }

    public string? Photo { get; set; }

    [StringLength(14, MinimumLength = 11, ErrorMessage = "O CPF deve ter entre 11 e 14 caracteres")]
    public string? Cpf { get; set; }

    public string? Address { get; set; }
}

