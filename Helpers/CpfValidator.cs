namespace EasyOrderCs.Helpers;

public static class CpfValidator
{
    public static bool IsValid(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        // Remove formatação
        var cleanCpf = cpf.Replace(".", "").Replace("-", "").Trim();

        // Verifica se tem 11 dígitos
        if (cleanCpf.Length != 11)
            return false;

        // Verifica se todos os dígitos são iguais
        if (cleanCpf.All(c => c == cleanCpf[0]))
            return false;

        // Valida primeiro dígito verificador
        int sum = 0;
        for (int i = 0; i < 9; i++)
        {
            sum += int.Parse(cleanCpf[i].ToString()) * (10 - i);
        }
        int remainder = 11 - (sum % 11);
        int digit1 = remainder >= 10 ? 0 : remainder;

        if (digit1 != int.Parse(cleanCpf[9].ToString()))
            return false;

        // Valida segundo dígito verificador
        sum = 0;
        for (int i = 0; i < 10; i++)
        {
            sum += int.Parse(cleanCpf[i].ToString()) * (11 - i);
        }
        remainder = 11 - (sum % 11);
        int digit2 = remainder >= 10 ? 0 : remainder;

        return digit2 == int.Parse(cleanCpf[10].ToString());
    }

    public static string Clean(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return string.Empty;

        return cpf.Replace(".", "").Replace("-", "").Trim();
    }
}

