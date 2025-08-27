# Spider IoT Platform - Domain-Driven Design (DDD) Architecture

This document describes the complete transformation of the Spider IoT platform to follow Domain-Driven Design (DDD) principles with SOLID architecture patterns.

## 🎯 Overview

The Spider IoT platform has been modernized from a legacy Windows-only WPF application into a cloud-native, cross-platform IoT data acquisition system following DDD principles. The new architecture provides:

- **Clean Architecture** with clear separation of concerns
- **Domain-Driven Design** with bounded contexts
- **CQRS and Event Sourcing** for scalable data operations
- **Microservices Architecture** with independent bounded contexts
- **Cross-platform compatibility** (Windows, Linux, macOS)
- **Container-ready deployment** with Docker support

## 🏗️ Architecture Overview

### Core Principles

1. **Domain-Driven Design (DDD)**: Business logic is encapsulated in domain models within bounded contexts
2. **SOLID Principles**: Each component follows single responsibility, open/closed, Liskov substitution, interface segregation, and dependency inversion
3. **Clean Architecture**: Dependencies flow inward toward the domain, with external concerns isolated in infrastructure layers
4. **CQRS**: Command Query Responsibility Segregation separates read and write operations
5. **Event Sourcing**: Domain events capture state changes for audit and integration

### Solution Structure

```
Spider.IoT/
├── src/
│   ├── Core/
│   │   ├── Spider.Core.SharedKernel/     # Domain abstractions and base classes
│   │   └── Spider.Core.Application/      # Application-level cross-cutting concerns
│   │
│   └── BoundedContexts/
│       ├── DeviceManagement/             # Device lifecycle and configuration
│       │   ├── Spider.DeviceManagement.Domain/
│       │   ├── Spider.DeviceManagement.Application/
│       │   ├── Spider.DeviceManagement.Infrastructure/
│       │   └── Spider.DeviceManagement.API/
│       │
│       ├── DataAcquisition/              # Real-time data collection
│       ├── ConnectionManagement/         # Protocol connections and drivers
│       ├── ProtocolManagement/           # Protocol definitions and adapters
│       ├── ProjectManagement/            # Project organization and configuration
│       └── MonitoringAndAlerting/        # System monitoring and notifications
│
└── tests/
    ├── Unit/
    ├── Integration/
    └── E2E/
```

## 🔧 Core Components

### SharedKernel

The `Spider.Core.SharedKernel` provides fundamental building blocks:

**Domain Abstractions:**
- `IDomainEvent` - For domain event publishing
- `IRepository<T,TId>` - Generic repository pattern
- `IUnitOfWork` - Transaction management
- `ISpecification<T>` - Query specification pattern

**Base Classes:**
- `Entity<TId>` - Base entity with domain events
- `AggregateRoot<TId>` - Domain aggregate root
- `ValueObject` - Immutable value objects
- `Enumeration` - Smart enum implementation

**Example Usage:**
```csharp
public class Device : AggregateRoot<Guid>
{
    public void ChangeStatus(DeviceStatus newStatus, string? reason = null)
    {
        if (Status != newStatus)
        {
            var previousStatus = Status;
            Status = newStatus;
            
            // Domain event is automatically tracked
            PublishDomainEvent(new DeviceStatusChangedEvent(Id, previousStatus.Name, newStatus.Name, reason));
        }
    }
}
```

### Application Layer

The `Spider.Core.Application` provides:

**CQRS Interfaces:**
```csharp
public interface ICommand<out TResponse> : IRequest<TResponse> { }
public interface IQuery<out TResponse> : IRequest<TResponse> { }
```

**Pipeline Behaviors:**
- `LoggingBehavior<TRequest,TResponse>` - Request/response logging with performance metrics
- `ValidationBehavior<TRequest,TResponse>` - FluentValidation integration

**Cross-cutting Services:**
- `ICacheService` - Distributed caching abstraction
- `ICurrentUserService` - User context and security

## 📋 Bounded Contexts

### DeviceManagement (Implemented)

**Domain Entities:**
- `Device` - IoT device aggregate root
- `DeviceStatus` - Connection status enumeration
- `ProtocolType` - Communication protocol enumeration
- `ConnectionParameters` - Connection configuration value object

**Key Features:**
- Device lifecycle management (create, configure, enable/disable)
- Protocol-agnostic connection parameters
- Domain events for status changes and configuration updates
- Validation and business rule enforcement

**API Endpoints:**
```bash
# Create a new device
POST /api/devices
{
  "name": "Modbus Device 1",
  "protocol": "Modbus",
  "host": "192.168.1.100",
  "port": 502,
  "projectId": "guid"
}

# Get devices by project
GET /api/devices/project/{projectId}

# Get supported protocols
GET /api/devices/protocols

# Health check
GET /api/devices/health
```

**Domain Events:**
- `DeviceCreatedEvent` - When a device is registered
- `DeviceStatusChangedEvent` - When connection status changes

### DataAcquisition (Planned)

**Responsibilities:**
- Real-time data collection from devices
- Data transformation and normalization
- Quality assessment and validation
- Historical data storage

