using EcoRecyclersGreenTech.Data.Users;

namespace EcoRecyclersGreenTech.Models
{
    public class SignupDataModel
    {
        public User user { get; set; } = new User();

        public UserType? type { get; set; }
        public Individual? individual { get; set; }
        public Factory? factory { get; set; }
        public Craftsman? craftsman { get; set; }
    }
}