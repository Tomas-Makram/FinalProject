using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoRecyclersGreenTech.Data.Users
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        public string? UserProfileImgURL { get; set; } = null;
        public string? FullName { get; set; }

        // Encrypted
        public string? Email { get; set; }
        public string? phoneNumber { get; set; }

        // Hash for uniqueness
        [Required]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{8,}$",
            ErrorMessage = "Password must include: upper, lower, digit, 8+ length")]
        public string? HashPassword { get; set; }

        public string? HashEmail { get; set; }
        public string? HashPhoneNumber { get; set; }


        // GPS Coordinates
        [Column(TypeName = "decimal(9,6)")]
        public decimal? Latitude { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal? Longitude { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        // Foreign Key
        public int UserTypeID { get; set; }
        public UserType UserType { get; set; } = null!;

        // User Types
        public Individual? IndividualProfile { get; set; }
        public Factory? FactoryProfile { get; set; }
        public Craftsman? CraftsmanProfile { get; set; }
        public Admin? AdminProfile { get; set; }

        public DateTime JoinDate { get; set; } = DateTime.UtcNow;

        // Account Verification OTP
        public bool Verified { get; set; } = false;

        [Required]
        public string OtpHash { get; set; } = ""; // Code OTP

        public DateTime? OtpExpiresAt { get; set; } // Expire Time
        public int OtpAttempts { get; set; } = 0; // Incorrect Number Input OTP
        public DateTime? OtpLastSentAt { get; set; } // Last Time Send OTP
        public int OtpRequestsCount { get; set; } = 0; // Number of times the account verification OTP

        // Reset Password OTP
        public string? PasswordResetOtpHash { get; set; } // Code OTP
        public DateTime? PasswordResetOtpExpiresAt { get; set; } // Expire Time
        public int PasswordResetOtpAttempts { get; set; } = 0; // Incorrect Number Input OTP
        public DateTime? LastMailSentResetPasswordAt { get; set; } // Last Time Send OTP
        public int PasswordOtpResetCount { get; set; } = 0; // Number of times the account verification OTP

        // Global Mail Limits
        public int MailActionsCount { get; set; } = 0; // Total OTP Mail Sending
        public DateTime MailActionsResetAt { get; set; } = DateTime.UtcNow; // First Time Send Mail
        public DateTime? MailBlockedUntil { get; set; } // Time Block Account

        // Per-flow window + verify blocking
        public DateTime ValidationOtpWindowResetAt { get; set; } = DateTime.UtcNow; // The countdown window for sending the OTP
        public DateTime ResetOtpWindowResetAt { get; set; } = DateTime.UtcNow; // The countdown window for sending OTP password reset

        // Block verification attempts
        public DateTime? OtpVerifyBlockedUntil { get; set; } // Time of expiry of the temporary ban on attempts to enter OTP
        public DateTime? ResetOtpVerifyBlockedUntil { get; set; } // Time of expiry of the temporary ban on attempts to enter OTP password reset

        // Account security
        public bool Blocked { get; set; } = false;
        public int FailedLoginAttempts { get; set; } = 0;

        // Accounting Paying
        public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
        public Wallet? Wallet { get; set; }
    }
}
