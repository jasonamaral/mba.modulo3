using System.Text.RegularExpressions;

namespace FluencyHub.Domain.PaymentProcessing;

public class CardDetails
{
    public string CardHolderName { get; private set; }
    public string MaskedCardNumber { get; private set; }
    public string ExpiryMonth { get; private set; }
    public string ExpiryYear { get; private set; }
    
    // EF Core constructor
    private CardDetails() { }
    
    public CardDetails(string cardHolderName, string cardNumber, string expiryMonth, string expiryYear)
    {
        if (string.IsNullOrWhiteSpace(cardHolderName))
            throw new ArgumentException("Card holder name cannot be empty", nameof(cardHolderName));
            
        if (string.IsNullOrWhiteSpace(cardNumber))
            throw new ArgumentException("Card number cannot be empty", nameof(cardNumber));
            
        if (string.IsNullOrWhiteSpace(expiryMonth))
            throw new ArgumentException("Expiry month cannot be empty", nameof(expiryMonth));
            
        if (string.IsNullOrWhiteSpace(expiryYear))
            throw new ArgumentException("Expiry year cannot be empty", nameof(expiryYear));
            
        if (!ValidateCardNumber(cardNumber))
            throw new ArgumentException("Invalid card number", nameof(cardNumber));
            
        if (!ValidateExpiryDate(expiryMonth, expiryYear))
            throw new ArgumentException("Invalid expiry date");
            
        CardHolderName = cardHolderName;
        MaskedCardNumber = MaskCardNumber(cardNumber);
        ExpiryMonth = expiryMonth;
        ExpiryYear = expiryYear;
    }
    
    private bool ValidateCardNumber(string cardNumber)
    {
        // Remove any non-digit characters
        var digitsOnly = new string(cardNumber.Where(char.IsDigit).ToArray());
        
        // Check length
        if (digitsOnly.Length < 13 || digitsOnly.Length > 19)
            return false;
            
        // Basic Luhn algorithm check
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
    
    private bool ValidateExpiryDate(string month, string year)
    {
        if (!int.TryParse(month, out int expiryMonth) || !int.TryParse(year, out int expiryYear))
            return false;
            
        if (expiryMonth < 1 || expiryMonth > 12)
            return false;
            
        // Handle 2-digit year format
        if (expiryYear < 100)
            expiryYear += 2000;
            
        var now = DateTime.UtcNow;
        var currentYear = now.Year;
        var currentMonth = now.Month;
        
        return (expiryYear > currentYear) || (expiryYear == currentYear && expiryMonth >= currentMonth);
    }
    
    private string MaskCardNumber(string cardNumber)
    {
        // Strip non-digit characters
        var digitsOnly = new string(cardNumber.Where(char.IsDigit).ToArray());
        
        // Keep only first 6 and last 4 digits visible
        if (digitsOnly.Length <= 10)
            return digitsOnly; // Too short to mask effectively
            
        string firstSix = digitsOnly.Substring(0, 6);
        string lastFour = digitsOnly.Substring(digitsOnly.Length - 4, 4);
        string masked = new string('*', digitsOnly.Length - 10);
        
        return $"{firstSix}{masked}{lastFour}";
    }
} 