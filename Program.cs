using System.IO;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

var contentRoot = builder.Environment.ContentRootPath;
var dataDir = Path.Combine(contentRoot, "umbraco", "Data");
var mediaDir = Path.Combine(contentRoot, "wwwroot", "media");

Directory.CreateDirectory(dataDir);
Directory.CreateDirectory(mediaDir);

// Ensure |DataDirectory| resolves to the container/app data folder so SQLite path works reliably.
AppContext.SetData("DataDirectory", dataDir);

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .Build();

WebApplication app = builder.Build();

await app.BootUmbracoAsync();

// Simple health endpoint for container orchestration and load balancers
app.MapGet("/health", () => Results.Text("OK"));

app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseInstallerEndpoints();
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

await app.RunAsync();
