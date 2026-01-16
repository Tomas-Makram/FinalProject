using EcoRecyclersGreenTech.Data.Users;

namespace EcoRecyclersGreenTech.Models.Auth
{
    public class SignupModel
    {
        public User user { get; set; } = new User();
        public UserType? type { get; set; }

        // User Types
        public Individual? individual { get; set; }
        public Factory? factory { get; set; }
        public Craftsman? craftsman { get; set; }
        public Admin? admin { get; set; }
    }
}
