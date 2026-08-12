using Azure;
using Azure.Data.Tables;

namespace ABCRetail.Models
{
    // Represents a customer stored in Azure Table Storage
    // PartitionKey = "CustomerProfile", RowKey = CustomerId
    public class CustomerProfile : ITableEntity
    {
        public string PartitionKey { get; set; } = "CustomerProfile";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public DateTime DateRegistered { get; set; } = DateTime.UtcNow;
    }
}
