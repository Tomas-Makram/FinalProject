using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace EcoRecyclersGreenTech.Services
{
    public interface IEncryptionKeyService
    {
        string GetEncryptionKey();
        Task<bool> RotateKeyAsync();
    }

    public class EncryptionKeyService : IEncryptionKeyService
    {
        private readonly string _encryptionKey;
        private readonly IConfiguration _config;
        private readonly ILogger<EncryptionKeyService> _logger;
        private readonly bool _isDevelopment;

        public EncryptionKeyService(IConfiguration config, ILogger<EncryptionKeyService> logger, IHostEnvironment environment)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _isDevelopment = environment?.IsDevelopment() ?? false;

            _encryptionKey = LoadOrGenerateEncryptionKey();
        }

        public string GetEncryptionKey() => _encryptionKey;

        public async Task<bool> RotateKeyAsync()
        {
            try
            {
                var newKey = GenerateSecureKey();
                var success = await StoreKeyAsync(newKey);

                if (success)
                {
                    _logger.LogInformation("Encryption key rotated successfully");
                }
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to rotate encryption key");
                return false;
            }
        }

        private string LoadOrGenerateEncryptionKey()
        {
            var key = _config["Encryption:Key"]
                     ?? _config["ENCRYPTION_KEY"]
                     ?? Environment.GetEnvironmentVariable("ENCRYPTION_KEY");

            if (!string.IsNullOrWhiteSpace(key))
            {
                _logger.LogInformation("🔑 Encryption key loaded from configuration");
                return key;
            }

            _logger.LogWarning("⚠️ Encryption key not found. Generating new key...");
            var newKey = GenerateSecureKey();

            _ = Task.Run(async () =>
            {
                var stored = await StoreKeyAsync(newKey);
                if (!stored)
                {
                    ShowKeyStorageInstructions(newKey);
                }
            });

            return newKey;
        }

        private string GenerateSecureKey()
        {
            using var rng = RandomNumberGenerator.Create();
            var keyBytes = new byte[32];
            rng.GetBytes(keyBytes);
            return Convert.ToBase64String(keyBytes);
        }

        private async Task<bool> StoreKeyAsync(string key)
        {
            if (_isDevelopment)
            {
                return await TryStoreInUserSecretsAsync(key) || await TryInitAndStoreUserSecretsAsync(key);
            }

            _logger.LogInformation("🔑 Production Environment - Use ENVIRONMENT VARIABLES for encryption key");
            return false;
        }

        private async Task<bool> TryStoreInUserSecretsAsync(string key)
        {
            try
            {
                var projectDirectory = FindProjectDirectory();
                if (string.IsNullOrEmpty(projectDirectory)) return false;

                var psi = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"user-secrets set ENCRYPTION_KEY \"{key}\"",
                    WorkingDirectory = projectDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                if (process == null) return false;

                await process.WaitForExitAsync();

                if (process.ExitCode == 0)
                {
                    _logger.LogInformation("✅ Encryption key stored in User Secrets");
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> TryInitAndStoreUserSecretsAsync(string key)
        {
            try
            {
                var projectDirectory = FindProjectDirectory();
                if (string.IsNullOrEmpty(projectDirectory)) return false;

                var initPsi = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "user-secrets init",
                    WorkingDirectory = projectDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var initProcess = Process.Start(initPsi);
                if (initProcess == null) return false;

                await initProcess.WaitForExitAsync();

                // ثانياً: تخزين المفتاح
                if (initProcess.ExitCode == 0)
                {
                    return await TryStoreInUserSecretsAsync(key);
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private void ShowKeyStorageInstructions(string key)
        {
            _logger.LogWarning($"""
        🚨 MANUAL CONFIGURATION REQUIRED 🚨
        
        Encryption Key: {key}

        To configure automatically, run these commands:
        --------------------------------------------------
        cd "D:\Universty\EcoRecyclersGreenTech\EcoRecyclersGreenTech"
        dotnet user-secrets init
        dotnet user-secrets set ENCRYPTION_KEY "{key}"
        --------------------------------------------------

        Or set environment variable:
        ENCRYPTION_KEY={key}

        ⚠️  Save this key securely - you'll need it to decrypt data!
        """);

            Console.WriteLine($"""
                🔑 NEW ENCRYPTION KEY GENERATED:
                {key}
                
                Run these commands to store it:
                cd "D:\Universty\EcoRecyclersGreenTech\EcoRecyclersGreenTech"
                dotnet user-secrets init
                dotnet user-secrets set ENCRYPTION_KEY "{key}"
                """);
        }

        private static string? FindProjectDirectory()
        {
            try
            {
                var baseDirectory = AppContext.BaseDirectory;
                var directory = new DirectoryInfo(baseDirectory);

                while (directory != null && !directory.GetFiles("*.csproj").Any())
                {
                    directory = directory.Parent;
                }

                return directory?.FullName;
            }
            catch
            {
                return null;
            }
        }
    }
}