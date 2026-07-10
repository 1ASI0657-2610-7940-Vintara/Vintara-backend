# Diagramas de Arquitectura Vintara - Mermaid Format

Este documento contiene los diagramas arquitectónicos del backend de **Vintara (WineSoft)** modelados con **Mermaid**. 
Mermaid se renderiza de forma nativa en GitHub, GitLab y en VS Code (usando la vista de vista previa de Markdown).

---

## 1. C4 Context Diagram

```mermaid
flowchart TD
    owner["Dueño de Negocio (Owner)"]
    operator["Operador (Operator)"]
    winesoft["Plataforma WineSoft"]
    iot_simulator["Servidor de Simulación IoT"]

    owner -->|Maneja bodega, inventario y reportes HTTPS| winesoft
    operator -->|Registra stock y gestiona alertas HTTPS| winesoft
    iot_simulator -->|Envía telemetría de sensores HTTP| winesoft
```

---

## 2. C4 Container Diagram

```mermaid
flowchart TD
    owner["Dueño de Negocio (Owner)"]
    operator["Operador (Operator)"]
    
    subgraph winesoft_system ["Límites del Sistema WineSoft"]
        web_app["Aplicación Web Vue.js / Vite"]
        gateway["API Gateway YARP"]
        auth_service["AuthService .NET"]
        inventory_service["InventoryService .NET"]
        profiles_service["ProfilesService .NET"]
        analytics_service["AnalyticsService .NET"]
        mysql_db[("Base de Datos MySQL")]
        redis_cache[("Caché Redis")]
        rabbitmq{"Message Broker RabbitMQ"}
    end

    iot_simulator["Servidor de Simulación IoT"]

    owner -->|HTTPS| web_app
    operator -->|HTTPS| web_app
    web_app -->|Peticiones HTTP/JSON| gateway
    
    gateway -->|/api/auth| auth_service
    gateway -->|/api/inventory| inventory_service
    gateway -->|/api/profiles| profiles_service
    gateway -->|/api/analytics| analytics_service

    auth_service -->|EF Core| mysql_db
    profiles_service -->|EF Core| mysql_db
    inventory_service -->|EF Core| mysql_db

    iot_simulator -->|HTTP/JSON: Token| auth_service
    iot_simulator -->|HTTP/JSON: Telemetría| inventory_service

    inventory_service -->|Publica SupplyStockChanged| rabbitmq
    rabbitmq -->|Consume event| analytics_service

    analytics_service -->|StackExchange.Redis| redis_cache
    analytics_service -->|HTTP/JSON| inventory_service
```

---

## 3. C4 Component Diagrams

### A. GatewayService
```mermaid
flowchart TD
    web_app["Aplicación Web Vue.js"]
    
    subgraph GatewayService
        program["Program.cs"]
        yarp["YARP Middleware"]
        cors_policy["CORS Middleware"]
        appsettings["appsettings.json"]
    end

    auth_service["AuthService"]
    inventory_service["InventoryService"]
    profiles_service["ProfilesService"]
    analytics_service["AnalyticsService"]

    web_app -->|HTTP Calls| cors_policy
    cors_policy --> yarp
    program --> yarp
    program --> appsettings

    yarp -->|/api/auth| auth_service
    yarp -->|/api/inventory| inventory_service
    yarp -->|/api/profiles| profiles_service
    yarp -->|/api/analytics| analytics_service
```

### B. AuthService
```mermaid
flowchart TD
    gateway["API Gateway YARP"]
    
    subgraph AuthService
        auth_controller["AuthController"]
        auth_query_svc["AuthQueryService"]
        auth_cmd_svc["AuthCommandService"]
        user_repo["UserRepository"]
        auth_db_context["AuthDbContext"]
        user_entity["User Entity"]
    end

    mysql_db[("MySQL DB")]

    gateway -->|HTTP /api/auth| auth_controller
    auth_controller --> auth_query_svc
    auth_controller --> auth_cmd_svc
    auth_query_svc --> user_repo
    auth_cmd_svc --> user_repo
    user_repo --> auth_db_context
    auth_db_context --> user_entity
    auth_db_context --> mysql_db
```

