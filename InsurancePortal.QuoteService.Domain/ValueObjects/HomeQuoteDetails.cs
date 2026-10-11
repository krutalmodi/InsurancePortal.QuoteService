namespace InsurancePortal.QuoteService.Domain.ValueObjects
{
    /// <summary>
    /// Immutable value object for Home Insurance specific details
    /// </summary>
    public class HomeQuoteDetails
    {
        public HomeQuoteDetails(
            string homeAddress,
            int squareFootage,
            int yearBuilt,
            string homeType,
            decimal homeValue,
            bool hasSecuritySystem,
            bool hasFireAlarm,
            int numberOfOccupants)
        {
            if (string.IsNullOrWhiteSpace(homeAddress))
                throw new ArgumentException("Home address cannot be empty");
            if (squareFootage <= 0)
                throw new ArgumentException("Square footage must be greater than 0");
            if (yearBuilt < 1800)
                throw new ArgumentException("Year built must be 1800 or later");
            if (homeValue <= 0)
                throw new ArgumentException("Home value must be greater than 0");

            HomeAddress = homeAddress;
            SquareFootage = squareFootage;
            YearBuilt = yearBuilt;
            HomeType = homeType;
            HomeValue = homeValue;
            HasSecuritySystem = hasSecuritySystem;
            HasFireAlarm = hasFireAlarm;
            NumberOfOccupants = numberOfOccupants;
        }

        public string HomeAddress { get; }
        public int SquareFootage { get; }
        public int YearBuilt { get; }
        public string HomeType { get; }
        public decimal HomeValue { get; }
        public bool HasSecuritySystem { get; }
        public bool HasFireAlarm { get; }
        public int NumberOfOccupants { get; }

        protected bool Equals(HomeQuoteDetails other)
        {
            return HomeAddress == other.HomeAddress &&
                   SquareFootage == other.SquareFootage &&
                   YearBuilt == other.YearBuilt &&
                   HomeType == other.HomeType &&
                   HomeValue == other.HomeValue &&
                   HasSecuritySystem == other.HasSecuritySystem &&
                   HasFireAlarm == other.HasFireAlarm &&
                   NumberOfOccupants == other.NumberOfOccupants;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((HomeQuoteDetails)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(HomeAddress, SquareFootage, YearBuilt, HomeType, HomeValue, 
                HasSecuritySystem, HasFireAlarm, NumberOfOccupants);
        }
    }
}
