using System.Security.Cryptography;
using System.Text;

namespace EcoRecyclersGreenTech.Services
{
    public interface IEncryptionKeyService
    {
        string GetEncryptionKey();
        void GenerateNewKey();
    }

    public class EncryptionKeyService : IEncryptionKeyService
    {
        private string _encryptionKey;
        private readonly IWebHostEnvironment _environment;
        private readonly string _keyFilePath;

        public EncryptionKeyService(IWebHostEnvironment environment)
        {
            _environment = environment;
            _encryptionKey = "";
            _keyFilePath = Path.Combine(_environment.ContentRootPath, "encryption.key");
            LoadOrGenerateKey();
        }

        public string GetEncryptionKey()
        {
            return _encryptionKey;
        }

        // AES-256 requires 32 bytes
        public void GenerateNewKey()
        {
            var keyBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(keyBytes);

            _encryptionKey = Convert.ToBase64String(keyBytes);

            SaveKeyToFile();
        }

        private void LoadOrGenerateKey()
        {
            if (File.Exists(_keyFilePath))
            {
                _encryptionKey = File.ReadAllText(_keyFilePath);

                try
                {
                    var keyBytes = Convert.FromBase64String(_encryptionKey);
                    if (keyBytes.Length != 32)
                        GenerateNewKey();
                }
                catch
                {
                    GenerateNewKey();
                }
            }
            else
            {
                GenerateNewKey();
            }
        }

        private void SaveKeyToFile()
        {
            File.WriteAllText(_keyFilePath, _encryptionKey);
        }
    }
}
