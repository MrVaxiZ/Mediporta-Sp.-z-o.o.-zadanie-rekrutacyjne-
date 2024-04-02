using Microsoft.EntityFrameworkCore;
using StackOverflowAPI_Handler.Properties;

namespace StackOverflowAPI_Handler.Data
{
    /// <summary>
    /// Application database context, managing the connection to the database and acting as a session with the underlying database.
    /// It includes configurations for entities and their relationships, and provides access to entity sets.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Gets or sets the DbSet of Tags in the context. Represents a collection of tags that can be queried from the database.
        /// </summary>
        public DbSet<Tag> Tags { get; set; }

        /// <summary>
        /// Initializes a new instance of the ApplicationDbContext with the specified DbContextOptions.
        /// DbContextOptions carries configuration information such as the connection string, database provider to use, etc.
        /// </summary>
        /// <param name="options">The options for this context.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
    }
}
