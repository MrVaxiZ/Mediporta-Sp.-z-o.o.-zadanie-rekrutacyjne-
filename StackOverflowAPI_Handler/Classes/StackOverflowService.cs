using StackOverflowAPI_Handler.Properties;
using StackOverflowAPI_Handler.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace StackOverflowAPI_Handler.Classes
{
    /// <summary>
    /// Service class for interacting with the Stack Overflow API to fetch tags and manage them in a local database.
    /// </summary>
    public class StackOverflowService
    {
        private const string NameOfHttpClient = "StackOverflowClient";
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly ILogger<StackOverflowService> _logger;

        /// <summary>
        /// Initializes a new instance of the StackOverflowService.
        /// </summary>
        /// <param name="context">The database context for accessing the local database.</param>
        /// <param name="httpClientFactory">Factory for creating HttpClient instances.</param>
        /// <param name="logger">Logger for logging messages.</param>
        public StackOverflowService(ApplicationDbContext context, IHttpClientFactory httpClientFactory, ILogger<StackOverflowService> logger)
        {
            _context = context;
            _logger = logger;

            _httpClient = httpClientFactory.CreateClient(NameOfHttpClient);
        }

        /// <summary>
        /// Retrieves all tags from the local database.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains an enumeration of tags.</returns>
        public async Task<IEnumerable<Tag>> GetTagsFromDatabaseAsync()
        {
            _logger.LogInformation("Retrieving tags from the database.");
            return await _context.Tags.ToListAsync();
        }

        /// <summary>
        /// Saves given ApplicationDbContext to the database
        /// </summary>
        /// <param name="context"></param>
        public async void SaveDataToDataBase(ApplicationDbContext context)
        {
            try
            {
                _logger.LogInformation("Saving data to the database...");
                await context.SaveChangesAsync();
                _logger.LogInformation("Saving has been completed.");
            }
            catch (Exception ex)
            {
                string e = "Exception occured during saving data to the database!";
                _logger.LogCritical(e, ex);
            }
        }

        /// <summary>
        /// Fetches tags from the Stack Overflow API and updates the local database.
        /// Ensures that a maximum of 1000 tags are fetched and stored.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task FetchTagsFromStackOverflowAsync()
        {
            _logger.LogInformation("Starting to fetch tags from Stack Overflow.");

            var allTags = new HashSet<Tag>(await _context.Tags.ToListAsync());
            int page = 1;
            bool keepFetching = true;

            while (keepFetching && allTags.Count < 1001)
            {
                string requestUri = $"tags?page={page}&pagesize=100&order=desc&sort=popular&site=stackoverflow";
                _logger.LogDebug($"Fetching tags from Stack Overflow API: {requestUri}");
                HttpResponseMessage response = await _httpClient.GetAsync(requestUri);
                response.EnsureSuccessStatusCode();

                Stream stream = await response.Content.ReadAsStreamAsync();
                JsonDocument jsonDocument = await JsonDocument.ParseAsync(stream);
                JsonElement items = jsonDocument.RootElement.GetProperty("items");

                foreach (JsonElement item in items.EnumerateArray())
                {
                    string? name = item.GetProperty("name").GetString();
                    int count = item.GetProperty("count").GetInt32();

                    // Add new tags to the set, avoiding duplicates.
                    if (allTags.All(tag => tag.Name != name))
                    {
                        allTags.Add(new Tag { Name = name, Count = count });
                    }
                }

                _logger.LogDebug($"Fetched {items.GetArrayLength()} tags in page {page}. Current total: {allTags.Count}");

                // Stop fetching if less than 100 tags were returned in the last page.
                if (items.GetArrayLength() < 100)
                {
                    keepFetching = false;
                }

                page++;
            }

            // Calculate and update percentage for each tag.
            long totalTagCount = allTags.Sum(tag => tag.Count);
            foreach (Tag tag in allTags)
            {
                tag.ProcentItHasInSumOfAllTagsCount = (float)tag.Count / totalTagCount * 100;
            }

            // Update database with new tag information.
            _context.Tags.UpdateRange(allTags);
            SaveDataToDataBase(_context);

            Flags.IsFetchingTagsFromSOCompletedSuccessfully = _context.Tags.Any();
            if (Flags.IsFetchingTagsFromSOCompletedSuccessfully)
            {
                _logger.LogInformation($"Successfully synced tags with Stack Overflow. Total tags now: {allTags.Count}.");
            }
            else
            {
                _logger.LogError("Failed to sync tags with Stack Overflow.");
            }
        }

        /// <summary>
        /// Clears all tags from the database and fetches them anew from the Stack Overflow API.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task RefreshDatabaseAsync()
        {
            _logger.LogInformation("Refreshing database with new tags from Stack Overflow.");
            _context.Tags.RemoveRange(_context.Tags);
            await _context.SaveChangesAsync();
            await FetchTagsFromStackOverflowAsync();
        }
    }
}
