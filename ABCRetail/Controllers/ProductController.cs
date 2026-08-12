//using ABCRetail.Models;
//using ABCRetail.Services;
//using Microsoft.AspNetCore.Mvc;

//namespace ABCRetail.Controllers
//{
//    public class ProductController : Controller
//    {
//        private readonly TableStorageService _tableService;
//        private readonly BlobStorageService _blobService;

//        public ProductController(TableStorageService tableService, BlobStorageService blobService)
//        {
//            _tableService = tableService;
//            _blobService = blobService;
//        }

//        // GET: Product
//        public async Task<IActionResult> Index()
//        {
//            var products = await _tableService.GetAllProductsAsync();
//            return View(products);
//        }

//        // GET: Product/Create
//        public IActionResult Create()
//        {
//            return View();
//        }

//        // POST: Product/Create
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
//        {
//            if (!ModelState.IsValid)
//            {
//                TempData["Error"] = "Please fill in all required fields.";
//                return View(product);
//            }

//            try
//            {
//                product.RowKey = Guid.NewGuid().ToString();

//                // Upload image to Blob Storage if one was provided
//                if (imageFile != null && imageFile.Length > 0)
//                {
//                    product.ImageUrl = await _blobService.UploadImageAsync(imageFile);
//                }

//                await _tableService.AddProductAsync(product);
//                TempData["Success"] = "Product added successfully.";
//                return RedirectToAction(nameof(Index));
//            }
//            catch (Exception ex)
//            {
//                TempData["Error"] = $"Something went wrong: {ex.Message}";
//                return View(product);
//            }
//        }

//        // GET: Product/Edit/{rowKey}
//        public async Task<IActionResult> Edit(string rowKey)
//        {
//            var product = await _tableService.GetProductAsync(rowKey);
//            if (product == null)
//            {
//                TempData["Error"] = "Product not found.";
//                return RedirectToAction(nameof(Index));
//            }
//            return View(product);
//        }

//        // POST: Product/Edit
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(Product product, IFormFile? imageFile)
//        {
//            if (!ModelState.IsValid)
//            {
//                TempData["Error"] = "Please correct the errors and try again.";
//                return View(product);
//            }

//            try
//            {
//                // If a new image was uploaded, delete the old one and upload the new one
//                if (imageFile != null && imageFile.Length > 0)
//                {
//                    if (!string.IsNullOrEmpty(product.ImageUrl))
//                        await _blobService.DeleteImageAsync(product.ImageUrl);

//                    product.ImageUrl = await _blobService.UploadImageAsync(imageFile);
//                }

//                await _tableService.UpdateProductAsync(product);
//                TempData["Success"] = "Product updated successfully.";
//                return RedirectToAction(nameof(Index));
//            }
//            catch (Exception ex)
//            {
//                TempData["Error"] = $"Update failed: {ex.Message}";
//                return View(product);
//            }
//        }

//        // GET: Product/Delete/{rowKey}
//        public async Task<IActionResult> Delete(string rowKey)
//        {
//            var product = await _tableService.GetProductAsync(rowKey);
//            if (product == null)
//            {
//                TempData["Error"] = "Product not found.";
//                return RedirectToAction(nameof(Index));
//            }
//            return View(product);
//        }

//        // POST: Product/DeleteConfirmed
//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirmed(string rowKey, string? imageUrl)
//        {
//            try
//            {
//                // Remove the product image from Blob Storage before deleting the record
//                if (!string.IsNullOrEmpty(imageUrl))
//                    await _blobService.DeleteImageAsync(imageUrl);

//                await _tableService.DeleteProductAsync(rowKey);
//                TempData["Success"] = "Product deleted successfully.";
//            }
//            catch (Exception ex)
//            {
//                TempData["Error"] = $"Delete failed: {ex.Message}";
//            }
//            return RedirectToAction(nameof(Index));
//        }
//    }
//}


