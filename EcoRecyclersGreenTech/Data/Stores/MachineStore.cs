using EcoRecyclersGreenTech.Data.Users;
using System.ComponentModel.DataAnnotations;

namespace EcoRecyclersGreenTech.Data.Stores
{
    public class MachineStore
    {
        [Key]
        public int MachineID { get; set; }
        public int SellerID { get; set; }
        public User Seller { get; set; } = null!;
        public string? MachineType { get; set; }
        public string? MachineImgURL { get; set; }
        public int Quantity { get; set; }
        public DateTime ManufactureDate { get; set; }
        public string? Description { get; set; }
        public string? Condition { get; set; }
    }

}
