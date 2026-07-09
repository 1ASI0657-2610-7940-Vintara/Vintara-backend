# Análisis Arquitectónico — Vintara Backend (WineSoft Platform)

> **Scope:** rama `develop`, revisión de julio 2026  
> **Fuente:** inspección directa de `src/`, `docker-compose.yml` y `.github/workflows/ci.yml`

---

## SECCIÓN 1 — ESTILO ARQUITECTÓNICO

### Veredicto: Arquitectura de Microservicios con API Gateway centralizado

El sistema está compuesto por **8 proyectos independientes** bajo `src/`, cada uno con su propio `Dockerfile`, base de datos MySQL separada, y proceso ASP.NET Core autónomo:

| Servicio | Proyecto |
|---|---|
| API Gateway | `src/GatewayService/WinesoftPlatform.GatewayService` |
| Autenticación | `src/AuthService/WinesoftPlatform.AuthService` |
| Inventario | `src/InventoryService/WinesoftPlatform.InventoryService` |
| Compras | `src/PurchaseService/WinesoftPlatform.PurchaseService` |
| Perfiles | `src/ProfilesService/WinesoftPlatform.ProfilesService` |
| Analítica | `src/AnalyticsService/WinesoftPlatform.AnalyticsService` |
| Simulador IoT | `src/IoTSimulatorService/WinesoftPlatform.IoTSimulatorService` |
| Shared Library | `src/Shared/WinesoftPlatform.Shared` |

**Evidencia directa en `docker-compose.yml`:**
```yaml
# Cada servicio es un contenedor independiente con su propia db:
services:
  auth-service:
    ports: ["5001:8080"]
    environment:
      - ConnectionStrings__DefaultConnection=...database=winesoft_auth...
  inventory-service:
    ports: ["5002:8080"]
    environment:
      - ConnectionStrings__DefaultConnection=...database=winesoft_inventory...
  purchase-service:
    ports: ["5003:8080"]
    environment:
      - ConnectionStrings__DefaultConnection=...database=winesoft_purchase...
```

Cada microservicio tiene **su propia base de datos MySQL aislada** (`winesoft_auth`, `winesoft_inventory`, `winesoft_purchase`, etc.), lo cual es el sello distintivo del estilo microservicios: _database per service_. El acceso externo está unificado por YARP en `GatewayService` (puerto 5000).

---

## SECCIÓN 2 — ESTILO DE COMUNICACIÓN

El sistema usa **dos mecanismos de comunicación coexistentes**, aplicados según el tipo de operación:

### Mecanismo 1: REST Síncrono (HTTP)

Usado para consultas de datos en tiempo real que requieren respuesta inmediata.

| Origen | Destino | Propósito | Archivo |
|---|---|---|---|
| `PurchaseService` | `InventoryService` | Obtener nombre de un insumo por ID al crear/actualizar una orden | `src/PurchaseService/.../Infrastructure/ExternalServices/InventoryServiceClient.cs` |
| `AnalyticsService` | `InventoryService` | Obtener lista de suministros para cálculo de métricas | `src/AnalyticsService/.../Infrastructure/ExternalServices/ServiceClients.cs` — `InventoryServiceClient` |
| `AnalyticsService` | `PurchaseService` | Obtener órdenes de compra para reportes | `src/AnalyticsService/.../Infrastructure/ExternalServices/ServiceClients.cs` — `PurchaseServiceClient` |
| `IoTSimulatorService` | `InventoryService` | Enviar telemetría de sensores | `src/IoTSimulatorService/.../Application/Engine/SimulationEngine.cs` |
| `IoTSimulatorService` | `AuthService` | Obtener token JWT de servicio | `src/IoTSimulatorService/.../Application/Engine/SimulationEngine.cs` |

**Evidencia en `InventoryServiceClient.cs` (PurchaseService):**
```csharp
var response = await _httpClient.GetAsync($"/api/v1/inventory/supplies/{supplyId}");
```

**Evidencia en `ServiceClients.cs` (AnalyticsService):**
```csharp
var response = await _httpClient.GetAsync("/api/v1/inventory/supplies");
var response = await _httpClient.GetAsync("/api/v1/purchase-orders");
```

### Mecanismo 2: Mensajería Asíncrona (RabbitMQ + MassTransit)

Usado para operaciones de escritura donde el emisor no necesita esperar al receptor.

