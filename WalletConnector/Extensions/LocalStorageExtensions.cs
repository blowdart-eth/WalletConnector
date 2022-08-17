using System.Net.Http.Headers;
using Blazored.LocalStorage;

namespace WalletConnector.Extensions
{
    internal static class LocalStorageExtensions
    {
        public static async Task TryAddBearerTokenAsync(this HttpClient http, string account, ILocalStorageService storage)
        {
            if (await storage.ContainKeyAsync($"JWT:{account}"))
            {
                var token = await storage.GetItemAsStringAsync($"JWT:{account}");
                if (!string.IsNullOrWhiteSpace(token))
                {
                    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));
                }
            }
        }
    }
}
