using EcoRecyclersGreenTech.Data.Users;
using EcoRecyclersGreenTech.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace EcoRecyclersGreenTech.Services
{
    public interface IOtpService
    {
        Task<bool> VerifyOtpAsync(int userId, string otpCode);
        Task<bool> VerifyOTPPasswordAsync(int userId, string otpCode);

        Task<string> ResendOtpAsync(int userId, bool sent = false);
        Task<string> SendOTPResetPassword(int userId);

        Task<bool> CanSendMailOTPOrReset(int userId);

        int GetOtpExpiryMinutes();
        int GetMaxOtpAttempts();
        int GetResendCooldownMinutes();
    }

    public class OtpService : IOtpService
    {
        private readonly DBContext _db;
        private readonly DataHasher _passwordHasher;
        private readonly ILogger<OtpService> _logger;

        // Core OTP policy
        private const int OTPExpiryMinutes = 10; // Expire OTP Time (Minutes)
        private const int maxOTPAttempts = 3; // Max Time Incorrect OTP
        private const int resendCoolDownMinutes = 2; // Can Send OTP after this time End (start by last send otp (Minutes))

        // Global mail policy
        private const int mailWindowDays = 30; // Day in Month
        private const int maxMailsInWindow = 20; // Max mail send per mailWindowDays

        // Per-flow send limits within their own windows
        private const int maxValidationSendsPerWindow = 10; // Can Send OTP in mailWindowDays
        private const int maxResetSendsPerWindow = 10; // Can Send OTP Reset in mailWindowDays

        // Verify blocking
        private const int verifyBlockMinutes = 15; // Block Account after Incorrect OTP (Minutes)

        public OtpService(DBContext db, DataHasher passwordHasher, ILogger<OtpService> logger)
        {
            _db = db;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public int GetOtpExpiryMinutes() => OTPExpiryMinutes;
        public int GetMaxOtpAttempts() => maxOTPAttempts;
        public int GetResendCooldownMinutes() => resendCoolDownMinutes;

        
        // Utilities
        private static DateTime UtcNow() => DateTime.UtcNow;

        private string GenerateOtpCode()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var number = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 1_000_000;
            return number.ToString("D6");
        }

        private bool IsActiveOtp(string? hash, DateTime? expiresAt)
        {
            if (string.IsNullOrWhiteSpace(hash)) return false;
            if (!expiresAt.HasValue) return false;
            return expiresAt.Value > UtcNow();
        }

        private int RemainingAttempts(int used) => Math.Max(0, maxOTPAttempts - used);

        private async Task EnsureGlobalMailWindowAsync(User user)
        {
            var now = UtcNow();

            // If window elapsed => reset counters
            if (user.MailActionsResetAt <= now.AddDays(-mailWindowDays))
            {
                user.MailActionsCount = 0;
                user.MailActionsResetAt = now;
                user.MailBlockedUntil = null;

                await _db.SaveChangesAsync();
            }
        }

        public async Task<bool> CanSendMailOTPOrReset(int userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return false;

            var now = UtcNow();

            // If globally blocked
            if (user.MailBlockedUntil.HasValue && user.MailBlockedUntil.Value > now)
                return false;

            await EnsureGlobalMailWindowAsync(user);

            // If reached global max => block for window duration
            if (user.MailActionsCount >= maxMailsInWindow)
            {
                user.MailBlockedUntil = now.AddDays(mailWindowDays);
                await _db.SaveChangesAsync();
                return false;
            }

            return true;
        }

        private async Task EnsureValidationWindowAsync(User user)
        {
            var now = UtcNow();

            if (user.ValidationOtpWindowResetAt <= now.AddDays(-mailWindowDays))
            {
                user.OtpRequestsCount = 0;
                user.ValidationOtpWindowResetAt = now;

                // unblock verify when new window starts
                user.OtpVerifyBlockedUntil = null;

                await _db.SaveChangesAsync();
            }
        }

        private async Task EnsureResetWindowAsync(User user)
        {
            var now = UtcNow();

            if (user.ResetOtpWindowResetAt <= now.AddDays(-mailWindowDays))
            {
                user.PasswordOtpResetCount = 0;
                user.ResetOtpWindowResetAt = now;

                // unblock verify when new window starts
                user.ResetOtpVerifyBlockedUntil = null;

                await _db.SaveChangesAsync();
            }
        }

        
        // Validation OTP (Account Verification)
        private async Task<bool> CanSendValidationOtpAsync(User user, bool bypassCooldown)
        {
            var now = UtcNow();

            if (user.Blocked) return false; // blocked accounts can not do flows
            if (user.Verified) return false; // already verified

            // reset per-flow window counters if elapsed
            await EnsureValidationWindowAsync(user);

            // global mail cap
            if (!await CanSendMailOTPOrReset(user.UserId))
                return false;

            // per-flow send cap
            if (user.OtpRequestsCount >= maxValidationSendsPerWindow)
                return false;

            if (!bypassCooldown)
            {
                if (user.OtpLastSentAt.HasValue &&
                    now < user.OtpLastSentAt.Value.AddMinutes(resendCoolDownMinutes))
                    return false;
            }

            return true;
        }

        private async Task<string> CreateAndSaveValidationOtpAsync(User user)
        {
            var now = UtcNow();

            var otp = GenerateOtpCode();

            user.OtpHash = _passwordHasher.HashData(otp);
            user.OtpExpiresAt = now.AddMinutes(OTPExpiryMinutes);

            // reset attempts for fresh OTP
            user.OtpAttempts = 0;

            // sending tracking
            user.OtpLastSentAt = now;
            user.OtpRequestsCount++;

            // global mail tracking
            user.MailActionsCount++;

            await _db.SaveChangesAsync();
            return otp;
        }

        public async Task<string> ResendOtpAsync(int userId, bool sent = false)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return "";

            if (!await CanSendValidationOtpAsync(user, bypassCooldown: sent))
                return "";

            return await CreateAndSaveValidationOtpAsync(user);
        }

        public async Task<bool> VerifyOtpAsync(int userId, string otpCode)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return false;

            var now = UtcNow();

            if (user.Blocked) return false;
            if (user.Verified) return false;

            // Verify-block (anti brute force on verify)
            if (user.OtpVerifyBlockedUntil.HasValue && user.OtpVerifyBlockedUntil.Value > now)
                return false;

            // must be active
            if (!IsActiveOtp(user.OtpHash, user.OtpExpiresAt))
                return false;

            // attempts exceeded => invalidate OTP + block verify for some time
            if (RemainingAttempts(user.OtpAttempts) <= 0)
            {
                user.OtpHash = "";
                user.OtpExpiresAt = null;
                user.OtpVerifyBlockedUntil = now.AddMinutes(verifyBlockMinutes);
                await _db.SaveChangesAsync();
                return false;
            }

            var isValid = _passwordHasher.VerifyHashed(otpCode, user.OtpHash);

            if (isValid)
            {
                user.Verified = true;

                // invalidate OTP
                user.OtpHash = "";
                user.OtpExpiresAt = null;
                user.OtpAttempts = 0;

                // clear verify block
                user.OtpVerifyBlockedUntil = null;
            }
            else
            {
                user.OtpAttempts++;

                // if exceeded after increment => invalidate + temporary verify block
                if (RemainingAttempts(user.OtpAttempts) <= 0)
                {
                    user.OtpHash = "";
                    user.OtpExpiresAt = null;
                    user.OtpVerifyBlockedUntil = now.AddMinutes(verifyBlockMinutes);
                }
            }

            await _db.SaveChangesAsync();
            return isValid;
        }

        
        // Reset Password OTP
        private async Task<bool> CanSendResetOtpAsync(User user)
        {
            var now = UtcNow();

            if (user.Blocked) return false;

            // reset per-flow window counters if elapsed
            await EnsureResetWindowAsync(user);

            // global mail cap
            if (!await CanSendMailOTPOrReset(user.UserId))
                return false;

            // per-flow send cap
            if (user.PasswordOtpResetCount >= maxResetSendsPerWindow)
                return false;

            // cooldown (separate from validation)
            if (user.LastMailSentResetPasswordAt.HasValue &&
                now < user.LastMailSentResetPasswordAt.Value.AddMinutes(resendCoolDownMinutes))
                return false;

            return true;
        }

        public async Task<string> SendOTPResetPassword(int userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return "";

            if (!await CanSendResetOtpAsync(user))
                return "";

            var now = UtcNow();

            var otp = GenerateOtpCode();

            user.PasswordResetOtpHash = _passwordHasher.HashData(otp);
            user.PasswordResetOtpExpiresAt = now.AddMinutes(OTPExpiryMinutes);

            // reset attempts for fresh OTP
            user.PasswordResetOtpAttempts = 0;

            // sending tracking (ONLY sends)
            user.PasswordOtpResetCount++;

            // cooldown timestamp
            user.LastMailSentResetPasswordAt = now;

            // global mail tracking
            user.MailActionsCount++;

            await _db.SaveChangesAsync();
            return otp;
        }

        public async Task<bool> VerifyOTPPasswordAsync(int userId, string otpCode)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return false;

            var now = UtcNow();

            if (user.Blocked) return false;

            // Verify-block (anti brute force on verify)
            if (user.ResetOtpVerifyBlockedUntil.HasValue && user.ResetOtpVerifyBlockedUntil.Value > now)
                return false;

            // must be active
            if (!IsActiveOtp(user.PasswordResetOtpHash, user.PasswordResetOtpExpiresAt))
                return false;

            // attempts exceeded => invalidate OTP + block verify
            if (RemainingAttempts(user.PasswordResetOtpAttempts) <= 0)
            {
                user.PasswordResetOtpHash = null;
                user.PasswordResetOtpExpiresAt = null;
                user.ResetOtpVerifyBlockedUntil = now.AddMinutes(verifyBlockMinutes);
                await _db.SaveChangesAsync();
                return false;
            }

            var isValid = _passwordHasher.VerifyHashed(otpCode, user.PasswordResetOtpHash!);

            if (isValid)
            {
                user.PasswordResetOtpHash = null;
                user.PasswordResetOtpExpiresAt = null;
                user.PasswordResetOtpAttempts = 0;

                // clear block
                user.ResetOtpVerifyBlockedUntil = null;

                // reset per-flow send counter after success
                user.PasswordOtpResetCount = 0;
            }
            else
            {
                user.PasswordResetOtpAttempts++;

                if (RemainingAttempts(user.PasswordResetOtpAttempts) <= 0)
                {
                    user.PasswordResetOtpHash = null;
                    user.PasswordResetOtpExpiresAt = null;
                    user.ResetOtpVerifyBlockedUntil = now.AddMinutes(verifyBlockMinutes);
                }
            }

            await _db.SaveChangesAsync();
            return isValid;
        }
    }
}
