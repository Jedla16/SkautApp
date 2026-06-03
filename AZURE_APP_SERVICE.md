# Azure App Service setup for SkautApp

## What to use

- App Service runtime: `.NET 8`
- OS: Windows or Linux
- Recommended database for production: Azure SQL
- Recommended storage for media: persistent App Service storage or Blob Storage

## App settings to configure in Azure

Set these in **App Service > Configuration > Application settings**:

- `ASPNETCORE_ENVIRONMENT=Production`
- `Umbraco__CMS__WebRouting__UmbracoApplicationUrl=https://your-domain.example`
- `Serilog__MinimumLevel__Default=Information`
- `Serilog__MinimumLevel__Override__Microsoft=Warning`
- `Serilog__MinimumLevel__Override__Microsoft.Hosting.Lifetime=Information`
- `Serilog__MinimumLevel__Override__System=Warning`

If you keep SQLite for a temporary first deploy, the data folder must be persistent.
For production, Azure SQL is the safer choice.

## Storage notes

Current project uses SQLite via `|DataDirectory|/Modry_zivot.sqlite.db`.

That means you need one of these approaches:

1. **Preferred:** move to Azure SQL and keep media on persistent storage.
2. **Temporary:** keep SQLite, but make sure the data directory is on persistent disk.

For Umbraco media files, make sure the upload folder stays persistent too.

## Deploy flow

```bash
dotnet publish -c Release -o ./publish
cd publish
zip -r ../app.zip .
cd ..
```

Then upload `app.zip` to App Service using ZIP deploy or the Azure Portal.

## Domain and HTTPS

1. Add custom domain in App Service.
2. Set DNS record at your domain provider.
3. Validate domain in Azure.
4. Enable managed certificate or upload your own certificate.
