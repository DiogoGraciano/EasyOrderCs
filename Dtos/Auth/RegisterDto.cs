using System.ComponentModel.DataAnnotations;

namespace EasyOrderCs.Dtos.Auth;

public class RegisterDto
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(255, ErrorMessage = "Nome deve ser uma string")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido. O email deve ter um formato válido (ex: usuario@exemplo.com)")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha é obrigatória")]
    [StringLength(255, MinimumLength = 6, ErrorMessage = "Senha muito curta. A senha deve ter pelo menos 6 caracteres")]
    public string Password { get; set; } = string.Empty;
}

