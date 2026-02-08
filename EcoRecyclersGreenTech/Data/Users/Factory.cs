using Microsoft.EntityFrameworkCore;
using Stripe;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoRecyclersGreenTech.Data.Users
{
    public class Factory
    {
        [Key]
        public int FactoryID { get; set; }

        [Required]
        public int UserID { get; set; }
        [ForeignKey(nameof(UserID))]
        public User User { get; set; } = null!;

        public string FactoryName { get; set; } = null!;

        // Img Factory
        public string? FactoryImgURL1 { get; set; }
        public string? FactoryImgURL2 { get; set; }
        public string? FactoryImgURL3 { get; set; }

        // Type And Description
        public string? FactoryType { get; set; }
        public string? Description { get; set; }

        // Wallet and Parentage
        [Precision(18, 2)]
        public decimal? TotalBalanceOrderWaiting { get; set; } = 0;

        [Precision(18, 2)]
        public decimal? TotalBalancePercentageRequests { get; set; } = 0;
    }
}
