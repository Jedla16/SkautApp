# Azure App Service deployment checklist

## Před nasazením

- [ ] Rozhodnout produkční doménu
- [ ] Založit Azure App Service
- [ ] Připravit produkční databázi
- [ ] Připravit trvalé úložiště pro media / data
- [ ] Nastavit `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Nastavit `Umbraco__CMS__WebRouting__UmbracoApplicationUrl`

## Publish

- [ ] Spustit `dotnet publish -c Release -o ./publish`
- [ ] Zabalit výstup do ZIP
- [ ] Nahrát ZIP do Azure App Service

## Po nasazení

- [ ] Ověřit homepage
- [ ] Ověřit login
- [ ] Ověřit nahrávání medií
- [ ] Připojit vlastní doménu
- [ ] Zapnout HTTPS
- [ ] Nastavit monitoring / logování