using ABCRetail.Models;
using ABCRetail.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetail.Controllers
{
    public class ProductController : Controller
    {
        private readonly TableStorageService _tableService;
        private readonly BlobStorageService _blobService;
        private readonly FileStorageService _fileService;
        private readonly QueueStorageService _queueService;

        public ProductController(TableStorageService tableService, BlobStorageService blobService,
            FileStorageService fileService, QueueStorageService queueService)
        {
            _tableService = tableService;
            _blobService = blobService;
            _fileService = fileService;
            _queueService = queueService;
        }

        // GET: Product
        public async Task<IActionResult> Index()
        {
            var products = await _tableService.GetAllProductsAsync();
            return View(products);
        }

        // GET: Product/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill in all required fields.";
                return View(product);
            }

            try
            {
                product.RowKey = Guid.NewGuid().ToString();

                // Upload image to Blob Storage if one was provided
                if (imageFile != null && imageFile.Length > 0)
                {
                    product.ImageUrl = await _blobService.UploadImageAsync(imageFile);
                }

                await _tableService.AddProductAsync(product);

                // Log the action
                await _fileService.AppendLogAsync($"log_{DateTime.UtcNow:yyyyMMdd}.txt",
                    $"Product {product.ProductName} (ID: {product.RowKey}) created at {DateTime.UtcNow}");

                // Send queue message for inventory management
                await _queueService.SendMessageAsync(
                    $"Product {product.ProductName} (ID: {product.RowKey}) added to inventory. Stock: {product.StockQuantity}");

                TempData["Success"] = "Product added successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Something went wrong: {ex.Message}";
                return View(product);
            }
        }

        // GET: Product/Edit/{rowKey}
        public async Task<IActionResult> Edit(string rowKey)
        {
            var product = await _tableService.GetProductAsync(rowKey);
            if (product == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // POST: Product/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct the errors and try again.";
                return View(product);
            }

            try
            {
                // If a new image was uploaded, delete the old one and upload the new one
                if (imageFile != null && imageFile.Length > 0)
                {
                    if (!string.IsNullOrEmpty(product.ImageUrl))
                        await _blobService.DeleteImageAsync(product.ImageUrl);

                    product.ImageUrl = await _blobService.UploadImageAsync(imageFile);
                }

                await _tableService.UpdateProductAsync(product);

                // Log the action
                await _fileService.AppendLogAsync($"log_{DateTime.UtcNow:yyyyMMdd}.txt",
                    $"Product {product.ProductName} (ID: {product.RowKey}) updated at {DateTime.UtcNow}");

                // Send queue message for inventory management
                await _queueService.SendMessageAsync(
                    $"Product {product.ProductName} (ID: {product.RowKey}) updated. New stock: {product.StockQuantity}");

                TempData["Success"] = "Product updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Update failed: {ex.Message}";
                return View(product);
            }
        }

        // GET: Product/Delete/{rowKey}
        public async Task<IActionResult> Delete(string rowKey)
        {
            var product = await _tableService.GetProductAsync(rowKey);
            if (product == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // POST: Product/DeleteConfirmed
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string rowKey, string? imageUrl)
        {
            try
            {
                // Get the product first so we can log its name
                var product = await _tableService.GetProductAsync(rowKey);
                var productName = product?.ProductName ?? rowKey;

                // Remove the product image from Blob Storage before deleting the record
                if (!string.IsNullOrEmpty(imageUrl))
                    await _blobService.DeleteImageAsync(imageUrl);

                await _tableService.DeleteProductAsync(rowKey);

                // Log the action
                await _fileService.AppendLogAsync($"log_{DateTime.UtcNow:yyyyMMdd}.txt",
                    $"Product {productName} (ID: {rowKey}) deleted at {DateTime.UtcNow}");

                // Send queue message for inventory management
                await _queueService.SendMessageAsync(
                    $"Product {productName} (ID: {rowKey}) removed from inventory at {DateTime.UtcNow}");

                TempData["Success"] = "Product deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Delete failed: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}