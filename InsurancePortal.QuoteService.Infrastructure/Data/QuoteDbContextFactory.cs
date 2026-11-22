using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InsurancePortal.QuoteService.Infrastructure.Data
{
    /// <summary>
    /// Design-time factory for creating DbContext instances during migrations
    /// </summary>
    public class QuoteDbContextFactory : IDesignTimeDbContextFactory<QuoteDbContext>
    {
        public QuoteDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<QuoteDbContext>();
            var connectionString = "Server=.;Database=InsurancePortal_QuoteService;Trusted_Connection=True;Encrypt=False;";

            optionsBuilder.UseSqlServer(connectionString);

            return new QuoteDbContext(optionsBuilder.Options);
        }
    }
}
