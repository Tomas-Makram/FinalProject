namespace EcoRecyclersGreenTech.Models.FactoryStore
{
    public class SearchFilterModel
    {
        public string? ProductType { get; set; }

        // Sorting
        public string? SortBy { get; set; }     // "newest", "salary", "hours", "distance"
        public string? SortDir { get; set; }    // "asc" or "desc"

        // Location input from user
        public decimal? UserLat { get; set; }
        public decimal? UserLng { get; set; }
        public decimal? MaxDistanceKm { get; set; }

        public string? Keyword { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Location { get; set; }
        public string? Status { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? ExperienceLevel { get; set; }
    }
}
