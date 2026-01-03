namespace EasyOrderCs.Helpers;

public static class CnpjValidator
{
    public static bool IsValid(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return false;

        // Remove formatação
        var cleanCnpj = cnpj.Replace(".", "").Replace("/", "").Replace("-", "").Trim();

        // Verifica se tem 14 dígitos
        if (cleanCnpj.Length != 14)
            return false;

        // Verifica se todos os dígitos são iguais
        if (cleanCnpj.All(c => c == cleanCnpj[0]))
            return false;

        // Valida primeiro dígito verificador
        int sum = 0;
        int weight = 5;
        for (int i = 0; i < 12; i++)
        {
            sum += int.Parse(cleanCnpj[i].ToString()) * weight;
            weight = weight == 2 ? 9 : weight - 1;
        }
        int remainder = sum % 11;
        int digit1 = remainder < 2 ? 0 : 11 - remainder;

        if (digit1 != int.Parse(cleanCnpj[12].ToString()))
            return false;

        // Valida segundo dígito verificador
        sum = 0;
        weight = 6;
        for (int i = 0; i < 13; i++)
        {
            sum += int.Parse(cleanCnpj[i].ToString()) * weight;
            weight = weight == 2 ? 9 : weight - 1;
        }
        remainder = sum % 11;
        int digit2 = remainder < 2 ? 0 : 11 - remainder;

        return digit2 == int.Parse(cleanCnpj[13].ToString());
    }

    public static string Clean(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return string.Empty;

        return cnpj.Replace(".", "").Replace("/", "").Replace("-", "").Trim();
    }
}

