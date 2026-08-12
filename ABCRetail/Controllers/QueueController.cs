using Microsoft.AspNetCore.Mvc;
using ABCRetail.Services;
using System;
using System.Threading.Tasks;

namespace ABCRetail.Controllers
{
    public class QueueController : Controller
    {
        private readonly QueueStorageService _queueService;

        public QueueController(QueueStorageService queueService)
        {
            _queueService = queueService;
        }

        // ──────────────────────────────────────────────────────────────
        //  Display Queue Messages
        // ──────────────────────────────────────────────────────────────

        public async Task<IActionResult> Index()
        {
            try
            {
                var messages = await _queueService.PeekMessagesAsync(50);
                var queueLength = await _queueService.GetQueueLengthAsync();

                ViewBag.QueueLength = queueLength;
                ViewBag.MessageCount = messages?.Count ?? 0;

                return View(messages ?? new System.Collections.Generic.List<string>());
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to retrieve queue messages: {ex.Message}";
                return View(new System.Collections.Generic.List<string>());
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  Send Test Message
        // ──────────────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendTestMessage()
        {
            try
            {
                var message = $"Test order message - {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
                var result = await _queueService.SendMessageAsync(message);

                if (result)
                {
                    TempData["Success"] = "Test message sent to queue!";
                }
                else
                {
                    TempData["Error"] = "Failed to send test message.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // ──────────────────────────────────────────────────────────────
        //  Process Messages (Receive and Remove)
        // ──────────────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessMessages()
        {
            try
            {
                var messages = await _queueService.ReceiveMessagesAsync(10);

                if (messages != null && messages.Count > 0)
                {
                    TempData["Success"] = $"Processed {messages.Count} messages from the queue.";
                }
                else
                {
                    TempData["Success"] = "No messages to process.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error processing messages: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // ──────────────────────────────────────────────────────────────
        //  Clear Queue
        // ──────────────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearQueue()
        {
            try
            {
                var result = await _queueService.ClearQueueAsync();

                if (result)
                {
                    TempData["Success"] = "Queue cleared successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to clear queue.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error clearing queue: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}