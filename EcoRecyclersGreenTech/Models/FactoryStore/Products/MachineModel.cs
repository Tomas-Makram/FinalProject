using EcoRecyclersGreenTech.Data.Stores;
using static EcoRecyclersGreenTech.Data.Stores.EnumsProductStatus;
using System.ComponentModel.DataAnnotations;

namespace EcoRecyclersGreenTech.Models.FactoryStore.Products
{
    public class MachineModel : IValidatableObject
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

        public ProductStatus Status { get; set; }

        public decimal? MinOrderQuantity { get; set; }

        [Required(ErrorMessage = "Manufacture date is required")]
        public DateTime ManufactureDate { get; set; }

        [Required(ErrorMessage = "Condition is required")]
        public MachineCondition? Condition { get; set; }

        public string? Brand { get; set; }
        public string? Model { get; set; }
        public int? WarrantyMonths { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (MinOrderQuantity.HasValue && MinOrderQuantity.Value < 0)
                yield return new ValidationResult("Minimum order quantity cannot be negative.", new[] { nameof(MinOrderQuantity) });

            if (MinOrderQuantity.HasValue && MinOrderQuantity.Value > Quantity)
                yield return new ValidationResult("Minimum order quantity cannot be greater than available quantity.", new[] { nameof(MinOrderQuantity), nameof(Quantity) });
        }
    }
}
