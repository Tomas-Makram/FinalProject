using System.Security.Cryptography;
using System.Text;

namespace EcoRecyclersGreenTech.Services
{
    public class EncryptionService
    {
        private readonly string _encryptionKey;

        public EncryptionService(string encryptionKey)
        {
            _encryptionKey = encryptionKey;
        }

        //Encryption Data
        public string Encrypt(string plainText)
        {
            using var aes = Aes.Create();

            aes.Key = Convert.FromBase64String(_encryptionKey);
            aes.IV = new byte[16];

            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        //Decryption Data
        public string Decrypt(string cipherText)
        {
            using var aes = Aes.Create();
            aes.Key = Convert.FromBase64String(_encryptionKey);
            aes.IV = new byte[16];

            var cipherBytes = Convert.FromBase64String(cipherText);
            using var ms = new MemoryStream(cipherBytes);
            using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }
    }
}