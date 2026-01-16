using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Mail;
using System.Net;

namespace EcoRecyclersGreenTech.Services
{
    // Structure of Email Template
    public record MailSender
    {
        public string Email { get; init; }
        public string Subject { get; init; }
        public string Body { get; init; }

        public MailSender(string email, string subject, string body)
        {
            Email = email;
            Subject = subject;
            Body = body;
        }
    }

    public static class EmailTemplateConfig
    {
        // Basic Information
        public static string CompanyName => "EcoRecyclers GreenTech";
        public static string SupportEmail => "support@ecorecyclers.com";
        public static string SupportPhone => "+20 122 193 6850";
        public static string WebsiteUrl => "https://localhost:7214";
        public static string AppName => "EcoRecyclers";
        public static string SupportLocation => "Cairo , Assiut";

        // Social Media Links
        public static string FacebookUrl => "https://facebook.com/ecorecyclers";
        public static string TwitterUrl => "https://twitter.com/ecorecyclers";
        public static string InstagramUrl => "https://instagram.com/ecorecyclers";
        public static string LinkedInUrl => "https://linkedin.com/company/ecorecyclers";

        // Email Templates Constants
        public static class Templates
        {
            public static string OTPExpiryMinutes = "10";
            public static string OTPMaxAttempts = "3";
            public static string OTPResetExpiry = "24 hours";
            public static string ContactHours => "9 AM - 5 PM (GMT+2)";
        }
    }

    public interface IEmailTemplateService
    {
        // Email Sending
        Task SendEmailAsync(MailSender mail);

        // Email Templates
        MailSender CreateWelcomeEmail(string email, string? userName = null);
        MailSender CreateOtpVerificationEmail(string email, string otpCode, string? userName = null);
        MailSender CreateAccountVerifiedEmail(string email, string? userName = null);
        MailSender CreatePasswordResetOtpEmail(string email, string otpCode, string? userName = null);
        MailSender CreatePasswordChangedEmail(string email, string? userName = null);
        MailSender CreateNotificationEmail(string email, string subject, string message, string? userName = null);

        // Additional Email Types
        MailSender CreateOrderConfirmationEmail(string email, string orderId, string? userName = null);
        MailSender CreateNewsletterEmail(string email, string newsletterTitle, string content, string? userName = null);
        MailSender CreateSupportTicketEmail(string email, string ticketId, string? userName = null);
        MailSender CreateEventReminderEmail(string email, string eventName, DateTime eventDate, string? userName = null);
    }

    public class MailService : IEmailSender, IEmailTemplateService
    {
        private readonly IConfiguration _configuration;
        private readonly IOtpService _otpService;
        private readonly ILogger<MailService> _logger;

        public MailService(IConfiguration configuration, IOtpService otpService, ILogger<MailService> logger)
        {
            _configuration = configuration;
            _otpService = otpService;
            _logger = logger;
            EmailTemplateConfig.Templates.OTPExpiryMinutes = _otpService.GetOtpExpiryMinutes().ToString();
            EmailTemplateConfig.Templates.OTPMaxAttempts = _otpService.GetMaxOtpAttempts().ToString();
            EmailTemplateConfig.Templates.OTPResetExpiry = _otpService.GetResendCooldownMinutes().ToString();

        }

        // IEmailSender Implementation Interface
        public Task SendEmailAsync(string to, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(to))
                throw new ArgumentException("Recipient email cannot be null or empty", nameof(to));

            bool enableSsl = true;
            int port = 587;

            bool.TryParse(_configuration["EmailSettings:EnableSsl"], out enableSsl);
            int.TryParse(_configuration["EmailSettings:MailPort"], out port);

            var client = new SmtpClient(_configuration["EmailSettings:MailServer"], port)
            {
                EnableSsl = enableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(
                    _configuration["EmailSettings:SenderEmail"],
                    _configuration["EmailSettings:Password"]
                )
            };

            var message = new MailMessage(
                from: _configuration["EmailSettings:SenderEmail"]!,
                to: to,
                subject: subject,
                body: body
            )
            {
                IsBodyHtml = true
            };

            return client.SendMailAsync(message);
        }

        // Send Email
        public async Task SendEmailAsync(MailSender mail)
        {
            if (mail == null)
                throw new ArgumentNullException(nameof(mail));

            await SendEmailAsync(mail.Email, mail.Subject, mail.Body);
        }

