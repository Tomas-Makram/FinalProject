using EcoRecyclersGreenTech.Models.FactoryStore.Products;

namespace EcoRecyclersGreenTech.Models.FactoryStore.Orders
{
    public class JobDetailsModel
    {
        public JobModel Job { get; set; } = new JobModel();
        public bool CanEdit { get; set; }
        public int OrdersCount { get; set; }

        public List<JobOrderRowModel> Orders { get; set; } = new();
    }
}
