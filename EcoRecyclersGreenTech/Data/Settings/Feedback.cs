using EcoRecyclersGreenTech.Data.Users;

namespace EcoRecyclersGreenTech.Data.Settings
{
    public class Feedback
    {
        public int FeedbackID { get; set; }

        public int To { get; set; }
        public User ToUser { get; set; } = null!;

        public int From { get; set; }
        public User FromUser { get; set; } = null!;

        public string? Message { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
    }

}
