using static EcoRecyclersGreenTech.Data.Stores.EnumsProductStatus;
using System.ComponentModel.DataAnnotations;

namespace EcoRecyclersGreenTech.Models.FactoryStore.Products
{
    public class AuctionModel : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product type is required")]
        [StringLength(150, ErrorMessage = "Product type cannot exceed 150 characters.")]
        public string? AuctionType { get; set; }

        public IFormFile? AuctionImage1 { get; set; }
        public IFormFile? AuctionImage2 { get; set; }
        public IFormFile? AuctionImage3 { get; set; }

        public string? CurrentImageUrl1 { get; set; }
        public string? CurrentImageUrl2 { get; set; }
        public string? CurrentImageUrl3 { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Starting price is required")]
        [Range(typeof(decimal), "0.01", "79228162514264337593543950335",
            ErrorMessage = "Starting price must be greater than 0")]
        public decimal StartPrice { get; set; }

        [Required(ErrorMessage = "Auction start date is required")]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? EndDate { get; set; }

        public ProductStatus Status { get; set; } = ProductStatus.Available;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
        public string? Description { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // EndDate must be after StartDate
            if (EndDate.HasValue && EndDate.Value < StartDate)
            {
                yield return new ValidationResult(
                    "Auction end date cannot be before start date.",
                    new[] { nameof(EndDate), nameof(StartDate) }
                );
            }

            if (StartDate < DateTime.Now.AddMinutes(-1))
            {
                yield return new ValidationResult(
                    "Auction start date cannot be in the past.",
                    new[] { nameof(StartDate) }
                );
            }

            if (EndDate.HasValue)
            {
                var minDuration = TimeSpan.FromMinutes(10);
                if (EndDate.Value - StartDate < minDuration)
                {
                    yield return new ValidationResult(
                        "Auction duration is too short. Please set an end date at least 10 minutes after the start date.",
                        new[] { nameof(EndDate), nameof(StartDate) }
                    );
                }
            }

            var hasAnyImage = (AuctionImage1 != null && AuctionImage1.Length > 0)
                           || (AuctionImage2 != null && AuctionImage2.Length > 0)
                           || (AuctionImage3 != null && AuctionImage3.Length > 0)
                           || !string.IsNullOrWhiteSpace(CurrentImageUrl1)
                           || !string.IsNullOrWhiteSpace(CurrentImageUrl2)
                           || !string.IsNullOrWhiteSpace(CurrentImageUrl3);
            if (!hasAnyImage)
            {
                yield return new ValidationResult(
                    "Please upload at least one auction image.",
                    new[] { nameof(AuctionImage1), nameof(AuctionImage2), nameof(AuctionImage3) }
                );
            }
        }
    }
}
