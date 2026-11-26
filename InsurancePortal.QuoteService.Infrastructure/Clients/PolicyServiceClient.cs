using System.Net.Http.Json;
using InsurancePortal.QuoteService.Application.Clients;
using Microsoft.Extensions.Configuration;

namespace InsurancePortal.QuoteService.Infrastructure.Clients
{
    public sealed class PolicyServiceClient : IPolicyServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public PolicyServiceClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<PolicyIssueResult> IssueFromAcceptedQuoteAsync(PolicyIssueRequest request)
        {
            var apiKey = _configuration["PolicyService:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new PolicyServiceIntegrationException("PolicyService:ApiKey is not configured");

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/internal/policies/issue")
            {
                Content = JsonContent.Create(request)
            };
            httpRequest.Headers.Add("X-PolicyService-Key", apiKey);

            try
            {
                using var response = await _httpClient.SendAsync(httpRequest);
                if (!response.IsSuccessStatusCode)
                    throw new PolicyServiceIntegrationException($"Policy Service returned HTTP {(int)response.StatusCode}");

                return await response.Content.ReadFromJsonAsync<PolicyIssueResult>()
                    ?? throw new PolicyServiceIntegrationException("Policy Service returned an empty issuance response");
            }
            catch (HttpRequestException ex)
            {
                throw new PolicyServiceIntegrationException("Could not issue the policy through Policy Service", ex);
            }
        }
    }
}
