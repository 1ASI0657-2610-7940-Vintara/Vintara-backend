# Análisis Arquitectónico — Vintara Backend (WineSoft Platform)

> **Scope:** rama `develop`, revisión de julio 2026  
> **Fuente:** inspección directa de `src/`, `docker-compose.yml` y `.github/workflows/ci.yml`

---

## SECCIÓN 1 — ESTILO ARQUITECTÓNICO

### Veredicto: Arquitectura de Microservicios con API Gateway centralizado

El sistema está compuesto por **7 proyectos independientes** bajo `src/`, cada uno con su propio `Dockerfile`, base de datos MySQL separada (para los que persisten), y proceso ASP.NET Core autónomo:

| Servicio | Proyecto |
|---|---|
| API Gateway | `src/GatewayService/WinesoftPlatform.GatewayService` |
| Autenticación | `src/AuthService/WinesoftPlatform.AuthService` |
| Inventario | `src/InventoryService/WinesoftPlatform.InventoryService` |
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
  profiles-service:
    ports: ["5004:8080"]
    environment:
      - ConnectionStrings__DefaultConnection=...database=winesoft_profiles...
```

Cada microservicio tiene **su propia base de datos MySQL aislada** (`winesoft_auth`, `winesoft_inventory`, `winesoft_profiles`, etc.), lo cual es el sello distintivo del estilo microservicios: _database per service_. El acceso externo está unificado por YARP en `GatewayService` (puerto 5000).

---

## SECCIÓN 2 — ESTILO DE COMUNICACIÓN

El sistema usa **dos mecanismos de comunicación coexistentes**, aplicados según el tipo de operación:

### Mecanismo 1: REST Síncrono (HTTP)

Usado para consultas de datos en tiempo real que requieren respuesta inmediata.

| Origen | Destino | Propósito | Archivo |
|---|---|---|---|
| `AnalyticsService` | `InventoryService` | Obtener lista de suministros para cálculo de métricas | `src/AnalyticsService/.../Infrastructure/ExternalServices/ServiceClients.cs` — `InventoryServiceClient` |
| `IoTSimulatorService` | `InventoryService` | Enviar telemetría de sensores | `src/IoTSimulatorService/.../Application/Engine/SimulationEngine.cs` |
| `IoTSimulatorService` | `AuthService` | Obtener token JWT de servicio | `src/IoTSimulatorService/.../Application/Engine/SimulationEngine.cs` |

**Evidencia en `ServiceClients.cs` (AnalyticsService):**
```csharp
var response = await _httpClient.GetAsync("/api/v1/inventory/supplies");
```

### Mecanismo 2: Mensajería Asíncrona (RabbitMQ + MassTransit)

Usado para operaciones de escritura/notificación donde el emisor no necesita esperar al receptor.

| Evento | Publicador | Consumidores | Archivo del publicador | Archivo del consumidor |
|---|---|---|---|---|
| `SupplyStockChanged` | `InventoryService` | `AnalyticsService` | `InventorySubject.cs` | `AnalyticsService/.../Consumers/SupplyStockChangedConsumer.cs` |

**Configuración de colas en `AnalyticsService/Program.cs`:**
```csharp
cfg.ReceiveEndpoint("analytics-stock-changed-queue", e => {
    e.ConfigureConsumer<SupplyStockChangedConsumer>(context);
});
```

**Flujo combinado de actualización de stock:**
```
POST /api/inventory/stockmovements
  → InventoryService: guarda StockMovement y actualiza Supply en MySQL
  → InventoryService: notifica localmente e inicia flujo
  → InventoryService: publica SupplyStockChanged → RabbitMQ (asíncrono)
  → AnalyticsService consume SupplyStockChanged → invalida caché Redis del Owner
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
| **Servicios** | `InventoryService`, `ProfilesService`, `AnalyticsService` |
| **Archivos** | `Application/Internal/CommandServices/`, `Application/Internal/QueryServices/`, `Domain/Model/Commands/`, `Domain/Model/Queries/` |
| **Problema que resuelve** | Separar responsabilidades de escritura (Commands) y lectura (Queries) para favorecer la modificabilidad y el testeo independiente |
| **Evidencia** | `CreateSupplyCommand.cs`, `GetAllSuppliesQuery.cs`, `SupplyCommandService.cs`, `SupplyQueryService.cs` en InventoryService |

### 3. Publisher/Subscriber (Pub/Sub vía MassTransit + RabbitMQ)

