using EcoRecyclersGreenTech.Models.FactoryStore.Products;

namespace EcoRecyclersGreenTech.Models.FactoryStore.Orders
{
    public class JobDetailsModel
    {
        public JobProductDetailsModel Job { get; set; } = new JobProductDetailsModel();
        public bool CanEdit { get; set; }
        public int OrdersCount { get; set; }

        public List<JobOrderDetailsModel> Orders { get; set; } = new();
    }
}
