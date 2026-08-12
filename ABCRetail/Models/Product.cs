using Azure;
using Azure.Data.Tables;

namespace ABCRetail.Models
{
    // Represents a product stored in Azure Table Storage
    // PartitionKey = "Product", RowKey = ProductId
    public class Product : ITableEntity
    {
        public string PartitionKey { get; set; } = "Product";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; }
        public int StockQuantity { get; set; }

        // This will store the Blob URL of the product image
        public string ImageUrl { get; set; } = string.Empty;
    }
}
