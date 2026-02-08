namespace EcoRecyclersGreenTech.Models.CraftsMan
{
    public class CraftsManStoreFilterModel
    {
        public string? Q { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? ExperienceLevel { get; set; }
        public string? Location { get; set; }

        public int? MaxKm { get; set; }
        public string SortBy { get; set; } = "newest"; // newest/price/distance
        public string SortDir { get; set; } = "desc";  // asc/desc

        // AI
        public bool UseSmartSkillFilter { get; set; }
        public double SmartThreshold { get; set; } = 0.15;
    }
}