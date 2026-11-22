namespace InsurancePortal.QuoteService.Domain.ValueObjects
{
    /// <summary>
    /// Immutable value object for Auto Insurance specific details
    /// </summary>
    public class AutoQuoteDetails
    {
        public AutoQuoteDetails(
            string VIN,
            string Make,
            string Model,
            int Year,
            string Color,
            int Mileage,
            string DrivingRecord,
            int NumberOfDrivers,
            decimal EstimatedAnnualMileage)
        {
            if (string.IsNullOrWhiteSpace(VIN))
                throw new ArgumentException("VIN cannot be empty");
            if (string.IsNullOrWhiteSpace(Make))
                throw new ArgumentException("Make cannot be empty");
            if (Year < 1900)
                throw new ArgumentException("Year must be 1900 or later");
            if (Mileage < 0)
                throw new ArgumentException("Mileage cannot be negative");
            if (NumberOfDrivers <= 0)
                throw new ArgumentException("Number of drivers must be greater than 0");
            if (EstimatedAnnualMileage <= 0)
                throw new ArgumentException("Estimated annual mileage must be greater than 0");

            this.VIN = VIN;
            this.Make = Make;
            this.Model = Model;
            this.Year = Year;
            this.Color = Color;
            this.Mileage = Mileage;
            this.DrivingRecord = DrivingRecord;
            this.NumberOfDrivers = NumberOfDrivers;
            this.EstimatedAnnualMileage = EstimatedAnnualMileage;
        }

        public string VIN { get; }
        public string Make { get; }
        public string Model { get; }
        public int Year { get; }
        public string Color { get; }
        public int Mileage { get; }
        public string DrivingRecord { get; }
        public int NumberOfDrivers { get; }
        public decimal EstimatedAnnualMileage { get; }

        protected bool Equals(AutoQuoteDetails other)
        {
            return VIN == other.VIN &&
                   Make == other.Make &&
                   Model == other.Model &&
                   Year == other.Year &&
                   Color == other.Color &&
                   Mileage == other.Mileage &&
                   DrivingRecord == other.DrivingRecord &&
                   NumberOfDrivers == other.NumberOfDrivers &&
                   EstimatedAnnualMileage == other.EstimatedAnnualMileage;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AutoQuoteDetails)obj);
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(VIN);
            hash.Add(Make);
            hash.Add(Model);
            hash.Add(Year);
            hash.Add(Color);
            hash.Add(Mileage);
            hash.Add(DrivingRecord);
            hash.Add(NumberOfDrivers);
            hash.Add(EstimatedAnnualMileage);
            return hash.ToHashCode();
        }
    }
}
