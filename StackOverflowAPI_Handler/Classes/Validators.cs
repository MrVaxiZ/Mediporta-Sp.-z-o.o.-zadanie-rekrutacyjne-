using StackOverflowAPI_Handler.Properties;

namespace StackOverflowAPI_Handler.Classes
{
    public static class Validators
    {
        /// <summary>
        /// Validates pagination parameters, ensuring that the page and pageSize are positive
        /// and that the requested page exists based on the total number of items.
        /// </summary>
        /// <param name="page">The requested page number.</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="tags">The collection of tags to paginate.</param>
        /// <returns>true if the pagination parameters are valid; otherwise, false.</returns>
        private static bool ValidatePagination(int page, int pageSize, IEnumerable<Tag> tags)
        {
            if (page <= 0 || pageSize <= 0)
            {
                return false; // Page and pageSize must be positive.
            }

            int totalItems = tags.Count();

            if (pageSize > totalItems)
            {
                return false; // pageSize can't be bigger than amount of tags
            }

            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return page <= totalPages; // The requested page must not exceed the total number of pages.
        }

        /// <summary>
        /// Validates sort direction.
        /// </summary>
        /// <param name="direction">The sort direction to validate.</param>
        /// <returns>true if the direction is either "asc" or "desc"; otherwise, false.</returns>
        private static bool ValidateSortDirection(string direction)
        {
            if (string.IsNullOrEmpty(direction)) { return false; }
            return direction.Equals("asc", StringComparison.OrdinalIgnoreCase) ||
                   direction.Equals("desc", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Validates the sort by parameter for tags.
        /// </summary>
        /// <param name="sortBy">The field name to sort by.</param>
        /// <returns>true if the sortBy parameter is one of the predefined fields; otherwise, false.</returns>
        private static bool ValidateTagsSortBy(string sortBy)
        {
            if (string.IsNullOrEmpty(sortBy)) { return false; }
            var validSortFields = new HashSet<string> { "id", "name", "percentage", "count" };
            return validSortFields.Contains(sortBy.ToLower());
        }

        /// <summary>
        /// Validates all parameters for the TagsController.Get method.
        /// </summary>
        /// <param name="sortBy">The field to sort by.</param>
        /// <param name="direction">The direction of sorting.</param>
        /// <param name="page">The page number for pagination.</param>
        /// <param name="pageSize">The size of each page.</param>
        /// <returns>
        /// A tuple where Item1 indicates whether the validation succeeded, 
        /// and Item2 contains the error message if the validation failed.
        /// </returns>
        public static (bool IsValid, string ErrorMessage) ValidateGetTagsParameters(string sortBy, string direction, int page, int pageSize, IEnumerable<Tag> tags)
        {
            if (!ValidatePagination(page, pageSize, tags))
            {
                return (false, "Invalid page or pageSize parameters.");
            }

            if (!ValidateSortDirection(direction))
            {
                return (false, "Invalid sort direction parameter.");
            }

            if (!ValidateTagsSortBy(sortBy))
            {
                return (false, $"Invalid sortBy parameter: {sortBy}.");
            }

            return (true, string.Empty);
        }
    }
}