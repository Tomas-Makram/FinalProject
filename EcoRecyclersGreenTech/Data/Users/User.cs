using System.ComponentModel.DataAnnotations;

namespace EcoRecyclersGreenTech.Data.Users
{
    public class User
    {
        [Key]
        public int UserID { get; set; }
        public string? UserProfieImgURL { get; set; } = null;
        public string? FullName { get; set; }
        [EmailAddress]
        public string? Email { get; set; }
        public string? phoneNumber { get; set; }
        [Required]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{8,}$",
        ErrorMessage = "Password must include: upper, lower, digit, 8+ length")]
        public string? HashPassword { get; set; }
        public string? Locaton { get; set; }
        //Foren Key
        public int UserTypeID { get; set; }
        public UserType UserType { get; set; } = null!;
        public DateTime JoinDate { get; set; } = DateTime.Now;
        public bool Verified { get; set; } = false;
        public bool Blocked { get; set; } = false;

        //To Save from Brute Force attacks
        public int FailedLoginAttempts { get; set; } = 0;
    }
}
