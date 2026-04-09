using KairusBot.Options;
using KairusBot.Services;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
if (string.IsNullOrWhiteSpace(port))
{
    port = "10000";
}

builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.Configure<VkOptions>(builder.Configuration.GetSection("Vk"));
builder.Services.AddHttpClient<VkApiService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddSingleton<InMemoryUserStateService>();
builder.Services.AddSingleton<RouteCatalogService>();
builder.Services.AddSingleton<InMemoryFavoritesService>();
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();