**Key Entities:**
- `DataPoint` - Individual sensor reading
- `DataStream` - Continuous data flow from a device
- `DataQuality` - Data validation and quality metrics
- `DataBuffer` - Temporary storage for batching

### ConnectionManagement (Planned)

**Responsibilities:**
- Physical connection management
- Driver lifecycle and configuration
- Connection pooling and retry logic
- Network discovery and auto-configuration

**Key Entities:**
- `Connection` - Physical device connection
- `Driver` - Protocol-specific communication driver
- `ConnectionPool` - Managed connection resources
- `NetworkEndpoint` - Network communication point

### ProtocolManagement (Planned)

**Responsibilities:**
- Protocol definition and configuration
- Message parsing and serialization
- Protocol adaptation and translation
- Custom protocol support

### ProjectManagement (Planned)

**Responsibilities:**
- Project organization and hierarchy
- User access and permissions
- Configuration management
- Deployment coordination

### MonitoringAndAlerting (Planned)

**Responsibilities:**
- System health monitoring
- Performance metrics collection
- Alert configuration and notification
- Diagnostic data collection

## 🚀 Getting Started

### Prerequisites

- .NET 8.0 SDK
- Docker (optional, for containerized deployment)

### Build and Run

1. **Clone the repository:**
   ```bash
   git clone https://github.com/rauldose/Spider.git
   cd Spider
   ```

2. **Build the DDD solution:**
   ```bash
   dotnet build Spider.IoT.DDD.sln
   ```

3. **Run the Device Management API:**
   ```bash
   cd src/BoundedContexts/DeviceManagement/Spider.DeviceManagement.API
   dotnet run --urls="http://localhost:5000"
   ```

4. **Test the API:**
   ```bash
   # Health check
   curl http://localhost:5000/api/devices/health
   
   # Get available protocols
   curl http://localhost:5000/api/devices/protocols
   
   # Create a device
   curl -X POST http://localhost:5000/api/devices \
     -H "Content-Type: application/json" \
     -d '{
       "name": "Test Device",
       "protocol": "Modbus",
       "host": "localhost",
       "port": 502,
       "projectId": "12345678-1234-5678-9012-123456789012"
     }'
   ```

### Development

Each bounded context can be developed and deployed independently:

```bash
# Device Management
cd src/BoundedContexts/DeviceManagement/Spider.DeviceManagement.API
dotnet watch run

# Future bounded contexts will follow the same pattern
```

## 🧪 Testing Strategy

### Unit Tests
- Domain logic validation
- Business rule enforcement
- Value object behavior
- Event publishing verification

### Integration Tests
- Database operations
- API endpoint validation
- Cross-boundary communication
- Event handling workflows

### End-to-End Tests
- Complete workflow validation
- Performance benchmarking
- Security verification
- Deployment validation

## 📈 Benefits of DDD Architecture

### Maintainability
- **Clear Boundaries**: Each bounded context has well-defined responsibilities
- **Independent Evolution**: Contexts can evolve without affecting others
- **Technology Flexibility**: Different contexts can use different technologies

### Scalability
- **Horizontal Scaling**: Each context can be scaled independently
- **Resource Optimization**: Resources allocated based on context needs
- **Performance Isolation**: Issues in one context don't affect others

### Testability
- **Unit Testing**: Domain logic is isolated and easily testable
- **Integration Testing**: Clear interfaces enable focused integration tests
- **Mocking**: Dependencies are abstracted for easy mocking

### Team Organization
- **Domain Expertise**: Teams can specialize in specific business domains
- **Parallel Development**: Multiple teams can work simultaneously
- **Reduced Coordination**: Clear boundaries reduce inter-team dependencies

## 🔄 Migration Strategy

### Phase 1: Foundation (Completed)
- [x] Core SharedKernel implementation
- [x] Core Application layer
- [x] DeviceManagement bounded context
- [x] Basic API and testing

### Phase 2: Core Functionality
- [ ] DataAcquisition bounded context
- [ ] ConnectionManagement bounded context
- [ ] Basic protocol drivers (Modbus, OPC UA, MQTT)

### Phase 3: Advanced Features
- [ ] ProtocolManagement bounded context
- [ ] ProjectManagement bounded context
- [ ] MonitoringAndAlerting bounded context

### Phase 4: Enterprise Features
- [ ] Event sourcing implementation
- [ ] Advanced caching and performance optimization
- [ ] Multi-tenant support
- [ ] Advanced security and authentication

### Phase 5: Cloud Native
- [ ] Kubernetes deployment
- [ ] Service mesh integration
- [ ] Advanced monitoring and observability
- [ ] Auto-scaling and resilience patterns

## 🤝 Contributing

1. Follow DDD principles and maintain bounded context boundaries
2. Write comprehensive unit tests for domain logic
3. Document domain concepts and business rules
4. Use consistent naming conventions across contexts
5. Implement proper error handling and logging

## 📚 Further Reading

- [Domain-Driven Design by Eric Evans](https://www.domainlanguage.com/ddd/)
- [Clean Architecture by Robert Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [.NET Microservices Architecture Guide](https://dotnet.microsoft.com/learn/aspnet/microservices-architecture)
- [CQRS Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs)

---

*This DDD transformation maintains full backward compatibility while providing a clear path to modern, cloud-native IoT data collection and management.*