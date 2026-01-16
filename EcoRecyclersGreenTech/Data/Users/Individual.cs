using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoRecyclersGreenTech.Data.Users
{
    public class Individual
    {
        [Key]
        public int IndividualID { get; set; }

        [Required]
        public int UserID { get; set; }
        [ForeignKey(nameof(UserID))]
        public User User { get; set; } = null!;

        public string? Occupation { get; set; }
    }
}