        // Email Templates
        public MailSender CreateWelcomeEmail(string email, string? userName = null)
        {
            var subject = $"🌟 Welcome to {EmailTemplateConfig.AppName}!";

            var body = GenerateEmailTemplate(
                title: "Welcome Aboard!",
                greeting: $"Welcome {(string.IsNullOrEmpty(userName) ? "" : userName + ",")}",
                mainContent: @"
                    <p>We're thrilled to have you join the <strong>EcoRecyclers GreenTech</strong> community!</p>
                    
                    <div style='background: #e8f5e9; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                        <h4 style='color: #2e7d32; margin-top: 0;'>🚀 Get Started:</h4>
                        <ol>
                            <li>Complete your profile</li>
                            <li>Explore recycling projects</li>
                            <li>Connect with eco-conscious individuals</li>
                            <li>Start your green journey today!</li>
                        </ol>
                    </div>
                    
                    <p>Together, we can make a significant impact on our planet's future.</p>",
                buttonText: "Go to Dashboard",
                buttonLink: $"{EmailTemplateConfig.WebsiteUrl}/dashboard",
                themeColor: "#e74c3c"
            );

            return new MailSender(email, subject, body);
        }

        public MailSender CreateOtpVerificationEmail(string email, string otpCode, string? userName = null)
        {
            var subject = $"🎯 Verify Your {EmailTemplateConfig.AppName} Account";

            var body = GenerateEmailTemplate(
                title: "Account Verification",
                greeting: $"Hello {(string.IsNullOrEmpty(userName) ? "" : userName + ",")}",
                mainContent: $@"
                    <p>Welcome to <strong>{EmailTemplateConfig.CompanyName}</strong>! We're excited to have you on board.</p>
                    <p>To complete your registration and unlock all features, please verify your email address.</p>
                    
                    <div style='background: #ffffff; border: 2px dashed #2ecc71; padding: 25px; 
                         text-align: center; margin: 25px 0; border-radius: 8px;'>
                        <p style='margin-top: 0; color: #7f8c8d;'><strong>Your Verification Code:</strong></p>
                        <div style='font-size: 36px; font-weight: bold; letter-spacing: 10px; color: #2c3e50;
                             font-family: 'Courier New', monospace; margin: 15px 0;'>
                            {otpCode}
                        </div>
                        <p style='color: #7f8c8d;'>Enter this 6-digit code in the verification page</p>
                    </div>
                    
                    <div style='background: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0;'>
                        <p style='color: #856404; margin: 0;'>
                            <strong>⚠️ Important Information:</strong><br>
                            • Expires in: <strong>{_otpService.GetOtpExpiryMinutes()} minutes</strong><br>
                            • Attempts: <strong>{_otpService.GetMaxOtpAttempts()} attempts</strong><br>
                            • Do not share this code with anyone
                        </p>
                    </div>",
                buttonText: "Verify My Account",
                buttonLink: $"{EmailTemplateConfig.WebsiteUrl}/verify",
                themeColor: "#2ecc71",
                showImportantNote: true
            );

            return new MailSender(email, subject, body);
        }

        public MailSender CreateAccountVerifiedEmail(string email, string? userName = null)
        {
            var subject = $"🎉 {EmailTemplateConfig.AppName} Account Verified Successfully!";

            var body = GenerateEmailTemplate(
                title: "Account Verified",
                greeting: $"Congratulations {(string.IsNullOrEmpty(userName) ? "" : userName + "!")}",
                mainContent: @"
                    <p>Your account has been <strong>successfully verified</strong>! 🎉</p>
                    
                    <div style='background: #d4edda; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                        <h4 style='color: #155724; margin-top: 0;'>✅ What's Next?</h4>
                        <ul style='margin-bottom: 0;'>
                            <li>Full access to all features</li>
                            <li>Start creating recycling projects</li>
                            <li>Connect with the community</li>
                            <li>Earn rewards for green activities</li>
                        </ul>
                    </div>
                    
                    <p>You're now part of our mission to create a sustainable future!</p>",
                buttonText: "Explore Features",
                buttonLink: $"{EmailTemplateConfig.WebsiteUrl}/features",
                themeColor: "#27ae60",
                showCelebration: true
            );

            return new MailSender(email, subject, body);
        }

