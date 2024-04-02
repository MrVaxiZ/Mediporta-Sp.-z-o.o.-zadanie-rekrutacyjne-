using StackOverflowAPI_Handler.Properties;
using StackOverflowAPI_Handler.Classes;
using Xunit;

namespace StackOverflowAPI_Handler.Tests
{
    public class Tests_Validators
    {
        [Fact]
        public void ValidateGetTagsParameters_ValidParameters_ReturnsTrue()
        {
            // Arrange
            var tags = GenerateTagsList(50);
            string sortBy = "name";
            string direction = "asc";
            int page = 1;
            int pageSize = 10;

            // Act
            var result = Validators.ValidateGetTagsParameters(sortBy, direction, page, pageSize, tags);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.ErrorMessage);
        }

        [Theory]
        [InlineData(0, 10)] // Page is 0
        [InlineData(-1, 10)] // Page is negative
        [InlineData(1, 0)] // PageSize is 0
        [InlineData(1, -1)] // PageSize is negative
        [InlineData(6, 10)] // Requesting a page number that doesn't exist
        [InlineData(null, null)] // Both nulls
        public void ValidateGetTagsParameters_InvalidPaginationParameters_ReturnsFalse(int page, int pageSize)
        {
            // Arrange
            var tags = GenerateTagsList(50);
            string sortBy = "name";
            string direction = "asc";

            // Act
            var result = Validators.ValidateGetTagsParameters(sortBy, direction, page, pageSize, tags);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Invalid page or pageSize parameters.", result.ErrorMessage);
        }

        [Theory]
        [InlineData("ascending")]
        [InlineData("descending")]
        [InlineData("")]
        [InlineData(null)]
        public void ValidateGetTagsParameters_InvalidSortDirection_ReturnsFalse(string direction)
        {
            // Arrange
            var tags = GenerateTagsList(50);
            string sortBy = "name";
            int page = 1;
            int pageSize = 10;

            // Act
            var result = Validators.ValidateGetTagsParameters(sortBy, direction, page, pageSize, tags);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Invalid sort direction parameter.", result.ErrorMessage);
        }

        [Theory]
        [InlineData("popularity")]
        [InlineData("")]
        [InlineData(null)]
        public void ValidateGetTagsParameters_InvalidSortBy_ReturnsFalse(string sortBy)
        {
            // Arrange
            var tags = GenerateTagsList(50);
            string direction = "asc";
            int page = 1;
            int pageSize = 10;

            // Act
            var result = Validators.ValidateGetTagsParameters(sortBy, direction, page, pageSize, tags);

            // Assert
            Assert.False(result.IsValid);
            Assert.StartsWith("Invalid sortBy parameter:", result.ErrorMessage);
        }

        [Fact]
        public void ValidateGetTagsParameters_LastPageExactlyFull_ReturnsTrue()
        {
            // Arrange
            var tags = GenerateTagsList(50);
            int page = 5;
            int pageSize = 10; // Exactly fills the last page

            // Act
            var result = Validators.ValidateGetTagsParameters("name", "asc", page, pageSize, tags);

            // Assert
            Assert.True(result.IsValid, "Pagination on the edge of being full should be valid.");
        }

        [Fact]
        public void ValidateGetTagsParameters_EmptyTagsList_ReturnsFalse()
        {
            // Arrange
            var tags = new List<Tag>(); // Empty list of tags
            int page = 1;
            int pageSize = 10;

            // Act
            var result = Validators.ValidateGetTagsParameters("name", "asc", page, pageSize, tags);

            // Assert
            Assert.False(result.IsValid, "Pagination with empty tags list should be invalid.");
            Assert.Equal("Invalid page or pageSize parameters.", result.ErrorMessage);
        }

        [Fact]
        public void ValidateGetTagsParameters_NullTags_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() =>
                Validators.ValidateGetTagsParameters("name", "asc", 1, 10, null));
        }

        [Theory]
        [InlineData(" ASC ")]
        [InlineData(" desc")]
        [InlineData("DESC")]
        public void ValidateGetTagsParameters_SortDirectionCaseInsensitiveAndTrimmed_ReturnsTrue(string direction)
        {
            // Arrange
            var tags = GenerateTagsList(10);
            string sortBy = "name";
            int page = 1;
            int pageSize = 5;

            // Act
            var result = Validators.ValidateGetTagsParameters(sortBy, direction.Trim(), page, pageSize, tags);

            // Assert
            Assert.True(result.IsValid, "Sort direction should be case-insensitive and trimmed.");
        }

        [Theory]
        [InlineData("nam")]
        [InlineData(" name ")]
        public void ValidateGetTagsParameters_InvalidSortByField_ReturnsFalse(string sortBy)
        {
            // Arrange
            var tags = GenerateTagsList(10);
            string direction = "asc";
            int page = 1;
            int pageSize = 5;

            // Act
            var result = Validators.ValidateGetTagsParameters(sortBy, direction, page, pageSize, tags);

            // Assert
            Assert.False(result.IsValid, $"SortBy field '{sortBy}' should be considered invalid.");
            Assert.StartsWith("Invalid sortBy parameter:", result.ErrorMessage);
        }

        // Helper method to generate a list of Tag objects
        private IEnumerable<Tag> GenerateTagsList(int count)
        {
            return Enumerable.Range(1, count).Select(i => new Tag
            {
                Id = i,
                Name = $"Tag{i}",
                Count = i,
                ProcentItHasInSumOfAllTagsCount = i * 0.1f
            }).ToList();
        }

    }
}
