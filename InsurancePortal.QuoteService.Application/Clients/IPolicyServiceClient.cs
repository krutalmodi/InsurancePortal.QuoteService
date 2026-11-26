namespace InsurancePortal.QuoteService.Application.Clients
{
    public interface IPolicyServiceClient
    {
        Task<PolicyIssueResult> IssueFromAcceptedQuoteAsync(PolicyIssueRequest request);
    }

    public sealed record PolicyIssueRequest(
        Guid QuoteId,
        Guid CustomerId,
        string QuoteReferenceNumber,
        DateTime AcceptedAt,
        IReadOnlyList<PolicyCoverageSnapshot> Coverages);

    public sealed record PolicyCoverageSnapshot(string ProductType, decimal Premium, string Currency);

    public sealed record PolicyIssueResult(Guid Id, int PolicyNumber);

    public sealed class PolicyServiceIntegrationException : Exception
    {
        public PolicyServiceIntegrationException(string message) : base(message)
        {
        }

        public PolicyServiceIntegrationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