        public MailSender CreatePasswordResetOtpEmail(string email, string otpCode, string? userName = null)
        {
            var subject = $"🔐 Password Reset Code - {EmailTemplateConfig.AppName}";

            var body = GenerateEmailTemplate(
                title: "Reset Your Password",
                greeting: $"Hello {(string.IsNullOrEmpty(userName) ? "" : userName + ",")}",
                mainContent: $@"
            <p>We received a request to reset your password.</p>
            <p>Please use the verification code below to continue the password reset process:</p>

            <div style='background: #ffffff; border: 2px dashed #3498db; padding: 25px;
                 text-align: center; margin: 25px 0; border-radius: 8px;'>
                <p style='margin-top: 0; color: #7f8c8d;'>
                    <strong>Your Password Reset Code:</strong>
                </p>
                <div style='font-size: 36px; font-weight: bold; letter-spacing: 10px; 
                     color: #2c3e50; font-family: ""Courier New"", monospace; margin: 15px 0;'>
                    {otpCode}
                </div>
                <p style='color: #7f8c8d;'>Enter this code on the password reset page</p>
            </div>

            <div style='background: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0;'>
                <p style='color: #856404; margin: 0;'>
                    <strong>⚠️ Security Information:</strong><br>
                    • Expires in: <strong>{_otpService.GetOtpExpiryMinutes()} minutes</strong><br>
                    • Max attempts: <strong>{_otpService.GetMaxOtpAttempts()} attempts</strong><br>
                    • Do not share this code with anyone
                </p>
            </div>

            <p>If you did not request a password reset, you can safely ignore this email.</p>
        ",
                buttonText: "Enter Reset Code",
                buttonLink: $"{EmailTemplateConfig.WebsiteUrl}/reset-password",
                themeColor: "#3498db",
                showSecurityNote: true
            );

            return new MailSender(email, subject, body);
        }

        public MailSender CreatePasswordChangedEmail(string email, string? userName = null)
        {
            var subject = $"✅ {EmailTemplateConfig.AppName} Password Changed Successfully";

            var body = GenerateEmailTemplate(
                title: "Password Updated",
                greeting: $"Hello {(string.IsNullOrEmpty(userName) ? "" : userName + ",")}",
                mainContent: @"
                    <p>Your password has been <strong>successfully changed</strong>.</p>
                    
                    <div style='background: #e8f6f3; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                        <h4 style='color: #0d6efd; margin-top: 0;'>🔐 What this means:</h4>
                        <ul style='margin-bottom: 0;'>
                            <li>Your account is now secured with the new password</li>
                            <li>All active sessions remain logged in</li>
                            <li>If this wasn't you, contact support immediately</li>
                        </ul>
                    </div>
                    
                    <p>If you have any questions, please contact our support team.</p>",
                buttonText: "Visit Dashboard",
                buttonLink: $"{EmailTemplateConfig.WebsiteUrl}/dashboard",
                themeColor: "#9b59b6"
            );

            return new MailSender(email, subject, body);
        }

        public MailSender CreateNotificationEmail(string email, string subject, string message, string? userName = null)
        {
            var emailSubject = $"📢 {subject} - {EmailTemplateConfig.AppName}";

            var body = GenerateEmailTemplate(
                title: subject,
                greeting: $"Hello {(string.IsNullOrEmpty(userName) ? "" : userName + ",")}",
                mainContent: $@"
                    <div style='background: white; padding: 25px; border-radius: 8px; border: 1px solid #e0e0e0; margin: 20px 0;'>
                        {message}
                    </div>
                    
                    <p>This is an automated notification from {EmailTemplateConfig.CompanyName}.</p>",
                buttonText: "View Details",
                buttonLink: $"{EmailTemplateConfig.WebsiteUrl}/notifications",
                themeColor: "#f39c12",
                isNotification: true
            );

            return new MailSender(email, emailSubject, body);
        }

        // ========== Additional Email Types ==========