### C. InventoryService
```mermaid
flowchart TD
    gateway["API Gateway YARP"]
    rabbitmq{"RabbitMQ"}
    mysql_db[("MySQL DB")]

    subgraph InventoryService
        supplies_ctrl["SuppliesController"]
        movements_ctrl["StockMovementsController"]
        alerts_ctrl["SensorAlertsController"]
        
        supply_cmd_svc["SupplyCommandService"]
        movement_cmd_svc["StockMovementCommandService"]
        alert_cmd_svc["SensorAlertCommandService"]
        
        supply_query_svc["SupplyQueryService"]
        movement_query_svc["StockMovementQueryService"]
        alert_query_svc["SensorAlertQueryService"]
        
        alert_engine["AlertEngine"]
        inv_subject["InventorySubject"]
        
        supply_repo["SupplyRepository"]
        movement_repo["StockMovementRepository"]
        alert_repo["SensorAlertRepository"]
        
        inv_db_context["InventoryDbContext"]
    end

    gateway -->|HTTP /api/inventory/supplies| supplies_ctrl
    gateway -->|HTTP /api/inventory/stockmovements| movements_ctrl
    gateway -->|HTTP /api/inventory/sensoralerts| alerts_ctrl

    supplies_ctrl --> supply_cmd_svc
    supplies_ctrl --> supply_query_svc
    movements_ctrl --> movement_cmd_svc
    movements_ctrl --> movement_query_svc
    alerts_ctrl --> alert_cmd_svc
    alerts_ctrl --> alert_query_svc

    supply_cmd_svc --> supply_repo
    supply_cmd_svc --> inv_subject
    movement_cmd_svc --> movement_repo
    movement_cmd_svc --> supply_repo
    movement_cmd_svc --> inv_subject
    alert_cmd_svc --> inv_subject

    supply_query_svc --> supply_repo
    movement_query_svc --> movement_repo
    alert_query_svc --> alert_repo

    inv_subject --> alert_engine
    inv_subject -->|Event Publish| rabbitmq

    alert_engine --> alert_repo

    supply_repo --> inv_db_context
    movement_repo --> inv_db_context
    alert_repo --> inv_db_context
    inv_db_context --> mysql_db
```

### D. ProfilesService
```mermaid
flowchart TD
    gateway["API Gateway YARP"]
    mysql_db[("MySQL DB")]

    subgraph ProfilesService
        profiles_ctrl["ProfilesController"]
        profile_cmd_svc["ProfileCommandService"]
        profile_query_svc["ProfileQueryService"]
        profile_facade["ProfilesContextFacade"]
        profile_repo["ProfileRepository"]
        profiles_db_context["ProfilesDbContext"]
    end

    gateway -->|HTTP /api/profiles| profiles_ctrl
    profiles_ctrl --> profile_cmd_svc
    profiles_ctrl --> profile_query_svc
    profile_facade --> profile_query_svc
    profile_cmd_svc --> profile_repo
    profile_query_svc --> profile_repo
    profile_repo --> profiles_db_context
    profiles_db_context --> mysql_db
```

