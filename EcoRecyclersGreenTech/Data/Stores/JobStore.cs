using EcoRecyclersGreenTech.Data.Users;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EcoRecyclersGreenTech.Data.Stores
{
    public class JobStore
    {
        [Key]
        public int JobID { get; set; }
        public int PostedBy { get; set; }
        public User User { get; set; } = null!;
        public string? JobType { get; set; }
        public int WorkHours { get; set; }
        public string? Location { get; set; }
        [Precision(18, 2)]
        public decimal Salary { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

}
