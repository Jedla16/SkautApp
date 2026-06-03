# Deployment notes

Krátké kroky pro kontejnerizaci a nasazení na VPS / Coolify.

1) Lokální build & run

```bash
docker build -t skautapp:latest .
docker-compose up -d --build
```

Po spuštění bude aplikace dostupná na portu 80.

2) Multi-arch image (pokud chcete pushnout do registru a nasadit z Coolify)

```bash
# povolit buildx a emulaci (pokud potřebujete build pro jiné architektury)
docker buildx create --use --name multi
docker buildx build --platform linux/amd64,linux/arm64 -t <registry>/skautapp:latest --push .
```

3) Volumes a persistované soubory
- `./umbraco/Data` musí být perzistentní (obsahuje SQLite DB, logy, media).
- `./wwwroot` obsahuje veřejné soubory/media.

4) Coolify / deployment tips
- Coolify může nasadit z Git repozitáře a spouštět Docker Compose. V Coolify zadejte build krok (pokud pushujete multi-arch image, můžete nasazovat přímo obraz z registry).
- V Coolify nastavte env var `ASPNETCORE_ENVIRONMENT=Production` a případné `ConnectionStrings__umbracoDbDSN` pokud chcete přepsat umístění DB.

5) Architektury
- Oracle free tier má ARM i x86 servery; obraz `mcr.microsoft.com/dotnet/aspnet:8.0` má multi-arch manifest. Pro spolehlivé nasazení na ARM použijte `docker buildx build --platform linux/arm64` nebo build přímo na cílovém ARM VPS.