| Evento | Publicador | Consumidores | Archivo del publicador | Archivo del consumidor |
|---|---|---|---|---|
| `OrderCreated` | `PurchaseService` | `InventoryService` | `OrderCommandService.cs` | `InventoryService/.../Consumers/OrderCreatedConsumer.cs` |
| `SupplyStockChanged` | `InventoryService` | `AnalyticsService` | `InventorySubject.cs` | `AnalyticsService/.../Consumers/SupplyStockChangedConsumer.cs` |

**Configuración de colas en `InventoryService/Program.cs`:**
```csharp
cfg.ReceiveEndpoint("order-created-queue", e => {
    e.ConfigureConsumer<OrderCreatedConsumer>(context);
});
```

**Flujo combinado de una compra:**
```
POST /api/purchases
  → PurchaseService: HTTP GET → InventoryService (nombre del insumo, síncrono)
  → PurchaseService: guarda Order en MySQL
  → PurchaseService: publica OrderCreated → RabbitMQ (asíncrono, sin esperar)
  → InventoryService consume OrderCreated → descuenta stock
  → InventoryService publica SupplyStockChanged → RabbitMQ
  → AnalyticsService consume SupplyStockChanged → invalida caché Redis
```

---

## SECCIÓN 3 — PATRONES ARQUITECTÓNICOS

### 1. API Gateway (YARP)

| Campo | Detalle |
|---|---|
| **Servicio** | `GatewayService` |
| **Archivo** | `src/GatewayService/.../Program.cs`, `appsettings.json` |
| **Problema que resuelve** | Ocultar la topología interna de puertos (5001-5006) al frontend. Centralizar CORS y Rate Limiting en un único punto de entrada (puerto 5000) |
| **Implementación** | `builder.Services.AddReverseProxy().LoadFromConfig(...)` con routes/clusters en `appsettings.json` |

### 2. CQRS (Command Query Responsibility Segregation)

| Campo | Detalle |
|---|---|
| **Servicios** | `InventoryService`, `PurchaseService`, `ProfilesService`, `AnalyticsService` |
| **Archivos** | `Application/Internal/CommandServices/`, `Application/Internal/QueryServices/`, `Domain/Model/Commands/`, `Domain/Model/Queries/` |
| **Problema que resuelve** | Separar responsabilidades de escritura (Commands) y lectura (Queries) para favorecer la modificabilidad y el testeo independiente |
| **Evidencia** | `CreateOrderCommand.cs`, `GetAllOrdersQuery.cs`, `OrderCommandService.cs`, `OrderQueryService.cs` en PurchaseService |

### 3. Resilience / Circuit Breaker (Polly via AddStandardResilienceHandler)

| Campo | Detalle |
|---|---|
| **Servicio** | `PurchaseService` |
| **Archivo** | `src/PurchaseService/.../Program.cs` (línea 55-59) |
| **Problema que resuelve** | Evitar que un fallo temporal de `InventoryService` bloquee indefinidamente a `PurchaseService` (caídas en cascada) |
| **Implementación** | `.AddStandardResilienceHandler(options => { options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(15); })` |

### 4. Publisher/Subscriber (Pub/Sub vía MassTransit + RabbitMQ)

| Campo | Detalle |
|---|---|
| **Servicios** | `PurchaseService` (publisher), `InventoryService` y `AnalyticsService` (subscribers) |
| **Archivos** | `OrderCommandService.cs`, `InventorySubject.cs`, `OrderCreatedConsumer.cs`, `SupplyStockChangedConsumer.cs` |
| **Problema que resuelve** | Desacoplar la creación de una orden del descuento de stock y la invalidación de caché. El emisor no sabe quién recibe el evento |

### 5. Observer (GoF — aplicado localmente en InventoryService)

| Campo | Detalle |
|---|---|
| **Servicio** | `InventoryService` |
| **Archivos** | `Domain/Services/IInventoryObserver.cs`, `IInventorySubject.cs`, `AlertEngine.cs`, `Application/Internal/CommandServices/InventorySubject.cs` |
| **Problema que resuelve** | Detectar anomalías de sensores IoT y caídas de stock de forma extensible. Nuevos observadores (ej: notificaciones por email) se añaden sin modificar la lógica de `SupplyCommandService` |
| **Registro en DI** | `Program.cs` líneas 149-150: `builder.Services.AddScoped<IInventorySubject, InventorySubject>()` y `builder.Services.AddScoped<IInventoryObserver, AlertEngine>()` |