### E. AnalyticsService
```mermaid
flowchart TD
    gateway["API Gateway YARP"]
    rabbitmq{"RabbitMQ"}
    redis_cache[("Redis Cache")]
    inventory_service["InventoryService"]

    subgraph AnalyticsService
        analytics_ctrl["AnalyticsController"]
        analytics_cmd_svc["AnalyticsCommandService"]
        analytics_query_svc["AnalyticsQueryService"]
        stock_consumer["SupplyStockChangedConsumer"]
        pdf_builder["QuestPdfAnalyticsReportBuilder"]
        cache_svc["AnalyticsCacheService"]
        srv_clients["ServiceClients"]
        analytics_repo["AnalyticsRepository"]
    end

    gateway -->|HTTP /api/analytics| analytics_ctrl
    analytics_ctrl --> analytics_query_svc
    analytics_ctrl --> analytics_cmd_svc
    
    analytics_query_svc --> cache_svc
    analytics_query_svc --> srv_clients
    analytics_query_svc --> pdf_builder
    
    stock_consumer --> cache_svc
    rabbitmq -->|Consume event| stock_consumer
    
    srv_clients --> inventory_service
    cache_svc --> redis_cache
```

### F. IoTSimulatorService
```mermaid
flowchart TD
    auth_service["AuthService"]
    inventory_service["InventoryService"]

    subgraph IoTSimulatorService
        sim_engine["SimulationEngine"]
        sim_factory["DeviceSimulatorFactory"]
        base_sim["BaseDeviceSimulator"]
        concrete_sims["ConcreteSensorSimulators"]
    end

    sim_engine --> auth_service
    sim_engine --> sim_factory
    sim_factory --> concrete_sims
    concrete_sims --> base_sim
    sim_engine --> inventory_service
```

---

## 4. Vista Lógica

```mermaid
flowchart TB
    subgraph Presentation_Layer ["Presentation Layer"]
        Controllers
        DTOs
        Assemblers
    end

    subgraph Application_Layer ["Application Layer"]
        CommandServices
        QueryServices
        Consumers["Message Consumers"]
    end

    subgraph Domain_Layer ["Domain Layer"]
        Aggregates["Aggregates / Entities"]
        Value_Objects["Value Objects"]
        Rep_Interfaces["Repository Interfaces"]
    end

    subgraph Infrastructure_Layer ["Infrastructure Layer"]
        Repositories["Repositories Implementation"]
        DbContext["DbContext EF Core"]
        Cache["Distributed Cache Services"]
    end

    Controllers --> CommandServices
    Controllers --> QueryServices
    CommandServices --> Rep_Interfaces
    QueryServices --> Rep_Interfaces
    Repositories -.->|Implements| Rep_Interfaces
    Repositories --> DbContext
```

---

## 5. Vista de Comportamiento (Diagramas de Secuencia)

### A. Inicio de Sesión
```mermaid
sequenceDiagram
    actor Usuario
    participant SPA as "Web App (Vue.js)"
    participant GW as "API Gateway"
    participant Ctrl as "AuthController"
    participant Svc as "AuthQueryService"
    participant DB as "MySQL DB"

    Usuario->>SPA: Introduce Credenciales
    SPA->>GW: POST /api/auth/login
    GW->>Ctrl: POST /api/v1/auth/login
    Ctrl->>Svc: LoginAsync(request)
    Svc->>DB: Buscar usuario
    DB-->>Svc: Retorna usuario
    Svc->>Svc: Validar BCrypt Hash
    Svc->>Svc: Generar JWT
    Svc-->>Ctrl: Retorna Token & User info
    Ctrl-->>GW: 200 OK (JWT)
    GW-->>SPA: 200 OK (JWT)
```

### B. Registro de Movimiento de Stock
```mermaid
sequenceDiagram
    actor Operador
    participant SPA as "Web App"
    participant GW as "API Gateway"
    participant Ctrl as "StockMovementsController"
    participant Svc as "StockMovementCommandService"
    participant SRepo as "SupplyRepository"
    participant Subj as "InventorySubject"
    participant Broker as "RabbitMQ"

    Operador->>SPA: Modifica cantidad de supply
    SPA->>GW: POST /api/inventory/stockmovements
    GW->>Ctrl: POST /api/v1/inventory/stockmovements
    Ctrl->>Svc: Handle(CreateStockMovementCommand)
    Svc->>SRepo: FindByIdAsync(id)
    SRepo-->>Svc: Supply entity
    Svc->>Svc: DeductStock(quantity)
    Svc->>SRepo: Update(Supply)
    Svc->>Subj: NotifySupplyStockChangedAsync()
    Subj->>Broker: Publish Event
    Svc-->>Ctrl: Supply updated
    Ctrl-->>GW: 201 Created
    GW-->>SPA: 201 Created
```

