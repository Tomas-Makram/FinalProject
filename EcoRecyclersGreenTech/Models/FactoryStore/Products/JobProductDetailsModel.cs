using static EcoRecyclersGreenTech.Data.Stores.EnumsProductStatus;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EcoRecyclersGreenTech.Models.FactoryStore.Products
{
    public class JobProductDetailsModel : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Job type is required")]
        [StringLength(150, ErrorMessage = "Job type cannot exceed 150 characters.")]
        public string? JobType { get; set; }

        [Required(ErrorMessage = "Work hours are required")]
        [Range(1, 24, ErrorMessage = "Work hours must be between 1 and 24")]
        public int WorkHours { get; set; }

        // GPS Coordinates
        [Column(TypeName = "decimal(9,6)")]
        public decimal? Latitude { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal? Longitude { get; set; }

        [Required(ErrorMessage = "Location is required")]
        [StringLength(250, ErrorMessage = "Location cannot exceed 250 characters.")]
        [MaxLength(255)]
        public string? Location { get; set; }

        [Required(ErrorMessage = "Salary is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Salary must be greater than 0")]
        public decimal Salary { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
        public string? Description { get; set; }

        public ProductStatus Status { get; set; } = ProductStatus.Available;

        public DateTime? ExpiryDate { get; set; }

        public string? RequiredSkills { get; set; }
        public string? ExperienceLevel { get; set; }

        [StringLength(50, ErrorMessage = "Employment type cannot exceed 50 characters.")]
        public string? EmploymentType { get; set; } = "Full-time";

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ExpiryDate.HasValue && ExpiryDate.Value.Date < DateTime.Today)
            {
                yield return new ValidationResult(
                    "Expiry date cannot be in the past.",
                    new[] { nameof(ExpiryDate) }
                );
            }

            var hasLat = Latitude.HasValue;
            var hasLng = Longitude.HasValue;

            if (hasLat ^ hasLng)
            {
                yield return new ValidationResult(
                    "Please provide both latitude and longitude.",
                    new[] { nameof(Latitude), nameof(Longitude) }
                );
            }

            if (Latitude.HasValue && (Latitude.Value < -90m || Latitude.Value > 90m))
            {
                yield return new ValidationResult(
                    "Latitude must be between -90 and 90.",
                    new[] { nameof(Latitude) }
                );
            }

            if (Longitude.HasValue && (Longitude.Value < -180m || Longitude.Value > 180m))
            {
                yield return new ValidationResult(
                    "Longitude must be between -180 and 180.",
                    new[] { nameof(Longitude) }
                );
            }

            if (!string.IsNullOrWhiteSpace(RequiredSkills))
            {
                var skills = RequiredSkills
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();

                if (skills.Count == 0)
                {
                    yield return new ValidationResult(
                        "Required skills format is invalid. Please provide comma-separated skills.",
                        new[] { nameof(RequiredSkills) }
                    );
                }

                if (skills.Count > 30)
                {
                    yield return new ValidationResult(
                        "Too many skills. Please provide up to 30 skills.",
                        new[] { nameof(RequiredSkills) }
                    );
                }
            }

            if (!string.IsNullOrWhiteSpace(EmploymentType))
            {
                var allowed = new[] { "Full-time", "Part-time", "Contract", "Temporary", "Internship", "Freelance" };
                if (!allowed.Contains(EmploymentType.Trim(), StringComparer.OrdinalIgnoreCase))
                {
                    yield return new ValidationResult(
                        "Employment type is invalid. Allowed: Full-time, Part-time, Contract, Temporary, Internship, Freelance.",
                        new[] { nameof(EmploymentType) }
                    );
                }
            }
        }
    }
}
