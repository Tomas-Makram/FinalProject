using EcoRecyclersGreenTech.Data.Users;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using static EcoRecyclersGreenTech.Data.Stores.EnumsProductStatus;

namespace EcoRecyclersGreenTech.Data.Stores
{

    public enum TypeEmployment
    {
        [Display(Name = "Full-time")]
        FullTime = 1,

        [Display(Name = "Part-time")]
        PartTime = 2,

        [Display(Name = "Contract")]
        Contract = 3,

        [Display(Name = "Temporary")]
        Temporary = 4,

        [Display(Name = "Internship")]
        Internship = 5,

        [Display(Name = "Freelance")]
        Freelance = 6
    }

    public class JobStore
    {
        [Key]
        public int JobID { get; set; }

        public int PostedBy { get; set; }

        [ForeignKey(nameof(PostedBy))]
        public User User { get; set; } = null!;

        public string? JobType { get; set; }
        public int WorkHours { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal? Latitude { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal? Longitude { get; set; }

        [Required(ErrorMessage = "Location is required")]
        [StringLength(250, ErrorMessage = "Location cannot exceed 250 characters.")]
        [MaxLength(255)]
        public string? Location { get; set; }

        [Precision(18, 2)]
        public decimal Salary { get; set; }

        public string? Description { get; set; }

        public ProductStatus Status { get; set; } = ProductStatus.Available;

        public DateTime? ExpiryDate { get; set; }
        public string? RequiredSkills { get; set; }
        public string? ExperienceLevel { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public TypeEmployment? EmploymentType { get; set; } = TypeEmployment.FullTime;

    }
}