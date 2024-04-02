namespace StackOverflowAPI_Handler.Properties
{
    /// <summary>
    /// Represents a tag entity as used in Stack Overflow. 
    /// Tags are keywords or labels that categorize questions with similar topics.
    /// </summary>
    public class Tag
    {
        /// <summary>
        /// Gets or sets the identifier for the tag.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the count of how many times the tag has been applied to questions.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the percentage of the tag count in relation to the sum of all tag counts.
        /// </summary>
        public float ProcentItHasInSumOfAllTagsCount { get; set; }

        /// <summary>
        /// Gets or sets the name of the tag.
        /// </summary>
        public string? Name { get; set; }
    }
}
