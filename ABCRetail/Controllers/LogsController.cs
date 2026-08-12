using Microsoft.AspNetCore.Mvc;
using ABCRetail.Services;
using System;
using System.Threading.Tasks;

namespace ABCRetail.Controllers
{
    public class LogsController : Controller
    {
        private readonly FileStorageService _fileService;

        public LogsController(FileStorageService fileService)
        {
            _fileService = fileService;
        }

        // ──────────────────────────────────────────────────────────────
        //  Display Log Files
        // ──────────────────────────────────────────────────────────────

        public async Task<IActionResult> Index()
        {
            try
            {
                var fileDetails = await _fileService.GetLogFileDetailsAsync();
                ViewBag.TotalLogs = fileDetails?.Count ?? 0;
                return View(fileDetails ?? new System.Collections.Generic.List<dynamic>());
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to retrieve log files: {ex.Message}";
                return View(new System.Collections.Generic.List<dynamic>());
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  View Log File Content
        // ──────────────────────────────────────────────────────────────

        public async Task<IActionResult> ViewLog(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                TempData["Error"] = "File name is required.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var content = await _fileService.ReadLogFileAsync(fileName);
                var fileSize = await _fileService.GetFileSizeAsync(fileName);

                if (content == null)
                {
                    TempData["Error"] = "Log file not found.";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.FileName = fileName;
                ViewBag.FileSize = fileSize;
                ViewBag.LineCount = content.Split('\n').Length;

                return View(content as object);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to view log file: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  Download Log File
        // ──────────────────────────────────────────────────────────────

        public async Task<IActionResult> Download(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                TempData["Error"] = "File name is required.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var stream = await _fileService.DownloadLogFileAsync(fileName);

                if (stream == null)
                {
                    TempData["Error"] = "Log file not found.";
                    return RedirectToAction(nameof(Index));
                }

                return File(stream, "text/plain", fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to download log file: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  Delete Log File
        // ──────────────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                TempData["Error"] = "File name is required.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var result = await _fileService.DeleteLogFileAsync(fileName);

                if (result)
                {
                    TempData["Success"] = $"Log file '{fileName}' deleted successfully!";
                }
                else
                {
                    TempData["Error"] = $"Failed to delete log file '{fileName}'.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to delete log file: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // ──────────────────────────────────────────────────────────────
        //  Create Test Log
        // ──────────────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTestLog()
        {
            try
            {
                var result = await _fileService.CreateTestLogAsync();

                if (result)
                {
                    TempData["Success"] = "Test log file created successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to create test log file.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to create test log: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}