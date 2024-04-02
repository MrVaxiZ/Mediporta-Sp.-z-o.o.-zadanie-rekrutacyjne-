using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq.Protected;
using Moq;
using StackOverflowAPI_Handler.Classes;
using StackOverflowAPI_Handler.Data;
using System.Net;
using Xunit;
using StackOverflowAPI_Handler.Properties;

namespace StackOverflowAPI_Handler.Tests
{
    public class StackOverflowServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly StackOverflowService _service;
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly HttpClient _mockHttpClient;
        private readonly Mock<ILogger<StackOverflowService>> _mockLogger;

        public StackOverflowServiceTests()
        {
            // Setup DbContext
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            _context = new ApplicationDbContext(options);

            // Setup Mock Logger
            _mockLogger = new Mock<ILogger<StackOverflowService>>();

            // Setup Mock HttpClient
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"items\": [{\"name\": \"js\", \"count\": 100}]}"),
                });

            _mockHttpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://api.stackexchange.com/2.3/")
            };

            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(_mockHttpClient);

            // Instantiate the service with the mock setup
            _service = new StackOverflowService(_context, _mockHttpClientFactory.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task FetchTagsFromStackOverflowAsync_ShouldAddTagsToDatabase()
        {
            // Act
            await _service.FetchTagsFromStackOverflowAsync();

            // Assert
            Assert.True(_context.Tags.Any(), "Database is empty!");

            var tag = await _context.Tags.FirstOrDefaultAsync();
            Assert.NotNull(tag);
            Assert.Equal("js", tag.Name);
            Assert.Equal(100, tag.Count);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            _mockHttpClient.Dispose();
        }
    }
}