### 6. Repository

| Campo | Detalle |
|---|---|
| **Todos los servicios** | `InventoryService`, `PurchaseService`, `ProfilesService`, `AnalyticsService`, `AuthService` |
| **Archivos** | Interfaces en `Domain/Repositories/` (ej. `ISupplyRepository.cs`), implementaciones en `Infrastructure/Persistence/Repositories/` (ej. `SupplyRepository.cs`) |
| **Base común** | `src/Shared/.../Infrastructure/Persistence/EFC/Repositories/` contiene `BaseRepository<T>` y `UnitOfWork` reutilizados por todos |
| **Problema que resuelve** | Abstraer las consultas de base de datos del dominio. La capa de aplicación depende de interfaces, no de EF Core directamente |

### 7. Aggregate Root (DDD — ver Sección 5)

Los aggregates actúan como fábricas internas: reciben Commands en el constructor y encapsulan la lógica de construcción, impidiendo la creación de entidades en estados inválidos. Ejemplo: `Supply(CreateSupplyCommand command)` en `Supply.cs`.

### 8. Saga / Proceso distribuido (parcial — sin orquestador formal)

El flujo de compra (Order → descuento stock → invalidación caché) constituye una saga implícita coordinada por eventos:
1. `PurchaseService` publica `OrderCreated`
2. `InventoryService` consume y publica `SupplyStockChanged`
3. `AnalyticsService` consume e invalida caché

**No existe un coordinador Saga explícito** (como saga state machine en MassTransit). Es una **coreografía** (cada servicio reacciona a eventos sin orquestador central), lo que representa una limitación identificada en la Sección 9.

---

## SECCIÓN 4 — ESTRUCTURA INTERNA DE MICROSERVICIOS

### Patrón predominante: Clean Architecture (con influencia de DDD)

Todos los microservicios con dominio propio (`InventoryService`, `PurchaseService`, `ProfilesService`, `AnalyticsService`) siguen la misma estructura de 4 capas con dependencias apuntando hacia adentro:

```
Interfaces/ (REST Controllers, Resources, Transforms)
    ↓ depende de
Application/ (CommandServices, QueryServices, Consumers)
    ↓ depende de
Domain/ (Model/Aggregates, Commands, Queries, Repositories [interfaces], Services [interfaces])
    ↑ no depende de nadie externo al dominio
Infrastructure/ (Persistence/EFC, ExternalServices)
    ↓ implementa las interfaces del Domain
```

**Evidencia de la estructura en InventoryService:**
```
Domain/Model/Aggregates/Supply.cs          ← Aggregate Root
Domain/Model/Commands/CreateSupplyCommand.cs
Domain/Model/Queries/GetAllSuppliesQuery.cs
Domain/Repositories/ISupplyRepository.cs  ← Interfaz en Domain
Domain/Services/IInventoryObserver.cs      ← Interfaz en Domain
Application/Internal/CommandServices/SupplyCommandService.cs
Application/Internal/QueryServices/SupplyQueryService.cs
Application/Internal/Consumers/OrderCreatedConsumer.cs
Infrastructure/Persistence/Repositories/SupplyRepository.cs ← Implementa ISupplyRepository
Interfaces/REST/SuppliesController.cs
```

### Comparativa por microservicio

| Servicio | Capas | Observaciones |
|---|---|---|
| `InventoryService` | Completas 4 capas + Tests/ | El más completo. Incluye Consumers, Observer, CQRS, Tests BDD (`.feature`) |
| `PurchaseService` | Completas 4 capas | Tiene `Infrastructure/ExternalServices/` para llamadas HTTP a Inventory |
| `ProfilesService` | Completas 4 capas | Los Value Objects están bien definidos (`CompanyName`, `FiscalAddress`, etc.) |
| `AnalyticsService` | Completas 4 capas | Incluye `Infrastructure/Services/` para caché. La `AnalyticsReportData` depende de Resources de Interfaces (violación de capas — ver Sección 9) |
| `AuthService` | Capas incompletas | Usa nomenclatura mixta: `application/internal/` (minúsculas) vs `interfaces/REST/`. No tiene subcarpetas `Application/Internal/CommandServices/`, mezcla el QueryService como command-query a la vez |
| `GatewayService` | Solo infrastructure | Correcto para su rol: solo `Program.cs` + `appsettings.json`. No tiene dominio propio |
| `IoTSimulatorService` | Simplificado | Tiene `Application/Engine/SimulationEngine.cs` como workaround. No sigue DDD; es un servicio de infraestructura |

