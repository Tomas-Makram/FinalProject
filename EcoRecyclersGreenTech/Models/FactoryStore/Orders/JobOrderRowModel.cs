namespace EcoRecyclersGreenTech.Models.FactoryStore.Orders
{
    public class JobOrderRowModel
    {
        public int JobOrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public string? OrderStatus { get; set; }

        public int IndividualUserID { get; set; }

        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public bool IsVerified { get; set; }
        public string? UserProfileImgUrl { get; set; }

        public string? UserTypeName { get; set; }
    }
}
