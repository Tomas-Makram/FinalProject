namespace EcoRecyclersGreenTech.Models.Store
{
    public class StoreIndexModel
    {
        public string? Query { get; set; }
        public string? Type { get; set; }   // Material / Machine / Rental / Auction / Job

        public List<StoreCardModel> Stores { get; set; } = new();
    }
}
