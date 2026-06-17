FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj and restore as distinct layers
COPY ["SkautApp.csproj", "./"]
RUN dotnet restore "SkautApp.csproj"

# copy everything and publish
COPY . .
RUN dotnet publish "SkautApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 80

COPY --from=build /app/publish .
COPY backup-db /seed-db
COPY docker-entrypoint.sh /docker-entrypoint.sh

# Ensure Umbraco's writable folders exist in the image so Docker volume initialization
# and first boot both see the expected directory structure.
RUN mkdir -p /app/umbraco/Data /app/wwwroot/media \
	&& chmod +x /docker-entrypoint.sh

USER root

ENTRYPOINT ["/docker-entrypoint.sh"]