### Desviación notable: `AuthService`

`AuthService` usa nomenclatura inconsistente: las carpetas son `application/internal/` y `interfaces/REST/` (minúsculas) en lugar de `Application/Internal/` y `Interfaces/REST/` (PascalCase como el resto). El `AuthQueryService` realiza tanto autenticación (operación de escritura: generar token) como consultas, mezclando responsabilidades que en CQRS estarían separadas.

---

## SECCIÓN 5 — MODELADO DE DOMINIO (DDD)

### Bounded Contexts (uno por microservicio)

| Bounded Context | Microservicio | Namespace raíz |
|---|---|---|
| **Identity** | `AuthService` | `WinesoftPlatform.API.Shared.Domain.Model` |
| **Inventory** | `InventoryService` | `WinesoftPlatform.API.Inventory.Domain` |
| **Purchase** | `PurchaseService` | `WinesoftPlatform.API.Purchase.Domain` |
| **Profiles** | `ProfilesService` | `WinesoftPlatform.API.Profiles.Domain` |
| **Analytics** | `AnalyticsService` | `WinesoftPlatform.API.Analytics.Domain` |
| **Simulation** | `IoTSimulatorService` | (sin namespace de dominio formal) |

### Aggregates y Aggregate Roots

| Aggregate Root | Bounded Context | Archivo |
|---|---|---|
| `Supply` | Inventory | `Domain/Model/Aggregates/Supply.cs` |
| `SensorAlert` | Inventory | `Domain/Model/Aggregates/SensorAlert.cs` |
| `Order` | Purchase | `Domain/Model/Aggregates/Order.cs` |
| `Profile` | Profiles | `Domain/Model/Aggregates/Profile.cs` |
| `AnalyticsReportData` | Analytics | `Domain/Model/Aggregates/AnalyticsReportData.cs` |
| `User` | Identity | `Domain/Model/User.cs` (hereda de `BaseEntity`) |

Los Aggregate Roots protegen sus invariantes con propiedades `private set`. Por ejemplo:

```csharp
// Supply.cs
public int OwnerId { get; private set; }
public void DeductStock(int quantity) { Quantity -= quantity; }
```

```csharp
// SensorAlert.cs — comentado explícitamente
/// <summary>Aggregate root representing a sensor alert received from an IoT device.</summary>
public class SensorAlert { public void Acknowledge() { Acknowledged = true; } }
```

### Value Objects

| Value Object | Bounded Context | Archivo |
|---|---|---|
| `CompanyName` | Profiles | `Domain/Model/ValueObjects/CompanyName.cs` |
| `FiscalAddress` | Profiles | `Domain/Model/ValueObjects/FiscalAddress.cs` |
| `ContactPhone` | Profiles | `Domain/Model/ValueObjects/ContacPhone.cs` |
| `TaxIdentity` | Profiles | `Domain/Model/ValueObjects/TaxIdentity.cs` |
| `ReportPeriod` | Analytics | `Domain/Model/ValueObjects/ReportPeriod.cs` |
| `AnalyticsProjections` | Analytics | `Domain/Model/ValueObjects/AnalyticsProjections.cs` |
| `WidgetType` | Analytics | `Domain/Model/ValueObjects/WidgetType.cs` |

`ReportPeriod` es un `record` con lógica de validación embebida, patrón correcto de Value Object:

```csharp
public record ReportPeriod(DateTime StartDate, DateTime EndDate)
{
    public int DaysDuration => (EndDate - StartDate).Days;
    public void Validate() {
        if (StartDate > EndDate) throw new ArgumentException("...");
    }
}
```

`Profile` encapsula 4 Value Objects en su constructor:

```csharp
public Profile(CreateProfileCommand command)
{
    Name = new CompanyName(command.BusinessName, command.Branch);
    Address = new FiscalAddress(command.Street, ...);
    Phone = new ContactPhone(command.Phone);
    LegalId = new TaxIdentity(command.LegalId);
}
```

### Domain Services (interfaces en Domain)

