namespace Jumia_Clone.CustomException
{
    public class InsufficientStockException : Exception
    {
        public int RequestedQuantity { get; }
        public int AvailableStock { get; }

        public InsufficientStockException(int requestedQuantity, int availableStock)
            : base($"Insufficient stock. Requested: {requestedQuantity}, Available: {availableStock}")
        {
            RequestedQuantity = requestedQuantity;
            AvailableStock = availableStock;
        }
    }
}
