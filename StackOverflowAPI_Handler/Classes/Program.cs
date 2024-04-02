using NLog.Web;
using Microsoft.EntityFrameworkCore;
using StackOverflowAPI_Handler.Data;
using StackOverflowAPI_Handler.Classes;
using System.Reflection;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// NLog
builder.Logging.ClearProviders();
builder.Host.UseNLog();

// Add services to the container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddHttpClient("StackOverflowClient", c =>
{
    c.BaseAddress = new Uri("https://api.stackexchange.com/2.3/");
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
});
builder.Services.AddScoped<StackOverflowService>();

builder.Services.AddEndpointsApiExplorer();

// Configure Swagger to use XML comments
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

try
{
    app.Logger.LogInformation("Starting application");
    app.Run();
}
catch (Exception exception)
{
    app.Logger.LogError(exception, "Application start-up failed");
}
finally
{
    NLog.LogManager.Shutdown();
}
