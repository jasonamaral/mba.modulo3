using System.Net.Http.Headers;

namespace FluencyHub.Tests.Integration.Config;

public static class HttpClientExtensions
{
    public static HttpClient JsonMediaType(this HttpClient client)
    {
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }
} 