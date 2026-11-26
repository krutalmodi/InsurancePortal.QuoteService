namespace InsurancePortal.QuoteService.Application.Quotes.Commands
{
    public sealed class QuoteAcceptanceConflictException : Exception
    {
        public QuoteAcceptanceConflictException(string message) : base(message)
        {
        }
    }
}
