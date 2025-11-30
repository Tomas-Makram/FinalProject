using System.Security.Cryptography;
using System.Text;

namespace EcoRecyclersGreenTech.Services
{
    public class EncryptionService
    {
        private readonly byte[] _key;
        private readonly byte[] _fixedIV;

        public EncryptionService(string base64Key)
        {
            _key = Convert.FromBase64String(base64Key);

            _fixedIV = _key.Take(16).ToArray();
        }

        public string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _fixedIV;

            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        public string Decrypt(string cipherText)
        {
            var cipherBytes = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _fixedIV;

            using var ms = new MemoryStream(cipherBytes);
            using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }
    }
}
