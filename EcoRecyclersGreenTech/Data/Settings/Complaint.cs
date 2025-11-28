using EcoRecyclersGreenTech.Data.Users;

namespace EcoRecyclersGreenTech.Data.Settings
{
    public class Complaint
    {
        public int ComplaintID { get; set; }
        public int From { get; set; }
        public User FromUser { get; set; } = null!;
        public int StatusCounter { get; set; } = 0;
        public string? Message { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
    }

}
