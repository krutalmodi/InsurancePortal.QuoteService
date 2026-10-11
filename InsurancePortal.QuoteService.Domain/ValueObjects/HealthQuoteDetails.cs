namespace InsurancePortal.QuoteService.Domain.ValueObjects
{
    /// <summary>
    /// Immutable value object for Health Insurance specific details
    /// </summary>
    public class HealthQuoteDetails
    {
        public HealthQuoteDetails(
            int age,
            string healthStatus,
            bool hasPreexistingConditions,
            bool smoker,
            string coverageType,
            int dependents)
        {
            if (age < 18 || age > 125)
                throw new ArgumentException("Age must be between 18 and 125");
            if (string.IsNullOrWhiteSpace(healthStatus))
                throw new ArgumentException("Health status cannot be empty");
            if (dependents < 0)
                throw new ArgumentException("Number of dependents cannot be negative");

            Age = age;
            HealthStatus = healthStatus;
            HasPreexistingConditions = hasPreexistingConditions;
            Smoker = smoker;
            CoverageType = coverageType;
            Dependents = dependents;
        }

        public int Age { get; }
        public string HealthStatus { get; }
        public bool HasPreexistingConditions { get; }
        public bool Smoker { get; }
        public string CoverageType { get; }
        public int Dependents { get; }

        protected bool Equals(HealthQuoteDetails other)
        {
            return Age == other.Age &&
                   HealthStatus == other.HealthStatus &&
                   HasPreexistingConditions == other.HasPreexistingConditions &&
                   Smoker == other.Smoker &&
                   CoverageType == other.CoverageType &&
                   Dependents == other.Dependents;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((HealthQuoteDetails)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Age, HealthStatus, HasPreexistingConditions, Smoker, CoverageType, Dependents);
        }
    }
}
