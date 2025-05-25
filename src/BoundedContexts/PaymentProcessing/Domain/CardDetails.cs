namespace FluencyHub.PaymentProcessing.Domain;

public class CardDetails
{
    public string CardHolderName { get; set; } = string.Empty;
    public string MaskedCardNumber { get; set; } = string.Empty;
    public string ExpiryMonth { get; set; } = string.Empty;
    public string ExpiryYear { get; set; } = string.Empty;

    // Construtor para EF Core
    private CardDetails()
    {
        CardHolderName = string.Empty;
        MaskedCardNumber = string.Empty;
        ExpiryMonth = string.Empty;
        ExpiryYear = string.Empty;
    }

    public CardDetails(string cardHolderName, string cardNumber, string expiryMonth, string expiryYear)
    {
        if (string.IsNullOrWhiteSpace(cardHolderName))
            throw new ArgumentException("O nome do titular do cartão não pode estar vazio", nameof(cardHolderName));

        if (string.IsNullOrWhiteSpace(cardNumber))
            throw new ArgumentException("O número do cartão não pode estar vazio", nameof(cardNumber));

        if (string.IsNullOrWhiteSpace(expiryMonth))
            throw new ArgumentException("O mês de validade não pode estar vazio", nameof(expiryMonth));

        if (string.IsNullOrWhiteSpace(expiryYear))
            throw new ArgumentException("O ano de validade não pode estar vazio", nameof(expiryYear));

        if (!ValidateCardNumber(cardNumber))
            throw new ArgumentException("Número de cartão inválido", nameof(cardNumber));

        if (!ValidateExpiryDate(expiryMonth, expiryYear))
            throw new ArgumentException("Data de validade inválida");

        CardHolderName = cardHolderName;
        MaskedCardNumber = MaskCardNumber(cardNumber);
        ExpiryMonth = expiryMonth;
        ExpiryYear = expiryYear;
    }

    public string GetMaskedCardNumber()
    {
        // O MaskedCardNumber já está no formato "453201******0366"
        // Vamos extrair apenas os últimos 4 dígitos e criar o formato desejado
        var lastFour = MaskedCardNumber.Substring(MaskedCardNumber.Length - 4, 4);
        return $"****-****-****-{lastFour}";
    }

    public bool IsExpired()
    {
        if (!int.TryParse(ExpiryMonth, out int month) || !int.TryParse(ExpiryYear, out int year))
            return true;

        // Trata o formato de ano com 2 dígitos
        if (year < 100)
            year += 2000;

        var now = DateTime.UtcNow;
        var currentYear = now.Year;
        var currentMonth = now.Month;

        return (year < currentYear) || (year == currentYear && month < currentMonth);
    }

    private static bool ValidateCardNumber(string cardNumber)
    {
        // Remove quaisquer caracteres não numéricos
        var digitsOnly = new string(cardNumber.Where(char.IsDigit).ToArray());

        // Verifica o comprimento
        if (digitsOnly.Length < 13 || digitsOnly.Length > 19)
            return false;

        // Verificação básica do algoritmo de Luhn
        int sum = 0;
        bool alternate = false;
        for (int i = digitsOnly.Length - 1; i >= 0; i--)
        {
            int n = int.Parse(digitsOnly[i].ToString());
            if (alternate)
            {
                n *= 2;
                if (n > 9)
                    n -= 9;
            }
            sum += n;
            alternate = !alternate;
        }

        return sum % 10 == 0;
    }

    private static bool ValidateExpiryDate(string month, string year)
    {
        if (!int.TryParse(month, out int expiryMonth) || !int.TryParse(year, out int expiryYear))
            return false;

        if (expiryMonth < 1 || expiryMonth > 12)
            return false;

        // Trata o formato de ano com 2 dígitos
        if (expiryYear < 100)
        {
            // Rejeita anos muito baixos (como 20 que seria 2020)
            if (expiryYear < 25)
                return false;
            expiryYear += 2000;
        }

        var now = DateTime.UtcNow;
        var currentYear = now.Year;
        var currentMonth = now.Month;

        return (expiryYear > currentYear) || (expiryYear == currentYear && expiryMonth >= currentMonth);
    }

    private static string MaskCardNumber(string cardNumber)
    {
        // Remove caracteres não numéricos
        var digitsOnly = new string(cardNumber.Where(char.IsDigit).ToArray());

        // Mantém visíveis apenas os primeiros 6 e os últimos 4 dígitos
        if (digitsOnly.Length <= 10)
            return digitsOnly; // Muito curto para mascarar efetivamente

        string firstSix = digitsOnly.Substring(0, 6);
        string lastFour = digitsOnly.Substring(digitsOnly.Length - 4, 4);
        string masked = new string('*', digitsOnly.Length - 10);

        return $"{firstSix}{masked}{lastFour}";
    }

    // Implementação de igualdade
    public override bool Equals(object? obj)
    {
        if (obj is not CardDetails other)
            return false;

        return CardHolderName == other.CardHolderName &&
               MaskedCardNumber == other.MaskedCardNumber &&
               ExpiryMonth == other.ExpiryMonth &&
               ExpiryYear == other.ExpiryYear;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(CardHolderName, MaskedCardNumber, ExpiryMonth, ExpiryYear);
    }
} 