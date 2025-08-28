# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY Spider.sln .
COPY Common/Cdy.Spider.Common/Cdy.Spider.Common.csproj Common/Cdy.Spider.Common/
COPY ModernAPI/SpiderDriver.UnifiedAPI/SpiderDriver.UnifiedAPI.csproj ModernAPI/SpiderDriver.UnifiedAPI/
COPY ModernWeb/SpiderStudio.BlazorServer/SpiderStudio.BlazorServer.csproj ModernWeb/SpiderStudio.BlazorServer/

# Restore dependencies
RUN dotnet restore ModernWeb/SpiderStudio.BlazorServer/SpiderStudio.BlazorServer.csproj

# Copy source code
COPY Common/ Common/
COPY ModernAPI/ ModernAPI/
COPY ModernWeb/ ModernWeb/

# Build application
RUN dotnet build ModernWeb/SpiderStudio.BlazorServer/SpiderStudio.BlazorServer.csproj -c Release -o /app/build

# Publish application
RUN dotnet publish ModernWeb/SpiderStudio.BlazorServer/SpiderStudio.BlazorServer.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN groupadd -r spider && useradd -r -g spider spider

# Copy published application
COPY --from=build /app/publish .

# Set ownership
RUN chown -R spider:spider /app

# Switch to non-root user
USER spider

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "SpiderStudio.BlazorServer.dll"]