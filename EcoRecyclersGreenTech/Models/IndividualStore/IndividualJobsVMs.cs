using System.ComponentModel.DataAnnotations;

namespace EcoRecyclersGreenTech.Models.IndividualStore
{
    public class IndividualJobsFilterVM
    {
        [Display(Name = "Search")]
        public string? Q { get; set; }

        [Display(Name = "Min Salary")]
        public decimal? MinSalary { get; set; }

        [Display(Name = "Max Salary")]
        public decimal? MaxSalary { get; set; }

        [Display(Name = "Experience Level")]
        public string? ExperienceLevel { get; set; }

        [Display(Name = "Location")]
        public string? Location { get; set; }

        [Display(Name = "Max Distance (km)")]
        public decimal? MaxKm { get; set; }

        public string? SortBy { get; set; } = "newest";
        public string? SortDir { get; set; } = "desc";

        // ✅ checkbox
        public bool UseSmartOccupationFilter { get; set; } = false;

        // ✅ optional tuning
        public double SmartThreshold { get; set; } = 0.18;
    }

    public class IndividualJobCardVM
    {
        public int JobID { get; set; }
        public string? JobType { get; set; }
        public decimal? Salary { get; set; }
        public int? WorkHours { get; set; }

        public string? Location { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public string? ExperienceLevel { get; set; }
        public string? EmploymentType { get; set; }
        public string? RequiredSkills { get; set; }
        public string? Description { get; set; }

        public int FactoryUserID { get; set; }
        public string? FactoryName { get; set; }
        public bool FactoryVerified { get; set; }
        public string? FactoryProfileImgUrl { get; set; }

        public double? MatchScore { get; set; }
        public double? DistanceKm { get; set; }
    }

    public class IndividualJobDetailsVM
    {
        public int JobID { get; set; }
        public string? JobType { get; set; }
        public decimal? Salary { get; set; }
        public int? WorkHours { get; set; }

        public string? Location { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public string? Description { get; set; }
        public string? ExperienceLevel { get; set; }
        public string? EmploymentType { get; set; }
        public string? RequiredSkills { get; set; }
        public string? Status { get; set; }

        public int FactoryUserID { get; set; }
        public string? FactoryName { get; set; }
        public bool FactoryVerified { get; set; }

        public string? FactoryAddress { get; set; }
        public string? FactoryPhone { get; set; }
        public string? FactoryEmail { get; set; }

        public string? FactoryProfileImgUrl { get; set; }
        public string? FactoryType { get; set; }
        public string? FactoryDescription { get; set; }

        public bool IsJoined { get; set; }
        public int? MyOrderId { get; set; }

        public List<string> FactoryImageUrls { get; set; } = new();

        public int JoinedCount { get; set; }
        public string? MyOrderStatus { get; set; }

    }

    public class IndividualJobsIndexVM
    {
        public IndividualJobsFilterVM Filter { get; set; } = new();
        public string? UserAddress { get; set; }
        public decimal? UserLat { get; set; }
        public decimal? UserLng { get; set; }

        public string? MyOccupation { get; set; }

        public List<IndividualJobCardVM> Jobs { get; set; } = new();

        public int ExpiringSoonCount { get; set; }
        public List<string> ExpiringSoonTitles { get; set; } = new();
    }

    public class MyJoinedJobVM
    {
        public int JobOrderID { get; set; }
        public int JobID { get; set; }
        public DateTime OrderDate { get; set; }

        public string? JobType { get; set; }
        public decimal? Salary { get; set; }
        public string? Location { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public string? FactoryName { get; set; }
        public bool FactoryVerified { get; set; }

        public bool IsDeletedByFactory { get; set; }
        public string? DeletedNote { get; set; }

        public bool IsExpired { get; set; }
        public int? DaysLeft { get; set; }

        public string? OrderStatus { get; set; }
    }
}