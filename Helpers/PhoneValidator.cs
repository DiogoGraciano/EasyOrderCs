namespace EasyOrderCs.Helpers;

public static class PhoneValidator
{
    private static readonly HashSet<string> ValidAreaCodes = new()
    {
        "11", "12", "13", "14", "15", "16", "17", "18", "19",
        "21", "22", "24", "27", "28",
        "31", "32", "33", "34", "35", "37", "38",
        "41", "42", "43", "44", "45", "46", "47", "48", "49",
        "51", "53", "54", "55",
        "61", "62", "63", "64", "65", "67", "68", "69",
        "71", "73", "74", "75", "77", "79",
        "81", "82", "83", "84", "85", "86", "87", "88", "89",
        "91", "92", "93", "94", "95", "96", "97", "98", "99"
    };

    public static bool IsValid(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;

        // Remove formatação
        var cleanPhone = phone.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "").Trim();

        // Verifica se tem 10 ou 11 dígitos
        if (cleanPhone.Length < 10 || cleanPhone.Length > 11)
            return false;

        // Verifica se todos os dígitos são iguais
        if (cleanPhone.All(c => c == cleanPhone[0]))
            return false;

        // Verifica código de área
        var areaCode = cleanPhone.Substring(0, 2);
        if (!ValidAreaCodes.Contains(areaCode))
            return false;

        return true;
    }

    public static string Clean(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return string.Empty;

        return phone.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "").Trim();
    }
}

