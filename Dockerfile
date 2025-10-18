# 🧱 Stage 1 — Build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet publish -c Release -o /app

# 🚀 Stage 2 — Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy from build stage
COPY --from=build /app .

# Environment variable for ASP.NET to listen on any address
ENV ASPNETCORE_URLS=http://+:8080

# Render / Fly.io inject PORT dynamically, ensure runtime uses it
ENV PORT=8080

# Expose port 8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "MyApp.dll"]
