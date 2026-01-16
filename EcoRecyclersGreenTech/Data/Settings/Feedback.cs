using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcoRecyclersGreenTech.Data.Users;

namespace EcoRecyclersGreenTech.Data.Settings
{
    public class Feedback
    {
        [Key]
        public int FeedbackID { get; set; }

        // FK
        public int From { get; set; }
        public int To { get; set; }

        // Navigation
        [ForeignKey(nameof(From))]
        public User FromUser { get; set; } = null!;

        [ForeignKey(nameof(To))]
        public User ToUser { get; set; } = null!;

        public string? Message { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
    }
}
