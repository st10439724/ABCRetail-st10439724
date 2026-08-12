using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ABCRetail.Services
{
    public class FileStorageService
    {
        private readonly ShareClient _logShare;
        private readonly string _directoryName;

        public FileStorageService(IConfiguration configuration)
        {
            var connectionString = configuration["AzureStorage:ConnectionString"];
            var shareName = configuration["AzureStorage:FileShares:Logs"] ?? "logs";
            _directoryName = "application";

            _logShare = new ShareClient(connectionString, shareName);
            _logShare.CreateIfNotExists();

            var directoryClient = _logShare.GetDirectoryClient(_directoryName);
            directoryClient.CreateIfNotExists();
        }

        private ShareDirectoryClient GetDirectoryClient()
        {
            return _logShare.GetDirectoryClient(_directoryName);
        }

        public async Task<bool> AppendLogAsync(string fileName, string logEntry)
        {
            try
            {
                var directoryClient = GetDirectoryClient();
                var fileClient = directoryClient.GetFileClient(fileName);

                var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var logLine = $"[{timestamp}] {logEntry}{Environment.NewLine}";
                var logBytes = Encoding.UTF8.GetBytes(logLine);

                // Read existing content if file exists
                string existingContent = "";
                if (await fileClient.ExistsAsync())
                {
                    var response = await fileClient.DownloadAsync();
                    using var reader = new StreamReader(response.Value.Content);
                    existingContent = await reader.ReadToEndAsync();
                }

                // Combine existing + new content
                var newContent = existingContent + logLine;
                var newBytes = Encoding.UTF8.GetBytes(newContent);

                // Upload the complete file
                using var stream = new MemoryStream(newBytes);
                await fileClient.CreateAsync(newBytes.Length);
                await fileClient.UploadAsync(stream);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error appending log: {ex.Message}");
                return false;
            }
        }

        public async Task<string?> ReadLogFileAsync(string fileName)
        {
            try
            {
                var directoryClient = GetDirectoryClient();
                var fileClient = directoryClient.GetFileClient(fileName);

                if (!await fileClient.ExistsAsync())
                {
                    return null;
                }

                var response = await fileClient.DownloadAsync();
                using var reader = new StreamReader(response.Value.Content);
                return await reader.ReadToEndAsync();
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<object>> GetLogFileDetailsAsync()
        {
            try
            {
                var directoryClient = GetDirectoryClient();
                var fileDetails = new List<object>();

                var response = directoryClient.GetFilesAndDirectoriesAsync();
                await foreach (var item in response)
                {
                    if (!item.IsDirectory)
                    {
                        var fileClient = directoryClient.GetFileClient(item.Name);
                        var properties = await fileClient.GetPropertiesAsync();

                        // FIXED: LastModified is DateTimeOffset (not nullable)
                        // Just use it directly
                        var lastModified = properties.Value.LastModified;

                        fileDetails.Add(new
                        {
                            Name = item.Name,
                            Size = properties.Value.ContentLength,
                            LastModified = lastModified
                        });
                    }
                }

                return fileDetails;
            }
            catch
            {
                return new List<object>();
            }
        }

        public async Task<bool> DeleteLogFileAsync(string fileName)
        {
            try
            {
                var directoryClient = GetDirectoryClient();
                var fileClient = directoryClient.GetFileClient(fileName);

                var response = await fileClient.DeleteIfExistsAsync();
                return response.Value;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Stream?> DownloadLogFileAsync(string fileName)
        {
            try
            {
                var directoryClient = GetDirectoryClient();
                var fileClient = directoryClient.GetFileClient(fileName);

                if (!await fileClient.ExistsAsync())
                {
                    return null;
                }

                var response = await fileClient.DownloadAsync();
                var memoryStream = new MemoryStream();
                await response.Value.Content.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                return memoryStream;
            }
            catch
            {
                return null;
            }
        }

        public async Task<long> GetFileSizeAsync(string fileName)
        {
            try
            {
                var directoryClient = GetDirectoryClient();
                var fileClient = directoryClient.GetFileClient(fileName);

                if (!await fileClient.ExistsAsync())
                {
                    return 0;
                }

                var properties = await fileClient.GetPropertiesAsync();
                return properties.Value.ContentLength;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<bool> CreateTestLogAsync()
        {
            try
            {
                var fileName = $"log_{DateTime.UtcNow:yyyyMMdd_HHmmss}.txt";
                var logEntries = new List<string>
                {
                    "=== APPLICATION STARTUP ===",
                    $"Application started at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} UTC",
                    "Running on Azure File Storage",
                    "=== TEST LOG ENTRY ===",
                    $"Test log created at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} UTC",
                    "=== END OF TEST LOG ==="
                };

                foreach (var entry in logEntries)
                {
                    await AppendLogAsync(fileName, entry);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}