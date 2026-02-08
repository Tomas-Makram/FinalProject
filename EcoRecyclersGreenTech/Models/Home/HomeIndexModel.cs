namespace EcoRecyclersGreenTech.Models.Home
{
    public class HomeIndexModel
    {
        public bool Logged { get; set; }
        public string Type { get; set; } = "";
        public string Email { get; set; } = "";

        public FactoryHomeModel? FactoryDashboard { get; set; }
    }
}
