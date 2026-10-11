namespace InsurancePortal.QuoteService.Domain.ValueObjects
{
    /// <summary>
    /// Immutable value object for Auto Insurance specific details
    /// </summary>
    public class AutoQuoteDetails
    {
        public AutoQuoteDetails(
            string vin,
            string make,
            string model,
            int year,
            string color,
            int mileage,
            string drivingRecord,
            int numberOfDrivers,
            decimal estimatedAnnualMileage)
        {
            if (string.IsNullOrWhiteSpace(vin))
                throw new ArgumentException("VIN cannot be empty");
            if (string.IsNullOrWhiteSpace(make))
                throw new ArgumentException("Make cannot be empty");
            if (year < 1900)
                throw new ArgumentException("Year must be 1900 or later");
            if (mileage < 0)
                throw new ArgumentException("Mileage cannot be negative");
            if (numberOfDrivers <= 0)
                throw new ArgumentException("Number of drivers must be greater than 0");
            if (estimatedAnnualMileage <= 0)
                throw new ArgumentException("Estimated annual mileage must be greater than 0");

            VIN = vin;
            Make = make;
            Model = model;
            Year = year;
            Color = color;
            Mileage = mileage;
            DrivingRecord = drivingRecord;
            NumberOfDrivers = numberOfDrivers;
            EstimatedAnnualMileage = estimatedAnnualMileage;
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
