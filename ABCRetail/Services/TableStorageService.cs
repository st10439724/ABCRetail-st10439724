using Azure.Data.Tables;
using ABCRetail.Models;

namespace ABCRetail.Services
{
    public class TableStorageService
    {
        private readonly TableClient _customerTable;
        private readonly TableClient _productTable;

        public TableStorageService(IConfiguration configuration)
        {
            var connectionString = configuration["AzureStorage:ConnectionString"];

            // Each entity type gets its own table in Azure Table Storage (i tried this 1st seme and it backfired
            _customerTable = new TableClient(connectionString, "CustomerProfiles");
            _productTable = new TableClient(connectionString, "Products");

            //
            _customerTable.CreateIfNotExists();
            _productTable.CreateIfNotExists();
        }

        // ──────────────────────────────────────────────────────────────
        //  Customer Profile Methods
        // ──────────────────────────────────────────────────────────────

        public async Task AddCustomerAsync(CustomerProfile customer)
        {
            await _customerTable.AddEntityAsync(customer);
        }

        public async Task<List<CustomerProfile>> GetAllCustomersAsync()
        {
            var customers = new List<CustomerProfile>();
            await foreach (var customer in _customerTable.QueryAsync<CustomerProfile>())
            {
                customers.Add(customer);
            }
            return customers;
        }

        public async Task<CustomerProfile?> GetCustomerAsync(string rowKey)
        {
            try
            {
                var response = await _customerTable.GetEntityAsync<CustomerProfile>("CustomerProfile", rowKey);
                return response.Value;
            }
            catch
            {
                return null;
            }
        }

        public async Task UpdateCustomerAsync(CustomerProfile customer)
        {
            await _customerTable.UpdateEntityAsync(customer, customer.ETag);
        }

        public async Task DeleteCustomerAsync(string rowKey)
        {
            await _customerTable.DeleteEntityAsync("CustomerProfile", rowKey);
        }

        // ──────────────────────────────────────────────────────────────
        //  Product Methods
        // ──────────────────────────────────────────────────────────────

        public async Task AddProductAsync(Product product)
        {
            await _productTable.AddEntityAsync(product);
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            var products = new List<Product>();
            await foreach (var product in _productTable.QueryAsync<Product>())
            {
                products.Add(product);
            }
            return products;
        }

        public async Task<Product?> GetProductAsync(string rowKey)
        {
            try
            {
                var response = await _productTable.GetEntityAsync<Product>("Product", rowKey);
                return response.Value;
            }
            catch
            {
                return null;
            }
        }

        public async Task UpdateProductAsync(Product product)
        {
            await _productTable.UpdateEntityAsync(product, product.ETag);
        }

        public async Task DeleteProductAsync(string rowKey)
        {
            await _productTable.DeleteEntityAsync("Product", rowKey);
        }
    }
}