        public MailSender CreateOrderConfirmationEmail(string email, string orderId, string? userName = null)
        {
            var subject = $"✅ Order #{orderId} Confirmed - {EmailTemplateConfig.AppName}";

            var body = GenerateEmailTemplate(
                title: "Order Confirmation",
                greeting: $"Thank you {(string.IsNullOrEmpty(userName) ? "" : userName + ",")}",
                mainContent: $@"
                    <p>Your order <strong>#{orderId}</strong> has been confirmed and is being processed.</p>
                    
                    <div style='background: #e3f2fd; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                        <h4 style='color: #1565c0; margin-top: 0;'>📦 Order Details:</h4>
                        <p><strong>Order ID:</strong> {orderId}</p>
                        <p><strong>Status:</strong> Processing</p>
                        <p><strong>Estimated Delivery:</strong> 3-5 business days</p>
                    </div>
                    
                    <p>You'll receive another email when your order ships.</p>",
                buttonText: "Track Order",
                buttonLink: $"{EmailTemplateConfig.WebsiteUrl}/orders/{orderId}",
                themeColor: "#2196f3"
            );

            return new MailSender(email, subject, body);
        }

        public MailSender CreateNewsletterEmail(string email, string newsletterTitle, string content, string? userName = null)
        {
            var subject = $"📰 {newsletterTitle} - {EmailTemplateConfig.AppName} Newsletter";

            var body = GenerateEmailTemplate(
                title: newsletterTitle,
                greeting: $"Hello {(string.IsNullOrEmpty(userName) ? "" : userName + ",")}",
                mainContent: $@"
                    <p>Here's your latest update from {EmailTemplateConfig.CompanyName}!</p>
                    
                    <div style='background: #f5f5f5; padding: 25px; border-radius: 8px; margin: 20px 0;'>
                        {content}
                    </div>
                    
                    <p>Stay tuned for more updates and eco-friendly tips!</p>",
                buttonText: "Read More",
                buttonLink: $"{EmailTemplateConfig.WebsiteUrl}/newsletter",
                themeColor: "#4caf50",
                showSocialLinks: true
            );

            return new MailSender(email, subject, body);
        }

        public MailSender CreateSupportTicketEmail(string email, string ticketId, string? userName = null)
        {
            var subject = $"🛠️ Support Ticket #{ticketId} Created - {EmailTemplateConfig.AppName}";

            var body = GenerateEmailTemplate(
                title: "Support Ticket Created",
                greeting: $"Hello {(string.IsNullOrEmpty(userName) ? "" : userName + ",")}",
                mainContent: $@"
                    <p>Your support ticket <strong>#{ticketId}</strong> has been successfully created.</p>
                    
                    <div style='background: #fff3e0; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                        <h4 style='color: #ef6c00; margin-top: 0;'>🛠️ Ticket Information:</h4>
                        <p><strong>Ticket ID:</strong> {ticketId}</p>
                        <p><strong>Status:</strong> Open</p>
                        <p><strong>Expected Response:</strong> Within 24 hours</p>
                    </div>
                    
                    <p>Our support team will review your ticket and get back to you soon.</p>",
                buttonText: "View Ticket",
                buttonLink: $"{EmailTemplateConfig.WebsiteUrl}/support/ticket/{ticketId}",
                themeColor: "#ff9800"
            );

            return new MailSender(email, subject, body);
        }

        public MailSender CreateEventReminderEmail(string email, string eventName, DateTime eventDate, string? userName = null)
        {
            var subject = $"📅 Reminder: {eventName} - {EmailTemplateConfig.AppName}";

            var body = GenerateEmailTemplate(
                title: "Event Reminder",
                greeting: $"Hello {(string.IsNullOrEmpty(userName) ? "" : userName + ",")}",
                mainContent: $@"
                    <p>This is a friendly reminder about your upcoming event.</p>
                    
                    <div style='background: #f3e5f5; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                        <h4 style='color: #7b1fa2; margin-top: 0;'>🎯 Event Details:</h4>
                        <p><strong>Event:</strong> {eventName}</p>
                        <p><strong>Date:</strong> {eventDate:dddd, MMMM dd, yyyy}</p>
                        <p><strong>Time:</strong> {eventDate:hh:mm tt}</p>
                    </div>
                    
                    <p>We look forward to seeing you there!</p>",
                buttonText: "View Event",
                buttonLink: $"{EmailTemplateConfig.WebsiteUrl}/events",
                themeColor: "#9c27b0",
                showCalendarIcon: true
            );

            return new MailSender(email, subject, body);
        }

