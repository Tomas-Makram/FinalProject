using EcoRecyclersGreenTech.Data.Stores;
using static EcoRecyclersGreenTech.Data.Stores.EnumsProductStatus;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EcoRecyclersGreenTech.Models.FactoryStore.Products
{
    public class RentalModel : IValidatableObject
    {
        public int Id { get; set; }

        // GPS Coordinates
        [Column(TypeName = "decimal(9,6)")]
        public decimal? Latitude { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal? Longitude { get; set; }

        [Required(ErrorMessage = "Location is required")]
        [StringLength(250, ErrorMessage = "Location cannot exceed 250 characters.")]

        [MaxLength(255)]
        public string? Location { get; set; } // location

        [Required(ErrorMessage = "Area is required")]
        [Range(1, double.MaxValue, ErrorMessage = "Area must be greater than 0")]
        public double Area { get; set; }

        [Required(ErrorMessage = "Price per month is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal PricePerMonth { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
        public string? Description { get; set; }

        public ProductStatus Status { get; set; } = ProductStatus.Available;

        [Required(ErrorMessage = "Available from date is required")]
        public DateTime AvailableFrom { get; set; }

        public DateTime? AvailableUntil { get; set; }

        [Required(ErrorMessage = "Condition is required")]
        public RentalCondition? Condition { get; set; }

        public IFormFile? RentalImage1 { get; set; }
        public IFormFile? RentalImage2 { get; set; }
        public IFormFile? RentalImage3 { get; set; }

        public string? CurrentImageUrl1 { get; set; }
        public string? CurrentImageUrl2 { get; set; }
        public string? CurrentImageUrl3 { get; set; }

        // Additional info
        public bool IsFurnished { get; set; }
        public bool HasElectricity { get; set; }
        public bool HasWater { get; set; }

        public int? MinRentalMonths { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (AvailableUntil.HasValue && AvailableUntil.Value.Date < AvailableFrom.Date)
            {
                yield return new ValidationResult(
                    "Available until date cannot be before available from date.",
                    new[] { nameof(AvailableUntil), nameof(AvailableFrom) }
                );
            }

            if (MinRentalMonths.HasValue && MinRentalMonths.Value < 0)
            {
                yield return new ValidationResult(
                    "Minimum rental months cannot be negative.",
                    new[] { nameof(MinRentalMonths) }
                );
            }

            if (Condition == RentalCondition.EmptyLand && IsFurnished)
            {
                yield return new ValidationResult(
                    "Empty land cannot be marked as furnished.",
                    new[] { nameof(IsFurnished), nameof(Condition) }
                );
            }
        }
    }
}
