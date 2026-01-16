using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcoRecyclersGreenTech.Data.Stores;
using EcoRecyclersGreenTech.Data.Users;

namespace EcoRecyclersGreenTech.Data.Orders
{
    public enum JobOrderStatus
    {
        Pending,
        Confirmed,
        Scheduled,
        InProgress,
        Completed,
        Cancelled
    }

    public class JobOrder
    {
        [Key]
        public int JobOrderID { get; set; }

        [Required]
        public int JobStoreID { get; set; }

        [ForeignKey(nameof(JobStoreID))]
        public JobStore JobStore { get; set; } = null!;

        [Required]
        public int UserID { get; set; }

        [ForeignKey(nameof(UserID))]
        public User User { get; set; } = null!;

        [Required]
        public JobOrderStatus Status { get; set; } = JobOrderStatus.Pending;

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    }
}