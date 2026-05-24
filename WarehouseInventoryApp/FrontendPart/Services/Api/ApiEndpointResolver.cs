using System.Net.Http;

namespace WarehouseInventory.Desktop.Services.Api;

internal static class ApiEndpointResolver
{
    private static readonly string[] CandidateBaseAddresses =
    [
        "http://localhost:5267/",
        "http://127.0.0.1:5087/",
        "http://localhost:5087/",
        "https://localhost:7275/"
    ];

    public static Uri ResolveBaseUri()
    {
        foreach (var candidate in CandidateBaseAddresses)
        {
            if (IsReachable(candidate))
            {
                return new Uri(candidate);
            }
        }

        return new Uri(CandidateBaseAddresses[0]);
    }

    private static bool IsReachable(string candidate)
    {
        try
        {
            using var client = new HttpClient
            {
                BaseAddress = new Uri(candidate),
                Timeout = TimeSpan.FromSeconds(1.5)
            };

            using var request = new HttpRequestMessage(HttpMethod.Get, "swagger/v1/swagger.json");
            using var response = client.Send(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