| Campo | Detalle |
|---|---|
| **Servicios** | `InventoryService` (publisher) y `AnalyticsService` (subscriber) |
| **Archivos** | `InventorySubject.cs`, `SupplyStockChangedConsumer.cs` |
| **Problema que resuelve** | Desacoplar la variación de stock de la actualización/invalidación de caché en el servicio analítico. El emisor no sabe quién recibe el evento |

### 4. Observer (GoF — aplicado localmente en InventoryService)

| Campo | Detalle |
|---|---|
| **Servicio** | `InventoryService` |
| **Archivos** | `Domain/Services/IInventoryObserver.cs`, `IInventorySubject.cs`, `AlertEngine.cs`, `Application/Internal/CommandServices/InventorySubject.cs` |
| **Problema que resuelve** | Detectar anomalías de sensores IoT de forma extensible. Nuevos observadores se añaden sin modificar la lógica de `SensorAlertCommandService` |
| **Registro en DI** | `Program.cs` líneas 149-150: `builder.Services.AddScoped<IInventorySubject, InventorySubject>()` y `builder.Services.AddScoped<IInventoryObserver, AlertEngine>()` |

### 5. Repository

| Campo | Detalle |
|---|---|
| **Todos los servicios** | `InventoryService`, `ProfilesService`, `AnalyticsService`, `AuthService` |
| **Archivos** | Interfaces en `Domain/Repositories/` (ej. `ISupplyRepository.cs`), implementaciones en `Infrastructure/Persistence/Repositories/` (ej. `SupplyRepository.cs`) |
| **Base común** | `src/Shared/.../Infrastructure/Persistence/EFC/Repositories/` contiene `BaseRepository<T>` y `UnitOfWork` reutilizados por todos |
| **Problema que resuelve** | Abstraer las consultas de base de datos del dominio. La capa de aplicación depende de interfaces, no de EF Core directamente |

### 6. Aggregate Root (DDD — ver Sección 5)

Los aggregates actúan como fábricas internas: reciben Commands en el constructor y encapsulan la lógica de construcción, impidiendo la creación de entidades en estados inválidos. Ejemplo: `Supply(CreateSupplyCommand command)` en `Supply.cs`.

---

## SECCIÓN 4 — ESTRUCTURA INTERNA DE MICROSERVICIOS

### Patrón predominante: Clean Architecture (con influencia de DDD)

