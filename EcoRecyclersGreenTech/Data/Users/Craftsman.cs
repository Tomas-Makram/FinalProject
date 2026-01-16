using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoRecyclersGreenTech.Data.Users
{
    public class Craftsman
    {
        [Key]
        public int CraftsmanID { get; set; }

        [Required]
        public int UserID { get; set; }
        [ForeignKey(nameof(UserID))]
        public User User { get; set; } = null!;

        public string? SkillType { get; set; }
        public int ExperienceYears { get; set; }
    }

}
