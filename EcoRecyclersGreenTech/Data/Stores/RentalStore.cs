using EcoRecyclersGreenTech.Data.Users;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EcoRecyclersGreenTech.Data.Stores
{
    public class RentalStore
    {
        [Key]
        public int RentalID { get; set; }
        public int OwnerID { get; set; }
        public User Owner { get; set; } = null!;
        public string? Location { get; set; }
        public double Area { get; set; }
        [Precision(18, 2)]
        public decimal PricePerMonth { get; set; }
        public string? Description { get; set; }
        public DateTime AvailableFrom { get; set; }
        public string? Condition { get; set; }
    }

}