| Interfaz | Implementación | Bounded Context | Archivo |
|---|---|---|---|
| `IInventoryObserver` | `AlertEngine` | Inventory | `Domain/Services/IInventoryObserver.cs` / `AlertEngine.cs` |
| `IInventorySubject` | `InventorySubject` | Inventory | `Domain/Services/IInventorySubject.cs` |
| `ISupplyCommandService` | `SupplyCommandService` | Inventory | `Domain/Services/ISupplyCommandService.cs` |
| `ISupplyQueryService` | `SupplyQueryService` | Inventory | `Domain/Services/ISupplyQueryService.cs` |
| `IAnalyticsCacheService` | `AnalyticsCacheService` | Analytics | `Domain/Services/IAnalyticsCacheService.cs` |

### Repositories (interfaces vs implementaciones)

| Interfaz | Implementación | Bounded Context |
|---|---|---|
| `ISupplyRepository` (Domain) | `SupplyRepository` (Infrastructure) | Inventory |
| `ISensorAlertRepository` (Domain) | `SensorAlertRepository` (Infrastructure) | Inventory |
| `IOrderRepository` (Domain) | `OrderRepository` (Infrastructure) | Purchase |
| `IUserRepository` (Shared Domain) | En Shared Infrastructure | Identity |

**`BaseRepository<T>` en Shared** es la clase base genérica con `AddAsync`, `Update`, `Remove`, `FindByIdAsync`, etc. Todas las implementaciones concretas la heredan y añaden queries específicas del contexto (ej: `SupplyRepository.ListByOwnerIdAsync(int ownerId)`).

---

## SECCIÓN 6 — DRIVERS ARQUITECTÓNICOS

### Drivers de Negocio

| Driver | Evidencia en código |
|---|---|
| **Multi-tenancy por propietario** | Cada entidad (`Supply`, `Order`, `SensorAlert`) tiene columna `OwnerId`. Toda query filtra por `OwnerId` extraído del JWT. `SupplyRepository.ListByOwnerIdAsync(ownerId)` garantiza aislamiento por defecto |
| **Plataforma B2B para dueños de negocio** | No existe rol "proveedor" o "administrador". La nota al final de `Tecnologias_Vintara_Backend.txt` lo documenta explícitamente: el segmento son *únicamente dueños de negocio* |
| **Monitoreo de inventario con IoT** | `IoTSimulatorService` envía telemetría continua a `InventoryService`. `AlertEngine` evalúa thresholds de temperatura, humedad, presión y nivel de stock |

### Drivers Técnicos

| Driver | Evidencia en código |
|---|---|
| **Despliegue en contenedores (Docker/Docker Compose)** | Cada microservicio tiene su propio `Dockerfile`. `docker-compose.yml` orquesta 8 servicios + MySQL + RabbitMQ + Redis |
| **Equipo distribuido con Pull Requests** | `ci.yml` ejecuta build y test automático en PRs a `develop` y `main`. El pipeline bloquea el merge si falla |
| **Arquitectura Cloud Native** | Variables de entorno en `.env`/`docker-compose.yml`, sin configuración hardcodeada en el código (validación en `Program.cs` con `throw` si faltan) |
| **Alta disponibilidad ante fallos de red** | Polly en `PurchaseService`, MassTransit con reintentos automáticos por defecto, Redis caché para absorber peticiones duplicadas |

### Restricciones

| Restricción | Evidencia |
|---|---|
| **.NET 10 (SDK)** | `ci.yml`: `dotnet-version: '10.0.x'`. Todos los `.csproj` usan `TargetFramework: net10.0` |
| **MySQL** | `docker-compose.yml` imagen `mysql:8.0`. Paquete `MySql.EntityFrameworkCore` en todos los `.csproj` |
| **RabbitMQ 3 como broker** | `docker-compose.yml` imagen `rabbitmq:3-management`. Configuración en cada `Program.cs` con `UsingRabbitMq` |
| **Redis Alpine para caché** | `docker-compose.yml` imagen `redis:alpine`. Solo `AnalyticsService` lo consume directamente |
| **Seguridad: repositorio público en GitHub** | `Tecnologias_Vintara_Backend.txt` documenta que el JWT secret estaba expuesto antes. El `.gitignore` excluye `.env`. Se usa `.env.example` como plantilla pública |

---

## SECCIÓN 7 — ATRIBUTOS DE CALIDAD

### 1. Seguridad