        // ========== Core Template Generator ==========
        private string GenerateEmailTemplate( string title, string greeting, string mainContent, string buttonText = "",
            string buttonLink = "",string themeColor = "#2ecc71", bool showImportantNote = false,
            bool showCelebration = false,bool showSecurityNote = false, bool isNotification = false,
            bool showSocialLinks = false, bool showCalendarIcon = false)
        {
            var icon = GetIconByType(title, isNotification, showCelebration, showCalendarIcon);

            return $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>{title}</title>
                <style>
                    {GetEmailStyles(themeColor)}
                </style>
            </head>
            <body>
                <!-- Header -->
                <div class='email-container'>
                    <div class='header' style='background: linear-gradient(135deg, {themeColor}, {DarkenColor(themeColor)});'>
                        <div class='header-content'>
                            <h1>{icon} {title}</h1>
                            <p>{EmailTemplateConfig.CompanyName}</p>
                        </div>
                    </div>
                    
                    <!-- Content -->
                    <div class='content'>
                        <!-- Greeting -->
                        <div class='greeting'>
                            <h2>{greeting}</h2>
                        </div>
                        
                        <!-- Main Content -->
                        {mainContent}
                        
                        <!-- Button (if provided) -->
                        {(string.IsNullOrEmpty(buttonText) ? "" : $@"
                        <div class='button-container'>
                            <a href='{buttonLink}' class='action-button'>{buttonText}</a>
                        </div>")}
                        
                        <!-- Additional Notes -->
                        {GetAdditionalNotes(showImportantNote, showSecurityNote)}
                        
                        <!-- Social Links (if enabled) -->
                        {(showSocialLinks ? GenerateSocialLinks() : "")}
                    </div>
                    
                    <!-- Footer -->
                    {GenerateEmailFooter()}
                </div>
            </body>
            </html>";
        }

        // ========== Helper Methods ==========
        private string GetEmailStyles(string themeColor)
        {
            return $@"
                body {{
                    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                    line-height: 1.6;
                    color: #333;
                    margin: 0;
                    padding: 0;
                    background-color: #f5f5f5;
                }}
                .email-container {{
                    max-width: 600px;
                    margin: 0 auto;
                    background: white;
                    border-radius: 10px;
                    overflow: hidden;
                    box-shadow: 0 4px 20px rgba(0,0,0,0.1);
                }}
                .header {{
                    color: white;
                    text-align: center;
                    padding: 40px 20px;
                }}
                .header-content h1 {{
                    margin: 0;
                    font-size: 28px;
                    font-weight: 600;
                }}
                .header-content p {{
                    margin: 10px 0 0;
                    font-size: 16px;
                    opacity: 0.9;
                }}
                .content {{
                    padding: 40px;
                }}
                .greeting h2 {{
                    color: #2c3e50;
                    margin-top: 0;
                    font-size: 24px;
                }}
                .button-container {{
                    text-align: center;
                    margin: 30px 0;
                }}
                .action-button {{
                    display: inline-block;
                    background: {themeColor};
                    color: white;
                    padding: 15px 40px;
                    text-decoration: none;
                    border-radius: 5px;
                    font-weight: bold;
                    font-size: 16px;
                    transition: background 0.3s ease;
                }}
                .action-button:hover {{
                    background: {DarkenColor(themeColor)};
                    text-decoration: none;
                }}
                .important-note {{
                    background: #fff3cd;
                    border-left: 4px solid #ffc107;
                    padding: 15px;
                    margin: 20px 0;
                    border-radius: 4px;
                }}
                .security-note {{
                    background: #f8d7da;
                    border-left: 4px solid #dc3545;
                    padding: 15px;
                    margin: 20px 0;
                    border-radius: 4px;
                }}
                .footer {{
                    background: #f8f9fa;
                    padding: 30px;
                    text-align: center;
                    border-top: 1px solid #e9ecef;
                }}
                .contact-info {{
                    margin-bottom: 20px;
                }}
                .contact-info p {{
                    margin: 5px 0;
                    color: #6c757d;
                    font-size: 14px;
                }}
                .social-links {{
                    margin: 20px 0;
                }}
                .social-links a {{
                    display: inline-block;
                    margin: 0 10px;
                    color: {themeColor};
                    text-decoration: none;
                }}
                .unsubscribe {{
                    margin-top: 20px;
                    font-size: 12px;
                    color: #adb5bd;
                }}
                @media (max-width: 600px) {{
                    .content {{
                        padding: 20px;
                    }}
                    .header {{
                        padding: 30px 15px;
                    }}
                    .header-content h1 {{
                        font-size: 24px;
                    }}
                }}
            ";
        }

        private string GenerateEmailFooter()
        {
            return $@"
                <div class='footer'>
                    <div class='contact-info'>
                        <p><strong>{EmailTemplateConfig.CompanyName}</strong></p>
                        <p>📍 {EmailTemplateConfig.SupportLocation}</p>
                        <p>📞 {EmailTemplateConfig.SupportPhone}</p>
                        <p>✉️ {EmailTemplateConfig.SupportEmail}</p>
                        <p>🌐 {EmailTemplateConfig.WebsiteUrl}</p>
                    </div>
                    
                    <div class='social-links'>
                        <a href='{EmailTemplateConfig.FacebookUrl}'>Facebook</a> •
                        <a href='{EmailTemplateConfig.TwitterUrl}'>Twitter</a> •
                        <a href='{EmailTemplateConfig.InstagramUrl}'>Instagram</a> •
                        <a href='{EmailTemplateConfig.LinkedInUrl}'>LinkedIn</a>
                    </div>
                    
                    <div class='unsubscribe'>
                        <p>© {DateTime.Now.Year} {EmailTemplateConfig.CompanyName}. All rights reserved.</p>
                        <p>This is an automated message. Please do not reply to this email.</p>
                        <p>Working Hours: {EmailTemplateConfig.Templates.ContactHours}</p>
                        <p><a href='{EmailTemplateConfig.WebsiteUrl}/unsubscribe' style='color: #adb5bd;'>Unsubscribe</a> | 
                           <a href='{EmailTemplateConfig.WebsiteUrl}/privacy' style='color: #adb5bd;'>Privacy Policy</a></p>
                    </div>
                </div>";
        }

        private string GenerateSocialLinks()
        {
            return $@"
                <div class='social-links' style='text-align: center; margin: 30px 0;'>
                    <p style='margin-bottom: 10px; color: #6c757d;'>Follow us:</p>
                    <a href='{EmailTemplateConfig.FacebookUrl}' style='margin: 0 10px; color: #1877f2;'>Facebook</a>
                    <a href='{EmailTemplateConfig.TwitterUrl}' style='margin: 0 10px; color: #1da1f2;'>Twitter</a>
                    <a href='{EmailTemplateConfig.InstagramUrl}' style='margin: 0 10px; color: #e4405f;'>Instagram</a>
                    <a href='{EmailTemplateConfig.LinkedInUrl}' style='margin: 0 10px; color: #0077b5;'>LinkedIn</a>
                </div>";
        }

        private string GetAdditionalNotes(bool showImportantNote, bool showSecurityNote)
        {
            var notes = "";

            if (showImportantNote)
            {
                notes += $@"
                    <div class='important-note'>
                        <p><strong>⚠️ Important:</strong> This email contains time-sensitive information. 
                        Please take appropriate action before it expires.</p>
                    </div>";
            }

            if (showSecurityNote)
            {
                notes += $@"
                    <div class='security-note'>
                        <p><strong>🔒 Security Notice:</strong> Never share your password, OTP, or personal information. 
                        {EmailTemplateConfig.CompanyName} will never ask for sensitive information via email.</p>
                    </div>";
            }

            return notes;
        }

        private string GetIconByType(string title, bool isNotification, bool showCelebration, bool showCalendarIcon)
        {
            if (showCelebration) return "🎉";
            if (showCalendarIcon) return "📅";
            if (isNotification) return "📢";

            if (title.Contains("Welcome", StringComparison.OrdinalIgnoreCase)) return "🌟";
            if (title.Contains("Verify", StringComparison.OrdinalIgnoreCase)) return "🎯";
            if (title.Contains("Password", StringComparison.OrdinalIgnoreCase)) return "🔐";
            if (title.Contains("Order", StringComparison.OrdinalIgnoreCase)) return "✅";
            if (title.Contains("Support", StringComparison.OrdinalIgnoreCase)) return "🛠️";

            return "🌱"; // Default icon
        }

        private string DarkenColor(string hexColor)
        {
            // Simple darken function (reduce brightness by 20%)
            try
            {
                var color = System.Drawing.ColorTranslator.FromHtml(hexColor);
                return System.Drawing.ColorTranslator.ToHtml(
                    System.Drawing.Color.FromArgb(
                        Math.Max(color.R - 50, 0),
                        Math.Max(color.G - 50, 0),
                        Math.Max(color.B - 50, 0)
                    )
                );
            }
            catch
            {
                return hexColor;
            }
        }
    }
}