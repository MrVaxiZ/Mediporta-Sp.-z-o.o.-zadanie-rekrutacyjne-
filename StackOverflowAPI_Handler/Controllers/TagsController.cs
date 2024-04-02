using StackOverflowAPI_Handler.Properties;
using StackOverflowAPI_Handler.Classes;
using Microsoft.AspNetCore.Mvc;

namespace StackOverflowAPI_Handler.Controllers
{
    /// <summary>
    /// Controller for handling tag-related requests in an ASP.NET Core application.
    /// Utilizes services to interact with Stack Overflow's API and a local database.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class TagsController : ControllerBase
    {
        private readonly StackOverflowService _service;
        private readonly ILogger<TagsController> _logger;

        /// <summary>
        /// Constructs a new instance of the TagsController.
        /// </summary>
        /// <param name="service">The service used to interact with Stack Overflow's API and the database.</param>
        /// <param name="logger">The logger for logging information and warnings.</param>
        public TagsController(StackOverflowService service, ILogger<TagsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a paginated list of tags, optionally sorted by a specified field and direction.
        /// </summary>
        /// <param name="sortBy">The field to sort by. Can be "id", "name", "percentage", "count"</param>
        /// <param name="direction">The direction of sorting. Can be "asc" for ascending or "desc" for descending.</param>
        /// <param name="page">The page number to return.</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>An IActionResult containing the paginated list of tags.</returns>
        [HttpGet]
        public async Task<IActionResult> Get(string sortBy = "name", string direction = "desc", int page = 1, int pageSize = 100)
        {
            _logger.LogInformation($"Received get request for tags with sorting by {sortBy} and direction {direction}, page {page}, pageSize {pageSize}.");

            IEnumerable<Tag> tags = await _service.GetTagsFromDatabaseAsync();

            // Fetch from Stack Overflow API if the database has less than 1001 tags
            _logger.LogInformation($"Checking if tags are meeting requierments in database...");
            if (!tags.Any() || tags.Count() < 1001)
            {
                _logger.LogInformation("Database has less than 1001 tags, fetching from Stack Overflow API.");
                await _service.FetchTagsFromStackOverflowAsync();
                tags = await _service.GetTagsFromDatabaseAsync();
            }
            _logger.LogInformation($"Database is complete and correct!");

            _logger.LogInformation($"Validating parameters...");
            var (IsValid, ErrorMessage) = Validators.ValidateGetTagsParameters(sortBy, direction, page, pageSize, tags);
            if (!IsValid)
            {
                string error = $"Validation has FAILED! Error: {ErrorMessage}";
                _logger.LogError(error);
                return Problem(error);
            }
            _logger.LogInformation("Validation has been PASSED!");

            // Sorting based on the provided sortBy and direction parameters
            tags = sortBy.ToLower() switch
            {
                "id" => direction.Equals("asc", StringComparison.CurrentCultureIgnoreCase) ? tags.OrderBy(t => t.Id) : tags.OrderByDescending(t => t.Id),
                "name" => direction.Equals("asc", StringComparison.CurrentCultureIgnoreCase) ? tags.OrderBy(t => t.Name) : tags.OrderByDescending(t => t.Name),
                "percentage" => direction.Equals("asc", StringComparison.CurrentCultureIgnoreCase) ? tags.OrderBy(t => t.ProcentItHasInSumOfAllTagsCount) : tags.OrderByDescending(t => t.ProcentItHasInSumOfAllTagsCount),
                "count" => direction.Equals("asc", StringComparison.CurrentCultureIgnoreCase) ? tags.OrderBy(t => t.Count) : tags.OrderByDescending(t => t.Count),
                _ => tags // Default no sorting if no valid sortBy parameter provided
            };

            // Paginating the sorted tags
            IEnumerable<Tag> paginatedTags = tags.Skip((page - 1) * pageSize).Take(pageSize);
            _logger.LogDebug($"Returning {paginatedTags.Count()} tags after pagination and sorting.");
            return Ok(paginatedTags);
        }

        /// <summary>
        /// Initiates a refresh of the tags by fetching updated data from Stack Overflow's API.
        /// </summary>
        /// <returns>An IActionResult indicating the success or failure of the refresh operation.</returns>
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            _logger.LogInformation("Received request to refresh tags.");
            await _service.RefreshDatabaseAsync();
            string message = Flags.IsFetchingTagsFromSOCompletedSuccessfully ? "Tags have been refreshed." : "Request failed. Please verify your connection and ensure the Stack Overflow API's rate limit has not been exceeded.";
            _logger.LogDebug(message);
            return Flags.IsFetchingTagsFromSOCompletedSuccessfully ? Ok(message) : Problem(message);
        }
    }
}