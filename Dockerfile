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

# Ensure umbraco data path exists (will be populated by bind mount in compose)
RUN mkdir -p umbraco/Data && mkdir -p wwwroot || true

ENTRYPOINT ["dotnet", "SkautApp.dll"]
