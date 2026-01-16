using System.ComponentModel.DataAnnotations;

namespace EcoRecyclersGreenTech.Models.FactoryStore
{
    public class StatusChangeModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string? Status { get; set; }

        public string? ProductType { get; set; }
    }

}
