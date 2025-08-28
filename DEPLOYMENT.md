# Spider Platform - Modern Deployment Guide

## Overview

This guide covers the deployment of the modernized Spider IoT platform using containerization and modern infrastructure practices.

## Architecture

The modern Spider platform consists of:

### Core Services
- **Spider Studio (Blazor Server)**: Modern web-based development interface
- **Spider Runtime**: IoT data collection engine (legacy runtime or modern implementation)
- **Unified Driver API**: Common interface for all industrial protocol drivers

### Infrastructure Services
- **Redis**: Caching and real-time data distribution
- **InfluxDB**: Time-series database for IoT data storage
- **Grafana**: Data visualization and monitoring dashboards
- **MQTT Broker**: IoT messaging and communication
- **Nginx**: Reverse proxy and load balancer

## Quick Start

### Prerequisites
- Docker & Docker Compose
- .NET 8 SDK (for development)
- Modern web browser

### Deploy with Docker Compose

1. **Clone the repository**
   ```bash
   git clone https://github.com/rauldose/Spider.git
   cd Spider
   ```

2. **Start all services**
   ```bash
   docker-compose up -d
   ```

3. **Access the platform**
   - Spider Studio: http://localhost
   - Grafana: http://localhost/grafana (admin/spider123)
   - InfluxDB: http://localhost:8086 (spider/spider123)

### Development Environment

1. **Install dependencies**
   ```bash
   dotnet restore
   ```

2. **Run Blazor Studio locally**
   ```bash
   cd ModernWeb/SpiderStudio.BlazorServer
   dotnet run
   ```

3. **Access at** http://localhost:5000

## Configuration

### Environment Variables

#### Spider Studio
- `ASPNETCORE_ENVIRONMENT`: Development/Production
- `ASPNETCORE_URLS`: Binding URLs
- `ConnectionStrings__Redis`: Redis connection string
- `ConnectionStrings__InfluxDB`: InfluxDB connection string

#### Infrastructure Services
- `DOCKER_INFLUXDB_INIT_*`: InfluxDB initialization
- `GF_SECURITY_ADMIN_PASSWORD`: Grafana admin password

### Volume Mounts
- `spider-data`: Application data and configurations
- `influxdb-data`: Time-series data storage  
- `grafana-data`: Dashboards and settings
- `mosquitto-data`: MQTT broker persistence

## Modern Features

### Unified Driver Interface
```csharp
public interface IUnifiedDriver
{
    Task<bool> ConnectAsync(ConnectionParameters parameters);
    Task<ReadResult> ReadAsync(ReadRequest request);
    Task<WriteResult> WriteAsync(WriteRequest request);
    Task<IDataSubscription> SubscribeAsync(SubscriptionRequest request);
}
```

### Protocol Support
- **Legacy**: All existing Spider drivers via adapter pattern
- **Modern**: New unified implementations with async/await
- **Protocols**: Modbus, OPC UA, MQTT, Siemens, Allen Bradley, etc.

### Cross-Platform Deployment
- **Containers**: Docker support for all platforms
- **Cloud**: Kubernetes manifests available
- **Edge**: ARM64 support for edge devices

## Monitoring & Observability

### Health Checks
- Application health: `/health`
- Service discovery via Docker health checks
- Grafana dashboards for system monitoring

### Logging
- Structured logging with Serilog
- Centralized log aggregation
- Performance monitoring and alerting

## Security

### Container Security
- Non-root user execution
- Minimal base images (Alpine Linux)
- Security scanning integration

### Network Security
- Internal Docker network isolation
- TLS/SSL termination at proxy
- Environment-based secrets management

## Scaling

### Horizontal Scaling
- Multiple Spider Studio instances behind load balancer
- Redis for shared session state
- Database connection pooling

### Performance Optimization
- Blazor Server with SignalR for real-time updates
- Efficient data serialization
- Connection multiplexing for drivers

## Migration Guide

### From Legacy WPF to Modern Blazor

1. **Assess Current Drivers**
   - Identify custom protocols and configurations
   - Map to unified driver interface

2. **Data Migration**
   - Export existing configurations
   - Import to modern format via migration tools

3. **Gradual Migration**
   - Run legacy and modern systems in parallel
   - Migrate drivers incrementally using adapter pattern
   - Full cutover after validation

### Legacy Driver Integration
```csharp
// Wrap existing drivers with unified interface
var legacyDriver = driverFactory.GetDevelopInstance("Modbus");
var unifiedDriver = new LegacyDriverAdapter(legacyDriver);
driverManager.AddDriver(unifiedDriver);
```

## Troubleshooting

### Common Issues

1. **Container startup failures**
   - Check Docker logs: `docker-compose logs service-name`
   - Verify port availability
   - Check file permissions

2. **Driver connectivity issues**
   - Verify network configuration
   - Check firewall settings
   - Validate device IP addresses

3. **Performance problems**
   - Monitor container resources
   - Check database connections
   - Review driver polling intervals

### Diagnostic Commands
```bash
# Check service status
docker-compose ps

# View logs
docker-compose logs spider-studio

# Connect to container
docker-compose exec spider-studio bash

# Monitor resources
docker stats
```

## Support

- **Documentation**: `/docs` folder
- **Issues**: GitHub Issues
- **Community**: Spider Platform Discord/Forums
- **Enterprise**: Commercial support available

## Next Steps

- Configure industrial device connections
- Set up monitoring dashboards
- Implement custom drivers using unified interface
- Scale deployment for production workloads