namespace EcoRecyclersGreenTech.Data.Users
{
    public class Factory
    {
        public int FactoryID { get; set; }
        public string FactoryName { get; set; } = null!;
        public string? FactoryImgURL { get; set; }
        public string? FactoryType { get; set; }
        public string? Description { get; set; }
    }
}
