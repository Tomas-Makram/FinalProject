using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoRecyclersGreenTech.Models.Home
{
    public class SettingModel
    {
        // Display sections
        public ProfileSection Profile { get; set; } = new();
        public AccountSection Account { get; set; } = new();
        public VerificationSection Verification { get; set; } = new();
        public UserTypeSection UserTypeDetails { get; set; } = new();

        // Forms
        public BasicInfoForm BasicInfo { get; set; } = new();
        public ProfileImageForm ProfileImage { get; set; } = new();
        public TypeSpecificForm TypeSpecific { get; set; } = new();

        // Factory images
        public FactoryImagesForm FactoryImages { get; set; } = new();

        // OTP & Password
        public VerifyOtpForm VerifyOtp { get; set; } = new();
        public ChangePasswordModel PasswordChange { get; set; } = new();
    }

    public class ProfileSection
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }

        // GPS Coordinates
        [Column(TypeName = "decimal(9,6)")]
        public decimal? Latitude { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal? Longitude { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        public string? ProfileImageUrl { get; set; }
        public DateTime JoinDate { get; set; }
    }

    public class AccountSection
    {
        public string? UserTypeName { get; set; }
        public bool Verified { get; set; }
        public bool Blocked { get; set; }
    }

    public class VerificationSection
    {
        public bool HasActiveOtp { get; set; }
        public int RemainingAttempts { get; set; }
        public TimeSpan OtpExpiryRemaining { get; set; }

        public bool CanResendOtp { get; set; }
        public TimeSpan ResendCooldownRemaining { get; set; }

        public DateTime? VerifyBlockedUntil { get; set; }

        public bool MailBlocked { get; set; }
        public DateTime? MailBlockedUntil { get; set; }

        public bool OpenVerifyModal { get; set; }
    }

    public class UserTypeSection
    {
        public string? TypeName { get; set; }

        // Admin
        public string? AdminType { get; set; }

        // Craftsman
        public string? SkillType { get; set; }
        public int? ExperienceYears { get; set; }

        // Individual
        public string? Occupation { get; set; }

        // Factory
        public string? FactoryName { get; set; }
        public string? FactoryType { get; set; }
        public string? Description { get; set; }

        // Images
        public List<string> FactoryImages { get; set; } = new();
        public int CurrentFactoryImagesCount { get; set; }
        public int MaxFactoryImages { get; set; } = 3;
    }

    // ---------------- Forms ----------------

    public class BasicInfoForm
    {
        [StringLength(100)]
        public string? FullName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }
    }

    public class ProfileImageForm
    {
        public IFormFile? ProfileImageFile { get; set; }
        public bool DeleteProfileImage { get; set; }

    }

    public class TypeSpecificForm
    {
        // Admin
        public string? AdminType { get; set; }

        // Craftsman
        public string? SkillType { get; set; }

        [Range(0, 100)]
        public int? ExperienceYears { get; set; }

        // Individual
        public string? Occupation { get; set; }

        // Factory
        public string? FactoryName { get; set; }
        public string? FactoryType { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }
    }

    public class FactoryImagesForm
    {
        // Replace/Add per slot (always 3)
        public IFormFile? Slot1File { get; set; }
        public IFormFile? Slot2File { get; set; }
        public IFormFile? Slot3File { get; set; }

        // Delete per slot
        public bool DeleteSlot1 { get; set; }
        public bool DeleteSlot2 { get; set; }
        public bool DeleteSlot3 { get; set; }
    }

    public class VerifyOtpForm
    {
        [Required]
        [StringLength(6, MinimumLength = 6)]
        [RegularExpression(@"^\d{6}$")]
        public string? OtpCode { get; set; }
    }

    public class ChangePasswordModel
    {
        [Required, DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }

        [Required, DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{8,}$",
            ErrorMessage = "Password must include uppercase, lowercase, digit, and be at least 8 characters.")]
        public string? NewPassword { get; set; }

        [Required, DataType(DataType.Password)]
        [Compare("NewPassword")]
        public string? ConfirmNewPassword { get; set; }
    }
}