| Campo | Detalle |
|---|---|
| **Escenario** | Un usuario autenticado intenta acceder a inventario o compras de otro usuario mediante manipulación de parámetros de URL (IDOR) |
| **Cómo se aborda** | `OwnerId` se extrae exclusivamente del JWT en el controlador (nunca del body/querystring). Toda query filtra por ese valor. `PurchaseOrdersController.GetOwnerId()` devuelve 401 si el claim no existe |
| **Verificación** | Intentar `GET /api/v1/purchase-orders` con JWT de usuario A y esperar obtener solo sus órdenes, no las de B |
| **Evidencia adicional** | Endpoints protegidos con `[Authorize]`. Rate Limiting en login/register (5 req/min). Paquetes JWT en versión que corrige CVE-2024-21319 |

### 2. Disponibilidad / Tolerancia a Fallos

| Campo | Detalle |
|---|---|
| **Escenario** | `InventoryService` está caído o responde lentamente cuando `PurchaseService` intenta crear una orden |
| **Cómo se aborda** | `AddStandardResilienceHandler` con timeout total de 15 segundos y por intento de 5 segundos. Si el servicio no responde, devuelve un string degradado en lugar de lanzar excepción no controlada |
| **Verificación** | Detener el contenedor de `InventoryService` y crear una orden: `PurchaseService` debe retornar respuesta degradada en < 15 segundos |

### 3. Rendimiento / Escalabilidad de Lectura

| Campo | Detalle |
|---|---|
| **Escenario** | Múltiples usuarios consultan reportes de analítica simultáneamente, generando carga sobre MySQL con JOINs complejos |
| **Cómo se aborda** | `AnalyticsQueryService.GetOrAddAsync()` cachea cada resultado en Redis con TTL aleatorio de 5-10 minutos. El caché se invalida automáticamente ante eventos `SupplyStockChanged` u `OrderCreated` |
| **Verificación** | Medir tiempo de respuesta de `GET /api/v1/analytics/kpis` en segunda petición (< 15ms esperado vs > 500ms primera vez) |

### 4. Trazabilidad / Observabilidad

| Campo | Detalle |
|---|---|
| **Escenario** | Un error ocurre en `InventoryService` al procesar un evento de compra. Es necesario rastrear la cadena de eventos desde la petición original |
| **Cómo se aborda** | `CorrelationIdMiddleware` genera o propaga el header `X-Correlation-Id` en cada request. Todos los consumers de MassTransit leen el `CorrelationId` del contexto del mensaje y lo inyectan en `LogContext` de Serilog |
| **Verificación** | Buscar el mismo UUID en los logs de `GatewayService`, `PurchaseService` e `InventoryService` para una operación de compra |

### 5. Modificabilidad / Extensibilidad

| Campo | Detalle |
|---|---|
| **Escenario** | Se necesita añadir un nuevo tipo de notificación (ej: email) cuando el stock cae por debajo del umbral crítico |
| **Cómo se aborda** | El patrón Observer (`IInventoryObserver`) permite registrar nuevos observadores en el DI container sin tocar `InventorySubject` ni `SupplyCommandService` |
| **Verificación** | Crear nueva clase que implemente `IInventoryObserver`, registrarla en `Program.cs`, y verificar que `AlertEngine` sigue funcionando sin cambios |

### 6. Integridad de Datos / Consistencia

| Campo | Detalle |
|---|---|
| **Escenario** | La base de datos evoluciona: se añade columna `OwnerId` a tablas existentes que ya tienen datos |
| **Cómo se aborda** | Migraciones formales de EF Core con `Database.Migrate()` al arrancar cada contenedor. `EnsureCreated()` fue eliminado por completo del código base |
| **Verificación** | Aplicar una nueva migración sin volcar volúmenes de Docker y verificar que los datos existentes se conservan |

---

## SECCIÓN 8 — TÁCTICAS ARQUITECTÓNICAS

### Tácticas de Seguridad

