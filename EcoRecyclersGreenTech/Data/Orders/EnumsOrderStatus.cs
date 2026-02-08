namespace EcoRecyclersGreenTech.Data.Orders
{
    public enum EnumsOrderStatus
    {
        Pending,
        Confirmed,
        Processing,
        ReadyForPickup,
        Shipped,

        PickedUp,
        Delivered,
        Completed,
        Refunded,
        Returned,
        Cancelled,

        DeletedByBuyer,
        DeletedBySeller
    }
}
