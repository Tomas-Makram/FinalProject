namespace EcoRecyclersGreenTech.Models.Store
{
    public class StoreCardModel
    {
        public int Id { get; set; }
        public string Type { get; set; } = "";

        public string Name { get; set; } = "";
        public string? Description { get; set; }

        public decimal? Price { get; set; }
        public string Currency { get; set; } = "EGP";

        public string SellerName { get; set; } = "";
        public string? Location { get; set; }

        public DateTime CreatedAt { get; set; }

        public string DetailController { get; set; } = "CraftsMan";
        public string DetailAction { get; set; } = "Index";
    }
}