| Táctica | Clase/Archivo | Atributo |
|---|---|---|
| Authenticate Actors (JWT Bearer) | `AddJwtBearer(...)` en `Program.cs` de cada microservicio | Seguridad |
| Authorize Actors (`[Authorize]`) | `SuppliesController.cs`, `PurchaseOrdersController.cs`, `AnalyticsController.cs` | Seguridad |
| Limit Access (OwnerId Filtering) | `GetOwnerId()` en controllers + `.Where(o => o.OwnerId == ownerId)` en repositories | Seguridad (IDOR prevention) |
| Limit Exposure (Rate Limiting) | `GatewayService/Program.cs` (60 req/min global), `AuthService/Program.cs` (5 req/min en login/register) | Seguridad |
| Validate Input (Env Variables at startup) | `throw new InvalidOperationException("JWT Key is not configured.")` en cada `Program.cs` | Seguridad |
| Service Authentication (Client Credentials) | `[Authorize(Policy = "ServiceOnly")]` en `InternalSuppliesController.cs` | Seguridad |

### Tácticas de Disponibilidad

| Táctica | Clase/Archivo | Atributo |
|---|---|---|
| Retry (Polly Standard Resilience) | `.AddStandardResilienceHandler()` en `PurchaseService/Program.cs` | Disponibilidad |
| Timeout (15s total, 5s por intento) | `options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(15)` | Disponibilidad |
| Degrade Gracefully | `InventoryServiceClient.cs`: retorna `"Degraded: Supply name unavailable"` en lugar de excepción | Disponibilidad |
| Asynchronous Decoupling (Pub/Sub) | `publishEndpoint.Publish<OrderCreated>(...)` en `OrderCommandService.cs` | Disponibilidad |
| Health Check | `.AddHealthChecks().AddCheck<DbHealthCheck<InventoryDbContext>>` en `InventoryService/Program.cs` | Disponibilidad |
| Retry on DB Migration | Bucle de 12 intentos con `Task.Delay(5s)` en `Program.cs` de cada servicio | Disponibilidad |

### Tácticas de Rendimiento

| Táctica | Clase/Archivo | Atributo |
|---|---|---|
| Cached Data (Read Cache) | `AnalyticsQueryService.GetOrAddAsync()` con Redis | Rendimiento |
| Event-Based Cache Invalidation | `SupplyStockChangedConsumer.cs` → `cacheService.InvalidateCacheAsync(ownerId)` | Rendimiento |
| Cache Key Tracking | `AnalyticsCacheService.TrackCacheKeyAsync()` registra llaves por OwnerId para invalidación masiva | Rendimiento |
| TTL Randomization | `TimeSpan.FromMinutes(Random.Shared.Next(5, 11))` evita cache stampede | Rendimiento |

### Tácticas de Trazabilidad

| Táctica | Clase/Archivo | Atributo |
|---|---|---|
| Correlation ID Propagation | `CorrelationIdMiddleware.cs` en todos los servicios | Trazabilidad |
| Structured Logging (Serilog) | `UseSerilog()` con `LogContext.PushProperty("CorrelationId", ...)` en todos los `Program.cs` | Trazabilidad |
| Rolling Log Files | `WriteTo.File("logs/...-log-.txt", rollingInterval: RollingInterval.Day)` | Trazabilidad |
| Correlation ID in MassTransit | `context.CorrelationId?.ToString()` en todos los Consumers | Trazabilidad |

### Tácticas de Modificabilidad

| Táctica | Clase/Archivo | Atributo |
|---|---|---|
| Dependency Inversion (Repository Pattern) | Interfaces en `Domain/Repositories/`, impl. en `Infrastructure/Persistence/` | Modificabilidad |
| Observer Pattern (Open/Closed) | `IInventoryObserver` + `AlertEngine` registrados en DI | Modificabilidad |
| Separate Concerns (CQRS) | Segregación de `CommandServices` y `QueryServices` | Modificabilidad |
| CI Pipeline (Prevent Regression) | `ci.yml`: `dotnet build --warnaserror:NU1902` + `dotnet test` en cada PR | Modificabilidad |

---

## SECCIÓN 9 — DEUDA TÉCNICA Y RIESGOS

### 1. Dependencia circular de capas en `AnalyticsService`

**Afectado:** `src/AnalyticsService/.../Domain/Model/Aggregates/AnalyticsReportData.cs`

```csharp
// Domain depende de Interfaces — violación de Clean Architecture
using WinesoftPlatform.API.Analytics.Interfaces.REST.Resources;

public class AnalyticsReportData
{
    public IEnumerable<PurchaseOrderResource> Orders { get; }   // Resource (capa Interfaces) en Domain
    public IEnumerable<SupplyRotationResource> SupplyRotation { get; }
}
```

