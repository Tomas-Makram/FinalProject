using EcoRecyclersGreenTech.Data.Stores;
using static EcoRecyclersGreenTech.Data.Stores.EnumsProductStatus;
using System.ComponentModel.DataAnnotations;

namespace EcoRecyclersGreenTech.Models.FactoryStore.Products
{
    public class MachineProductDetailsModel : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Machine type is required")]
        public string? MachineType { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        public IFormFile? MachineImage1 { get; set; }
        public IFormFile? MachineImage2 { get; set; }
        public IFormFile? MachineImage3 { get; set; }

        public string? CurrentImageUrl1 { get; set; }
        public string? CurrentImageUrl2 { get; set; }
        public string? CurrentImageUrl3 { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string? Description { get; set; }

        public ProductStatus Status { get; set; } = ProductStatus.Available;

        public decimal? MinOrderQuantity { get; set; }

        [Required(ErrorMessage = "Manufacture date is required")]
        public DateTime ManufactureDate { get; set; }

        [Required(ErrorMessage = "Condition is required")]
        public MachineCondition? Condition { get; set; }

        public string? Brand { get; set; }
        public string? Model { get; set; }
        public int? WarrantyMonths { get; set; }

        // Pickup Location

        [MaxLength(255, ErrorMessage = "Address can't exceed 255 characters")]
        public string? Address { get; set; }

        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public decimal? Latitude { get; set; }

        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public decimal? Longitude { get; set; }

        public bool UseFactoryLocation { get; set; } = true;

        // timings
        [Range(0, 365, ErrorMessage = "Cancel window days must be between 0 and 365")]
        public int CancelWindowDays { get; set; } = 3;

        [Range(0, 365, ErrorMessage = "Delivery days must be between 0 and 365")]
        public int DeliveryDays { get; set; } = 5;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // MinOrderQuantity rules
            if (MinOrderQuantity.HasValue && MinOrderQuantity.Value < 0)
                yield return new ValidationResult(
                    "Minimum order quantity cannot be negative.",
                    new[] { nameof(MinOrderQuantity) }
                );

            if (MinOrderQuantity.HasValue && MinOrderQuantity.Value > Quantity)
                yield return new ValidationResult(
                    "Minimum order quantity cannot be greater than available quantity.",
                    new[] { nameof(MinOrderQuantity), nameof(Quantity) }
                );

            // Location rules
            if (!UseFactoryLocation)
            {
                var hasAddress = !string.IsNullOrWhiteSpace(Address);
                var hasCoords = Latitude.HasValue && Longitude.HasValue;

                if (!hasAddress && !hasCoords)
                {
                    yield return new ValidationResult(
                        "Please provide a pickup location (address or coordinates) or choose Use Factory Location.",
                        new[] { nameof(Address), nameof(Latitude), nameof(Longitude), nameof(UseFactoryLocation) }
                    );
                }
            }

            if (Latitude.HasValue && !Longitude.HasValue)
            {
                yield return new ValidationResult(
                    "Longitude is required when Latitude is provided.",
                    new[] { nameof(Longitude) }
                );
            }

            if (Longitude.HasValue && !Latitude.HasValue)
            {
                yield return new ValidationResult(
                    "Latitude is required when Longitude is provided.",
                    new[] { nameof(Latitude) }
                );
            }
        }
    }
}