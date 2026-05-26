using Microsoft.AspNetCore.Components;
using System.Net;
using System.Net.Http.Json;

public static class HttpClientExtensions
{
    public static async Task<T> GetFromJsonOrRedirectAsync<T>(
        this HttpClient client,
        string url,
        NavigationManager nav,
        string redirectOn404 = "/404/")
    {
        var response = await client.GetAsync(url);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            nav.NavigateTo(redirectOn404);
            return default;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }
}