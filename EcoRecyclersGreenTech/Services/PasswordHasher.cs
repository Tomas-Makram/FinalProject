using Microsoft.AspNetCore.Identity;

namespace EcoRecyclersGreenTech.Services
{
    public interface PasswordHasher
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
    }

    public class PasswordHasherService : PasswordHasher
    {
        private readonly PasswordHasher<object> _passwordHasher;

        public PasswordHasherService()
        {
            _passwordHasher = new PasswordHasher<object>();
        }

        //Hashing Password (Encryption Password in one way)
        public string HashPassword(string password)
        {
            return _passwordHasher.HashPassword(null!, password);
        }

        //Check if Password is Valid or not
        public bool VerifyPassword(string password, string hashedPassword)
        {
            var result = _passwordHasher.VerifyHashedPassword(null!, hashedPassword, password);
            return result == PasswordVerificationResult.Success;
        }
    }
}