**Riesgo:** El dominio no puede compilar sin la capa de presentación. Imposibilita testear el dominio de forma aislada y viola el principio fundamental de Clean Architecture.

---

### 2. Errores silenciados con `Console.WriteLine` en `OrderCommandService`

**Afectado:** `src/PurchaseService/.../Application/Internal/CommandServices/OrderCommandService.cs`

```csharp
catch (Exception e)
{
    Console.WriteLine($"Error creating order: {e.Message}");
    return null;  // Retorna null en lugar de propagar el error
}
```

**Riesgo:** Los errores en la creación de órdenes se silencian y no aparecen en Serilog. El llamador recibe `null` y puede interpretarlo como "orden no encontrada" en lugar de "error interno". Las excepciones con datos de contexto se pierden.

---

### 3. Sin orquestador Saga — consistencia eventual débilmente garantizada

**Afectado:** Flujo `PurchaseService` → `InventoryService` → `AnalyticsService`

La saga entre servicios es una **coreografía implícita** sin estado persistido. Si `InventoryService` falla al consumir `OrderCreated` y el mensaje se reencola, no existe compensación si la orden ya fue creada. MassTransit no tiene configurado un `Outbox Pattern` ni una dead-letter queue explícita visible en el código.

**Riesgo:** Una falla en el broker durante una ventana crítica puede resultar en órdenes creadas sin descuento de stock, o stock descontado sin orden registrada. La consistencia eventual depende enteramente de la fiabilidad de RabbitMQ sin salvaguardas adicionales.

---

### 4. `InventoryServiceClient` (PurchaseService) retorna un string no-descriptivo

**Afectado:** `src/PurchaseService/.../Infrastructure/ExternalServices/InventoryServiceClient.cs`

```csharp
return "Supply fetched from HTTP"; // Simplified for this migration step
```

**Riesgo:** El nombre del suministro siempre es el literal `"Supply fetched from HTTP"` en lugar del nombre real. El comentario indica que es una migración pendiente, pero en estado actual, todos los registros de órdenes tendrán datos incorrectos de nombre de producto. Es deuda técnica explícita documentada en el código.

---

### 5. Credenciales de RabbitMQ hardcodeadas en producción

**Afectado:** `src/InventoryService/.../Program.cs`, `src/PurchaseService/.../Program.cs`, `src/AnalyticsService/.../Program.cs`

```csharp
cfg.Host(rabbitHost, "/", h => {
    h.Username("guest");
    h.Password("guest");
});
```

**Riesgo:** Las credenciales predeterminadas de RabbitMQ (`guest`/`guest`) están hardcodeadas. Cualquier persona con acceso a la red Docker puede autenticarse en el broker en producción. Las variables de entorno para JWT y MySQL se parametrizan correctamente, pero no así las de RabbitMQ.

---

### 6. AuthService — falta de separación CQRS y nomenclatura inconsistente

**Afectado:** `src/AuthService/WinesoftPlatform.AuthService/application/internal/queryservices/AuthQueryService.cs`

La clase se llama `QueryService` pero realiza efectos de escritura (genera tokens de sesión, valida cliente de servicio). Las carpetas están en minúscula en lugar de PascalCase como el resto de servicios. No existe `AuthCommandService`.

**Riesgo:** El servicio de autenticación no sigue el patrón CQRS que todos los demás aplican. Esto lo excluye del tooling y convenciones del equipo, y mezcla en una sola clase dos responsabilidades que en un sistema de producción deberían ser separadas (autenticación → command; introspección de token → query).

---

### 7. `IoTSimulatorService` sin estructura DDD ni aislamiento de configuración

**Afectado:** `src/IoTSimulatorService/WinesoftPlatform.IoTSimulatorService/`

El simulador usa `ClientId=iot-simulator` y `ClientSecret=iot-simulator-secret-key-123456` tanto en `docker-compose.yml` como en `AuthService/Program.cs`:

```yaml
# docker-compose.yml
- ServiceAuth__ClientId=iot-simulator
- ServiceAuth__ClientSecret=iot-simulator-secret-key-123456
```

**Riesgo:** El secret de autenticación service-to-service está en texto plano en el repositorio (visible en el código de `AuthService`). Aunque para un simulador de pruebas es aceptable, en un entorno de producción real esto representa un vector de acceso no autorizado al inventario mediante token de servicio.
