using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoRecyclersGreenTech.Data.Users
{
    public class UserType
    {
        [Key]
        public int TypeID { get; set; }

        public int? RealTypeID { get; set; }

        public string TypeName { get; set; } = null!;

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
