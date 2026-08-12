//using ABCRetail.Models;
//using ABCRetail.Services;
//using Microsoft.AspNetCore.Mvc;

//namespace ABCRetail.Controllers
//{
//    public class CustomerProfileController : Controller
//    {
//        private readonly TableStorageService _tableService;

//        public CustomerProfileController(TableStorageService tableService)
//        {
//            _tableService = tableService;
//        }

//        // GET: CustomerProfile
//        public async Task<IActionResult> Index()
//        {
//            var customers = await _tableService.GetAllCustomersAsync();
//            return View(customers);
//        }

//        // GET: CustomerProfile/Create
//        public IActionResult Create()
//        {
//            return View();
//        }

//        // POST: CustomerProfile/Create
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(CustomerProfile customer)
//        {
//            if (!ModelState.IsValid)
//            {
//                TempData["Error"] = "Please fill in all required fields.";
//                return View(customer);
//            }

//            try
//            {
//                customer.RowKey = Guid.NewGuid().ToString();
//                customer.DateRegistered = DateTime.UtcNow;
//                await _tableService.AddCustomerAsync(customer);
//                TempData["Success"] = "Customer profile created successfully.";
//                return RedirectToAction(nameof(Index));
//            }
//            catch (Exception ex)
//            {
//                TempData["Error"] = $"Something went wrong: {ex.Message}";
//                return View(customer);
//            }
//        }

//        // GET: CustomerProfile/Edit/{rowKey}
//        public async Task<IActionResult> Edit(string rowKey)
//        {
//            var customer = await _tableService.GetCustomerAsync(rowKey);
//            if (customer == null)
//            {
//                TempData["Error"] = "Customer not found.";
//                return RedirectToAction(nameof(Index));
//            }
//            return View(customer);
//        }

//        // POST: CustomerProfile/Edit
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(CustomerProfile customer)
//        {
//            if (!ModelState.IsValid)
//            {
//                TempData["Error"] = "Please correct the errors and try again.";
//                return View(customer);
//            }

//            try
//            {
//                await _tableService.UpdateCustomerAsync(customer);
//                TempData["Success"] = "Customer profile updated successfully.";
//                return RedirectToAction(nameof(Index));
//            }
//            catch (Exception ex)
//            {
//                TempData["Error"] = $"Update failed: {ex.Message}";
//                return View(customer);
//            }
//        }

//        // GET: CustomerProfile/Delete/{rowKey}
//        public async Task<IActionResult> Delete(string rowKey)
//        {
//            var customer = await _tableService.GetCustomerAsync(rowKey);
//            if (customer == null)
//            {
//                TempData["Error"] = "Customer not found.";
//                return RedirectToAction(nameof(Index));
//            }
//            return View(customer);
//        }

//        // POST: CustomerProfile/DeleteConfirmed
//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirmed(string rowKey)
//        {
//            try
//            {
//                await _tableService.DeleteCustomerAsync(rowKey);
//                TempData["Success"] = "Customer profile deleted.";
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
    public class CustomerProfileController : Controller
    {
        private readonly TableStorageService _tableService;
        private readonly FileStorageService _fileService;

        public CustomerProfileController(TableStorageService tableService, FileStorageService fileService)
        {
            _tableService = tableService;
            _fileService = fileService;
        }

        // GET: CustomerProfile
        public async Task<IActionResult> Index()
        {
            var customers = await _tableService.GetAllCustomersAsync();
            return View(customers);
        }

        // GET: CustomerProfile/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CustomerProfile/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerProfile customer)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill in all required fields.";
                return View(customer);
            }

            try
            {
                customer.RowKey = Guid.NewGuid().ToString();
                customer.DateRegistered = DateTime.UtcNow;
                await _tableService.AddCustomerAsync(customer);

                // Log the action
                await _fileService.AppendLogAsync($"log_{DateTime.UtcNow:yyyyMMdd}.txt",
                    $"Customer {customer.RowKey} created at {DateTime.UtcNow}");

                TempData["Success"] = "Customer profile created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Something went wrong: {ex.Message}";
                return View(customer);
            }
        }

        // GET: CustomerProfile/Edit/{rowKey}
        public async Task<IActionResult> Edit(string rowKey)
        {
            var customer = await _tableService.GetCustomerAsync(rowKey);
            if (customer == null)
            {
                TempData["Error"] = "Customer not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // POST: CustomerProfile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CustomerProfile customer)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct the errors and try again.";
                return View(customer);
            }

            try
            {
                await _tableService.UpdateCustomerAsync(customer);

                // Log the action
                await _fileService.AppendLogAsync($"log_{DateTime.UtcNow:yyyyMMdd}.txt",
                    $"Customer {customer.RowKey} updated at {DateTime.UtcNow}");

                TempData["Success"] = "Customer profile updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Update failed: {ex.Message}";
                return View(customer);
            }
        }

        // GET: CustomerProfile/Delete/{rowKey}
        public async Task<IActionResult> Delete(string rowKey)
        {
            var customer = await _tableService.GetCustomerAsync(rowKey);
            if (customer == null)
            {
                TempData["Error"] = "Customer not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // POST: CustomerProfile/DeleteConfirmed
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string rowKey)
        {
            try
            {
                await _tableService.DeleteCustomerAsync(rowKey);

                // Log the action
                await _fileService.AppendLogAsync($"log_{DateTime.UtcNow:yyyyMMdd}.txt",
                    $"Customer {rowKey} deleted at {DateTime.UtcNow}");

                TempData["Success"] = "Customer profile deleted.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Delete failed: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}