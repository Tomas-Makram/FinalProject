using EcoRecyclersGreenTech.Data.Stores;
using EcoRecyclersGreenTech.Data.Users;

namespace EcoRecyclersGreenTech.Data.Orders
{
    public class JobOrder
    {
        public int JobOrderID { get; set; }
        public int JobStoreID { get; set; }
        public JobStore JobStore { get; set; } = null!;
        public int UserID { get; set; }
        public User User { get; set; } = null!;
        public string Status { get; set; } = "Pending";
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public DateTime? Meetingdate { get; set; }
        public string? PickupLocation { get; set; }
    }
}