Todos los microservicios con dominio propio (`InventoryService`, `ProfilesService`, `AnalyticsService`) siguen la misma estructura de 4 capas con dependencias apuntando hacia adentro:

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
Infrastructure/Persistence/Repositories/SupplyRepository.cs ← Implementa ISupplyRepository
Interfaces/REST/SuppliesController.cs
```

### Comparativa por microservicio

| Servicio | Capas | Observaciones |
|---|---|---|
| `InventoryService` | Completas 4 capas + Tests/ | El más completo. Incluye Observer, CQRS, Tests BDD (`.feature`) y Unitarios |
| `ProfilesService` | Completas 4 capas | Los Value Objects están bien definidos (`CompanyName`, `FiscalAddress`, etc.) |
| `AnalyticsService` | Completas 4 capas | Incluye `Infrastructure/Services/` para caché. La `AnalyticsReportData` depende de Resources de Interfaces (violación de capas — ver Sección 9) |
| `AuthService` | Capas incompletas | Usa nomenclatura mixta: `application/internal/` (minúsculas) vs `interfaces/REST/`. Mezcla el QueryService como command-query a la vez |
| `GatewayService` | Solo infrastructure | Correcto para su rol: solo `Program.cs` + `appsettings.json`. No tiene dominio propio |
| `IoTSimulatorService` | Simplificado | Tiene `Application/Engine/SimulationEngine.cs` como workaround. No sigue DDD; es un servicio de infraestructura |

### Desviación notable: `AuthService`

`AuthService` usa nomenclatura inconsistente: las carpetas son `application/internal/` y `interfaces/REST/` (minúsculas) en lugar de `Application/Internal/` y `Interfaces/REST/` (PascalCase como el resto). El `AuthQueryService` realiza tanto autenticación (operación de escritura: generar token) como consultas, mezclando responsabilidades.

---

## SECCIÓN 5 — MODELADO DE DOMINIO (DDD)

### Bounded Contexts (uno por microservicio)

| Bounded Context | Microservicio | Namespace raíz |
|---|---|---|
| **Identity** | `AuthService` | `WinesoftPlatform.API.Shared.Domain.Model` |
| **Inventory** | `InventoryService` | `WinesoftPlatform.API.Inventory.Domain` |
| **Profiles** | `ProfilesService` | `WinesoftPlatform.API.Profiles.Domain` |
| **Analytics** | `AnalyticsService` | `WinesoftPlatform.API.Analytics.Domain` |
| **Simulation** | `IoTSimulatorService` | (sin namespace de dominio formal) |

### Aggregates y Aggregate Roots

| Aggregate Root | Bounded Context | Archivo |
|---|---|---|
| `Supply` | Inventory | `Domain/Model/Aggregates/Supply.cs` |
| `SensorAlert` | Inventory | `Domain/Model/Aggregates/SensorAlert.cs` |
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
| `IUserRepository` (Shared Domain) | En Shared Infrastructure | Identity |

**`BaseRepository<T>` en Shared** es la clase base genérica con `AddAsync`, `Update`, `Remove`, `FindByIdAsync`, etc. Todas las implementaciones concretas la heredan y añaden queries específicas del contexto (ej: `SupplyRepository.ListByOwnerIdAsync(int ownerId)`).

---

## SECCIÓN 6 — DRIVERS ARQUITECTÓNICOS

### Drivers de Negocio

| Driver | Evidencia en código |
|---|---|
| **Multi-tenancy por propietario** | Cada entidad (`Supply`, `SensorAlert`) tiene columna `OwnerId`. Toda query filtra por `OwnerId` extraído del JWT. `SupplyRepository.ListByOwnerIdAsync(ownerId)` garantiza aislamiento por defecto |
| **Plataforma B2B para dueños de negocio** | No existe rol "proveedor" o "administrador". La nota al final de `Tecnologias_Vintara_Backend.txt` lo documenta explícitamente: el segmento son *únicamente dueños de negocio* |
| **Monitoreo de inventario con IoT** | `IoTSimulatorService` envía telemetría continua a `InventoryService`. `AlertEngine` evalúa thresholds de temperatura, humedad, presión y nivel de stock |

### Drivers Técnicos

| Driver | Evidencia en código |
|---|---|
| **Despliegue en contenedores (Docker/Docker Compose)** | Cada microservicio tiene su propio `Dockerfile`. `docker-compose.yml` orquesta 7 servicios + MySQL + RabbitMQ + Redis |
| **Equipo distribuido con Pull Requests** | `ci.yml` ejecuta build y test automático en PRs a `develop` y `main`. El pipeline bloquea el merge si falla |
| **Arquitectura Cloud Native** | Variables de entorno en `.env`/`docker-compose.yml`, sin configuración hardcodeada en el código (validación en `Program.cs` con `throw` si faltan) |
| **Alta disponibilidad y tolerancia a fallos** | MassTransit con reintentos por defecto, desacoplamiento asíncrono vía RabbitMQ, Redis caché para absorber consultas concurrentes |

### Restricciones

| Restricción | Evidencia |
|---|---|
| **.NET 10 (SDK)** | `ci.yml`: `dotnet-version: '10.0.x'`. Todos los `.csproj` usan `TargetFramework: net10.0` |
| **MySQL** | `docker-compose.yml` imagen `mysql:8.0`. Paquete `MySql.EntityFrameworkCore` en todos los `.csproj` |
| **RabbitMQ 3 como broker** | `docker-compose.yml` imagen `rabbitmq:3-management`. Configuración en cada `Program.cs` con `UsingRabbitMq` |
| **Redis Alpine para caché** | `docker-compose.yml` imagen `redis:alpine`. Solo `AnalyticsService` lo consume directamente |
| **Seguridad: repositorio público en GitHub** | Se evita subir secretos reales. El `.gitignore` excluye `.env`. Se usa `.env.example` como plantilla pública |

---

## SECCIÓN 7 — ATRIBUTOS DE CALIDAD Y SU DEMOSTRACIÓN

### 1. Seguridad (Aislamiento Multi-tenant y Prevención IDOR)

* **Importancia**: Garantizar que un `OwnerId` (inquilino/dueño de negocio) no pueda ver, modificar ni borrar información (suministros, alertas, perfiles) de otro, previniendo vulnerabilidades tipo IDOR (Insecure Direct Object Reference).
* **Cómo se demuestra**:
  - `OwnerId` se extrae de forma segura a través de los Claims del token JWT en el controlador (usando la cabecera HTTP `Authorization: Bearer <token>`). Nunca se acepta del cuerpo del request (`body`) o de parámetros de la URL para operaciones de usuario.
  - Los repositorios aplican filtros estrictos de pertenencia (por ejemplo, `SupplyRepository.ListByOwnerIdAsync(ownerId)` y `SupplyQueryService.ListSuppliesAsync(ownerId)`).
  - Las pruebas unitarias en `SupplyQueryServiceTests.cs` comprueban explícitamente que al listar suministros se excluyen los de otros inquilinos, y que intentar acceder a un suministro ajeno lanza `UnauthorizedAccessException`.

### 2. Disponibilidad / Tolerancia a Fallos (Robustez del Sistema)

* **Importancia**: Asegurar que las fallas temporales de la red o la latencia de servicios dependientes no causen caídas en cascada del backend completo.
* **Cómo se demuestra**:
  - **Desacoplamiento asíncrono**: `InventoryService` publica el evento `SupplyStockChanged` a través de RabbitMQ de forma no bloqueante. Si `AnalyticsService` se encuentra temporalmente inactivo, las alertas y el inventario siguen funcionando; los mensajes quedan seguros en la cola y se procesan cuando el servicio analítico se recupera.
  - **Tolerancia al arranque tardío de BD**: Cada microservicio (como se ve en `InventoryService/Program.cs`) implementa un bucle de reintento automático (12 intentos, 5 segundos de retraso) para aplicar las migraciones de base de datos en el inicio. Esto evita que el contenedor muera inmediatamente si el contenedor MySQL aún no está listo.

### 3. Rendimiento / Escalabilidad de Lectura

* **Importancia**: Evitar la sobrecarga en la base de datos MySQL por consultas complejas y repetitivas de KPI analíticos generadas por múltiples usuarios concurrentes.
* **Cómo se demuestra**:
  - **Uso de Caché Distribuido (Redis)**: `AnalyticsQueryService.GetOrAddAsync()` cachea las proyecciones de analítica por inquilino.
  - **TTL Dinámico**: Se añade un factor aleatorio en el tiempo de vida (TTL) de la caché de 5 a 10 minutos para evitar el efecto de caída concurrente de caché (Cache Stampede).
  - **Invalidación basada en Eventos**: El caché se invalida inmediatamente al recibir el evento `SupplyStockChanged` mediante `SupplyStockChangedConsumer.cs`, garantizando consistencia eventual sin sacrificar rendimiento.

### 4. Trazabilidad / Observabilidad

* **Importancia**: Ser capaz de rastrear una sola solicitud de usuario o telemetría a través del API Gateway, middlewares y múltiples consumidores asíncronos en entornos de microservicios distribuidos.
* **Cómo se demuestra**:
  - **Correlation ID**: `CorrelationIdMiddleware.cs` inyecta la cabecera `X-Correlation-Id` en cada petición entrante. Si no viene del cliente, se genera un UUID único.
  - **Log Estructurado**: Serilog propaga este identificador al contexto de diagnóstico del log (`LogContext`), escribiéndolo en la consola y archivos de log rotativos con la plantilla `({CorrelationId})`.
  - **Propagación en RabbitMQ**: El ID de correlación viaja adjunto en las propiedades del mensaje a través de MassTransit, permitiendo asociar el consumo asíncrono con la llamada REST original.

### 5. Modificabilidad / Extensibilidad (Mantenimiento de Código)

* **Importancia**: Minimizar el impacto de cambios y la adición de nuevas funcionalidades (por ejemplo, nuevos motores de alerta o reglas de negocio) sobre el núcleo de producción.
* **Cómo se demuestra**:
  - **CQRS**: Separación clara de peticiones de lectura y escritura en la capa de aplicación.
  - **Patrón Observer local**: `InventorySubject` notifica de forma dinámica a todos los componentes que implementan `IInventoryObserver` (como `AlertEngine`). Si se requiere enviar alertas por correo o Slack, basta con agregar una nueva implementación y registrarla en el contenedor de inyección de dependencias (DI) en `Program.cs`, cumpliendo el principio Open/Closed.

---

## SECCIÓN 8 — TÁCTICAS ARQUITECTÓNICAS

### Tácticas de Seguridad

| Táctica | Clase/Archivo | Atributo |
|---|---|---|
| Authenticate Actors (JWT Bearer) | `AddJwtBearer(...)` en `Program.cs` de cada microservicio | Seguridad |
| Authorize Actors (`[Authorize]`) | `SuppliesController.cs`, `SensorAlertsController.cs`, `AnalyticsController.cs` | Seguridad |
| Limit Access (OwnerId Filtering) | `GetOwnerId()` en controllers + `.Where(o => o.OwnerId == ownerId)` en repositories | Seguridad (IDOR prevention) |
| Limit Exposure (Rate Limiting) | `GatewayService/Program.cs` (60 req/min global), `AuthService/Program.cs` (5 req/min en login/register) | Seguridad |
| Validate Input (Env Variables at startup) | `throw new InvalidOperationException("JWT Key is not configured.")` en cada `Program.cs` | Seguridad |
| Service Authentication (Client Credentials) | `[Authorize(Policy = "ServiceOnly")]` en `InternalSuppliesController.cs` | Seguridad |

### Tácticas de Disponibilidad

| Táctica | Clase/Archivo | Atributo |
|---|---|---|
| Asynchronous Decoupling (Pub/Sub) | `publishEndpoint.Publish<SupplyStockChanged>(...)` en `InventorySubject.cs` | Disponibilidad |
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
| CI Pipeline (Prevent Regression) | `ci.yml`: `dotnet build` + `dotnet test` en cada PR | Modificabilidad |

---

## SECCIÓN 9 — DEUDA TÉCNICA Y RIESGOS

### 1. Dependencia circular de capas en `AnalyticsService`

**Afectado:** `src/AnalyticsService/.../Domain/Model/Aggregates/AnalyticsReportData.cs`

```csharp
// Domain depende de Interfaces — violación de Clean Architecture
using WinesoftPlatform.API.Analytics.Interfaces.REST.Resources;

