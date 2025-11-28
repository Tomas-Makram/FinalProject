using System.ComponentModel.DataAnnotations;

namespace EcoRecyclersGreenTech.Data.Users
{
    public class User
    {
        [Key]
        public int UserID { get; set; }
        public string? UserProfieImgURL { get; set; } = null;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? phoneNumber { get; set; }
        public string? Password { get; set; }
        public string? Locaton { get; set; }
        //Foren Key
        public int UserTypeID { get; set; }
        public UserType UserType { get; set; } = null!; // Navigation property
        public DateTime JoinDate { get; set; } = DateTime.Now;
        public bool Verified { get; set; } = false;
        public bool Blocked { get; set; } = false;
    }
}
