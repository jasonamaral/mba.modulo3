using System.Net.Http.Headers;

namespace FluencyHub.Tests.Integration.Config;

public static class TestsExtensions
{
    public static decimal ApenasNumeros(this string value)
    {
        return Convert.ToDecimal(new string(value.Where(char.IsDigit).ToArray()));
    }

    public static void AtribuirToken(this HttpClient client, string token)
    {
        client.JsonMediaType();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

}