public class AnalyticsReportData
{
    public IEnumerable<SupplyRotationResource> SupplyRotation { get; } // Resource (capa Interfaces) en Domain
    public IEnumerable<SupplyLevelResource> SupplyLevels { get; }     // Resource (capa Interfaces) en Domain
    public IEnumerable<LowStockAlertResource> LowStockAlerts { get; } // Resource (capa Interfaces) en Domain
}
```

**Riesgo:** El dominio no puede compilar sin la capa de presentación/interfaces. Imposibilita testear el dominio de forma aislada y viola el principio fundamental de Clean Architecture, donde el Core/Domain no debe tener dependencias de las capas externas.

---

### 2. Credenciales de RabbitMQ hardcodeadas en producción

**Afectado:** `src/InventoryService/.../Program.cs`, `src/AnalyticsService/.../Program.cs`

```csharp
cfg.Host(rabbitHost, "/", h => {
    h.Username("guest");
    h.Password("guest");
});
```

**Riesgo:** Las credenciales predeterminadas de RabbitMQ (`guest`/`guest`) están hardcodeadas en el código fuente. Cualquier persona con acceso a la red Docker puede autenticarse en el broker en producción. Las variables de entorno para JWT y MySQL se parametrizan correctamente, pero no así las de RabbitMQ.

---

### 3. AuthService — falta de separación CQRS y nomenclatura inconsistente

**Afectado:** `src/AuthService/WinesoftPlatform.AuthService/application/internal/queryservices/AuthQueryService.cs`

La clase se llama `QueryService` pero realiza efectos de escritura (genera tokens de sesión, valida cliente de servicio). Las carpetas están en minúscula (`application/internal/...`) en lugar de PascalCase como el resto de servicios. No existe un `AuthCommandService` separado de manera consistente para todas las operaciones de autenticación.

**Riesgo:** El servicio de autenticación no sigue el patrón CQRS que todos los demás aplican. Esto lo excluye del tooling y convenciones del equipo, y mezcla en una sola clase responsabilidades de lectura y escritura.

---

### 4. `IoTSimulatorService` sin estructura DDD ni aislamiento de configuración

**Afectado:** `src/IoTSimulatorService/WinesoftPlatform.IoTSimulatorService/`

El simulador usa `ClientId=iot-simulator` y `ClientSecret=iot-simulator-secret-key-123456` tanto en `docker-compose.yml` como en `AuthService/Program.cs`:

```yaml
# docker-compose.yml
- ServiceAuth__ClientId=iot-simulator
- ServiceAuth__ClientSecret=iot-simulator-secret-key-123456
```

**Riesgo:** El secret de autenticación service-to-service está en texto plano en el repositorio (visible en el código de `AuthService`). Aunque para un simulador de pruebas es aceptable, en un entorno de producción real esto representa un vector de acceso no autorizado al inventario mediante token de servicio.
