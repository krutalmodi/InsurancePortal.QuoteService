using System.Net.Http.Headers;
using System.Net.Http.Json;
using InsurancePortal.QuoteService.Application.Clients;
using InsurancePortal.QuoteService.Application.DTOs.Requests;

namespace InsurancePortal.QuoteService.Infrastructure.Clients
{
    public class LocationClient : ILocationClient
    {
        private readonly HttpClient _httpClient;

        public LocationClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<LocationDto?> GetByIdAsync(Guid id, string accessToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"api/locations/{id}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await _httpClient.SendAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<LocationDto>();
        }

        public async Task<LocationDto> CreateAsync(LocationRequest request, string accessToken)
        {
            using var message = new HttpRequestMessage(HttpMethod.Post, "api/locations")
            {
                Content = JsonContent.Create(request)
            };
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await _httpClient.SendAsync(message);
            response.EnsureSuccessStatusCode();

            return (await response.Content.ReadFromJsonAsync<LocationDto>())!;
        }
    }
}
