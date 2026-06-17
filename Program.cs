using System.IO;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

var contentRoot = builder.Environment.ContentRootPath;
AppContext.SetData("DataDirectory", Path.Combine(contentRoot, "umbraco", "Data"));

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .Build();

WebApplication app = builder.Build();

// Tady byl ten zakopaný pes - Umbraco se musí nejdřív nabootovat
await app.BootUmbracoAsync();

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
