namespace FluencyHub.PaymentProcessing.Domain;

public class CardDetails
{
    public string CardHolderName { get; private set; }
    public string MaskedCardNumber { get; private set; }
    public string ExpiryMonth { get; private set; }
    public string ExpiryYear { get; private set; }

    // Construtor para EF Core
    private CardDetails()
    { }

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
            expiryYear += 2000;

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
} 