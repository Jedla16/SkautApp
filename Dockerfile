# 1. Fáze sestavení
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Kopírování a obnova závislostí
COPY ["SkautApp.csproj", "./"]
RUN dotnet restore "SkautApp.csproj"

# Kopírování zbytku kódu a publikace
COPY . .
RUN dotnet publish "SkautApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Fáze spuštění (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Příprava složky pro trvalá data Umbraca
RUN mkdir -p /app/umbraco/Data /app/wwwroot/media

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "SkautApp.dll"]