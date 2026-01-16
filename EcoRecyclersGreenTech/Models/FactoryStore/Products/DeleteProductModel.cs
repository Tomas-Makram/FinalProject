using System.ComponentModel.DataAnnotations;

namespace EcoRecyclersGreenTech.Models.FactoryStore.Products
{
    public class DeleteProductModel
    {
        [Required(ErrorMessage = "Id is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Id")]
        public int Id { get; set; }
    }
}
