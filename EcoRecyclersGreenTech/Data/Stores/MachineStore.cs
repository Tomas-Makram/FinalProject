using EcoRecyclersGreenTech.Data.Users;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using static EcoRecyclersGreenTech.Data.Stores.EnumsProductStatus;

namespace EcoRecyclersGreenTech.Data.Stores
{
    public enum MachineCondition
    {
        New = 1,
        LikeNew = 2,
        Used = 3,
        UsedFrequently = 4,
        AlmostBroken = 5,
        Disabled =6
    }

    public class MachineStore
    {
        [Key]
        public int MachineID { get; set; }
        public int SellerID { get; set; }
        public User Seller { get; set; } = null!;
        public string? MachineType { get; set; }
        public string? MachineImgURL1 { get; set; }
        public string? MachineImgURL2 { get; set; }
        public string? MachineImgURL3 { get; set; }
        public int Quantity { get; set; }
        public string? Description { get; set; }
        public MachineCondition? Condition { get; set; } = MachineCondition.Used;

        [Precision(18, 2)]
        public decimal Price { get; set; }

        public ProductStatus Status { get; set; } = ProductStatus.Available;
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public DateTime ManufactureDate { get; set; }
        public int? WarrantyMonths { get; set; }

        [Precision(18, 2)]
        public decimal? MinOrderQuantity { get; set; }
    }
}
