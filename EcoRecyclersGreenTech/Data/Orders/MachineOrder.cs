using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcoRecyclersGreenTech.Data.Stores;
using EcoRecyclersGreenTech.Data.Users;
using Microsoft.EntityFrameworkCore;

namespace EcoRecyclersGreenTech.Data.Orders
{
    public class MachineOrder
    {
        [Key]
        public int MachineOrderID { get; set; }

        [Required]
        public int MachineStoreID { get; set; }
        [ForeignKey(nameof(MachineStoreID))]
        public MachineStore MachineStore { get; set; } = null!;

        [Required]
        public int BuyerID { get; set; }
        [ForeignKey(nameof(BuyerID))]
        public User Buyer { get; set; } = null!;

        [Required]
        public int Quantity { get; set; }

        [Required]
        public EnumsOrderStatus Status { get; set; } = EnumsOrderStatus.Pending;
        
        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Precision(18, 2)]
        public decimal UnitPrice { get; set; }

        [Precision(18, 2)]
        public decimal TotalPrice { get; set; }

        [Precision(18, 2)]
        public decimal DepositPaid { get; set; }

        public DateTime CancelUntil { get; set; }   // OrderDate + CancelWindowDays
        public DateTime ExpectedArrivalDate { get; set; } // OrderDate + DeliveryDays

        // GPS Coordinates
        [Column(TypeName = "decimal(9,6)")]
        public decimal? Latitude { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal? Longitude { get; set; }

        [MaxLength(255)]
        public string? PickupLocation { get; set; }

    }
}