using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ABCRetail.Services
{
    public class QueueStorageService
    {
        private readonly QueueClient _orderQueue;

        public QueueStorageService(IConfiguration configuration)
        {
            var connectionString = configuration["AzureStorage:ConnectionString"];
            var queueName = configuration["AzureStorage:Queues:OrderQueue"] ?? "orderqueue";

            _orderQueue = new QueueClient(connectionString, queueName);
            _orderQueue.CreateIfNotExists();
        }

        public async Task<bool> SendMessageAsync(string message)
        {
            try
            {
                var encodedMessage = Convert.ToBase64String(Encoding.UTF8.GetBytes(message));
                await _orderQueue.SendMessageAsync(encodedMessage);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<string>> PeekMessagesAsync(int maxCount = 10)
        {
            try
            {
                var messages = new List<string>();
                var response = await _orderQueue.PeekMessagesAsync(maxCount);

                if (response.Value != null)
                {
                    foreach (var message in response.Value)
                    {
                        var decodedMessage = Encoding.UTF8.GetString(
                            Convert.FromBase64String(message.MessageText));
                        messages.Add(decodedMessage);
                    }
                }

                return messages;
            }
            catch
            {
                return new List<string>();
            }
        }

        public async Task<List<string>> ReceiveMessagesAsync(int maxCount = 10)
        {
            try
            {
                var messages = new List<string>();
                var response = await _orderQueue.ReceiveMessagesAsync(maxCount);

                if (response.Value != null)
                {
                    foreach (var message in response.Value)
                    {
                        var decodedMessage = Encoding.UTF8.GetString(
                            Convert.FromBase64String(message.MessageText));
                        messages.Add(decodedMessage);
                    }
                }

                return messages;
            }
            catch
            {
                return new List<string>();
            }
        }

        public async Task<int> GetQueueLengthAsync()
        {
            try
            {
                var properties = await _orderQueue.GetPropertiesAsync();
                // Fixed: ApproximateMessagesCount is already an int, just return it
                return properties.Value.ApproximateMessagesCount;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<bool> ClearQueueAsync()
        {
            try
            {
                await _orderQueue.ClearMessagesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}












//using Azure.Storage.Queues;
//using Azure.Storage.Queues.Models;
//using Microsoft.Extensions.Configuration;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading.Tasks;

//namespace ABCRetail.Services
//{
//    public class QueueStorageService
//    {
//        private readonly QueueClient _orderQueue;

//        public QueueStorageService(IConfiguration configuration)
//        {
//            var connectionString = configuration["AzureStorage:ConnectionString"];
//            var queueName = configuration["AzureStorage:Queues:OrderQueue"] ?? "orderqueue";

//            _orderQueue = new QueueClient(connectionString, queueName);
//            _orderQueue.CreateIfNotExists();
//        }

//        // ──────────────────────────────────────────────────────────────
//        //  Send a message to the queue
//        // ──────────────────────────────────────────────────────────────

//        public async Task<bool> SendMessageAsync(string message)
//        {
//            try
//            {
//                var encodedMessage = Convert.ToBase64String(Encoding.UTF8.GetBytes(message));
//                await _orderQueue.SendMessageAsync(encodedMessage);
//                return true;
//            }
//            catch
//            {
//                return false;
//            }
//        }

//        // ──────────────────────────────────────────────────────────────
//        //  Peek at messages without removing them
//        // ──────────────────────────────────────────────────────────────

//        public async Task<List<string>> PeekMessagesAsync(int maxCount = 10)
//        {
//            try
//            {
//                var messages = new List<string>();
//                var response = await _orderQueue.PeekMessagesAsync(maxCount);

//                if (response.Value != null)
//                {
//                    foreach (var message in response.Value)
//                    {
//                        var decodedMessage = Encoding.UTF8.GetString(
//                            Convert.FromBase64String(message.MessageText));
//                        messages.Add(decodedMessage);
//                    }
//                }

//                return messages;
//            }
//            catch
//            {
//                return new List<string>();
//            }
//        }

//        // ──────────────────────────────────────────────────────────────
//        //  Receive and remove messages from the queue
//        // ──────────────────────────────────────────────────────────────

//        public async Task<List<string>> ReceiveMessagesAsync(int maxCount = 10)
//        {
//            try
//            {
//                var messages = new List<string>();
//                var response = await _orderQueue.ReceiveMessagesAsync(maxCount);

//                if (response.Value != null)
//                {
//                    foreach (var message in response.Value)
//                    {
//                        var decodedMessage = Encoding.UTF8.GetString(
//                            Convert.FromBase64String(message.MessageText));
//                        messages.Add(decodedMessage);
//                    }
//                }

//                return messages;
//            }
//            catch
//            {
//                return new List<string>();
//            }
//        }

//        // ──────────────────────────────────────────────────────────────
//        //  Get the approximate number of messages in the queue
//        // ──────────────────────────────────────────────────────────────

//        public async Task<int> GetQueueLengthAsync()
//        {
//            try
//            {
//                var properties = await _orderQueue.GetPropertiesAsync();
//                return properties.Value.ApproximateMessagesCount ?? 0;
//            }
//            catch
//            {
//                return 0;
//            }
//        }

//        // ──────────────────────────────────────────────────────────────
//        //  Clear all messages from the queue // Doesnt work???????
//        // ──────────────────────────────────────────────────────────────

//        public async Task<bool> ClearQueueAsync()
//        {
//            try
//            {
//                await _orderQueue.ClearMessagesAsync();
//                return true;
//            }
//            catch
//            {
//                return false;
//            }
//        }
//    }
//}