---

## 6. Vista Física

```mermaid
flowchart TD
    subgraph Client ["Client Device"]
        SPA["Vue.js Web Application"]
    end

    subgraph Host ["Docker Host Server"]
        GW["gateway-service Container"]
        Auth["auth-service Container"]
        Inv["inventory-service Container"]
        Anal["analytics-service Container"]
        Prof["profiles-service Container"]
        Iot["iot-simulator-service Container"]
        MySQL[("MySQL Container")]
        Redis[("Redis Container")]
        RabbitMQ{"RabbitMQ Container"}
    end

    SPA -->|HTTPS Port 5000| GW
    GW -->|Port 8080| Auth
    GW -->|Port 8080| Inv
    GW -->|Port 8080| Anal
    GW -->|Port 8080| Prof

    Iot -->|HTTP| Auth
    Iot -->|HTTP| Inv

    Auth -->|Port 3306| MySQL
    Inv -->|Port 3306| MySQL
    Prof -->|Port 3306| MySQL

    Inv -->|AMQP Port 5672| RabbitMQ
    Anal -->|AMQP Port 5672| RabbitMQ

    Anal -->|Port 6379| Redis
```

---

## 7. Vista Modular

```mermaid
flowchart TD
    Gateway["WinesoftPlatform.GatewayService"] --> Shared["WinesoftPlatform.Shared"]
    Auth["WinesoftPlatform.AuthService"] --> Shared
    Inventory["WinesoftPlatform.InventoryService"] --> Shared
    Profiles["WinesoftPlatform.ProfilesService"] --> Shared
    Analytics["WinesoftPlatform.AnalyticsService"] --> Shared
    Simulator["WinesoftPlatform.IoTSimulatorService"] --> Shared
```

---

## 8. Vista de Despliegue

```mermaid
flowchart TD
    subgraph Vercel_CDN ["Vercel Cloud"]
        Frontend["Vue.js Static Assets"]
    end

    subgraph Render_Cloud ["Render Platform"]
        GW["API Gateway Service YARP"]
        Backend["Backend Containers Services"]
        MySQL[("Managed MySQL DB")]
        Redis[("Managed Redis Cache")]
        MQ{"Managed CloudAMQP RabbitMQ"}
    end

    Frontend -->|HTTPS REST API| GW
    GW -->|Internal HTTP| Backend
    Backend -->|SQL| MySQL
    Backend -->|TCP| Redis
    Backend -->|AMQP| MQ
```

---

## 9. Modelo de Datos (Diagrama ER)

```mermaid
erDiagram
    User ||--o| Profile : "Posee"
    User ||--o{ Supply : "Es dueño de"
    User ||--o{ SensorAlert : "Recibe"
    Supply ||--o{ StockMovement : "Registra"

    User {
        int id PK
        string username
        string email
        string password_hash
        string full_name
        string phone
    }

    Profile {
        int id PK
        string business_name
        string branch
        string street
        string number
        string city
        string postal_code
        string country
        string phone
        string legal_id
    }

    Supply {
        int id PK
        string supply_name
        int quantity
        string unit
        string supplier
        decimal price
        datetime date
        int owner_id FK
    }

    StockMovement {
        int id PK
        int supply_id FK
        int quantity
        string type
        string reason
        datetime date
        int owner_id
    }

    SensorAlert {
        int id PK
        string device_id
        string sensor_type
        double value
        string unit
        datetime timestamp
        string status
        boolean is_anomaly
        int owner_id
    }
```
