# Arquitectura Detallada del Backend de Vintara

Este documento explica de manera detallada el rol y la interacción de las **12 tecnologías clave** integradas en el backend de la plataforma **WineSoft (Vintara)**. Para cada tecnología se detalla su función, su método de implementación en el sistema, los archivos involucrados y su comportamiento mediante un gráfico de secuencia en formato PlantUML.

---

## 1. Variables de Entorno (`.env`)

### Función
Separar los datos de configuración sensible (contraseñas de base de datos, claves secretas, emisores de tokens) del código fuente y del repositorio público, inyectándolos en tiempo de ejecución.

### Cómo se cumple
Las contraseñas de las bases de datos y la clave del JWT se obtienen mediante la interfaz genérica `IConfiguration`. En el entorno local, se provee el archivo [.env.example](.env.example) para crear un archivo `.env` que Docker Compose lee y mapea como variables de entorno del sistema operativo dentro de los contenedores de cada servicio en [docker-compose.yml](docker-compose.yml).

### Archivos clave
* [.env.example](.env.example)
* [docker-compose.yml](docker-compose.yml)
* [AuthQueryService.cs](src/AuthService/WinesoftPlatform.AuthService/application/internal/queryservices/AuthQueryService.cs) (lee `Jwt:Key` dinámicamente)

### Diagrama de Secuencia
```plantuml
@startuml
autonumber
actor Host as "Host (Docker Engine)"
control DC as "Docker Compose"
participant App as "Microservicio ASP.NET Core"
database Config as "IConfiguration"

Host -> DC : Lee archivo .env local
DC -> App : Inyecta variables como env-vars en el contenedor
App -> Config : Carga las variables en builder.Configuration al iniciar
App -> Config : Consulta _configuration["Jwt:Key"]
Config --> App : Retorna valor del secreto en memoria
@enduml
```

![Diagrama de secuencia 1](../diagrams/01_variables_de_entorno.png)


---

## 2. Autenticación JWT Bearer

### Función
Proteger los endpoints del backend, permitiendo validar la identidad de los usuarios de forma descentralizada sin sobrecargar la base de datos en cada petición.

### Cómo se cumple
Cada microservicio independiente utiliza el middleware nativo `Microsoft.AspNetCore.Authentication.JwtBearer` para descifrar y validar localmente la firma del token recibido en la cabecera HTTP `Authorization: Bearer <token>`. La firma se valida utilizando la clave secreta compartida configurada mediante variables de entorno.

### Archivos clave
* [ProfilesService/Program.cs](src/ProfilesService/WinesoftPlatform.ProfilesService/Program.cs) (Configuración de JwtBearer)
* [PurchaseService/Program.cs](src/PurchaseService/WinesoftPlatform.PurchaseService/Program.cs) (Configuración de JwtBearer)
* [InventoryService/Program.cs](src/InventoryService/WinesoftPlatform.InventoryService/Program.cs) (Configuración de JwtBearer)
* [AnalyticsService/Program.cs](src/AnalyticsService/WinesoftPlatform.AnalyticsService/Program.cs) (Configuración de JwtBearer)

### Diagrama de Secuencia
```plantuml
@startuml
autonumber
actor Cliente
participant Gateway as "GatewayService (YARP)"
participant Auth as "AuthService"
participant API as "Microservicio (ej. Inventory)"

Cliente -> Gateway : POST /api/auth/login (credenciales)
Gateway -> Auth : Redirige petición
Auth --> Cliente : Retorna JWT firmado
Cliente -> Gateway : GET /api/inventory/supplies (Header: Bearer JWT)
Gateway -> API : Redirige petición con Header intacto
Note over API : JwtBearerMiddleware valida firma, emisor y expiración
alt JWT Válido
    API --> Cliente : Retorna 200 OK + Suministros
else JWT Inválido/Expirado
    API --> Cliente : Retorna 401 Unauthorized
end
@enduml
```

![Diagrama de secuencia 2](../diagrams/02_autenticacion_jwt_bearer.png)


---

## 3. Aislamiento Multi-Tenant por `OwnerId`

### Función
Garantizar la privacidad lógica de los datos entre diferentes dueños de negocio (tenants), impidiendo que un usuario acceda, modifique o visualice registros pertenecientes a otro (vulnerabilidades IDOR).

### Cómo se cumple
Las entidades de base de datos como `Supply` y `Order` poseen un atributo de persistencia `OwnerId`. En la capa de controladores web, se extrae el ID del usuario directamente desde el token JWT previamente validado, inyectándolo de manera forzosa en los parámetros de los comandos y consultas que se dirigen a los repositorios de persistencia.

### Archivos clave
* [PurchaseOrdersController.cs](src/PurchaseService/WinesoftPlatform.PurchaseService/Interfaces/REST/PurchaseOrdersController.cs) (método `GetOwnerId`)
* [SuppliesController.cs](src/InventoryService/WinesoftPlatform.InventoryService/Interfaces/REST/SuppliesController.cs) (método `GetOwnerId`)
* [OrderRepository.cs](src/PurchaseService/WinesoftPlatform.PurchaseService/Infrastructure/Persistence/EFC/Repositories/OrderRepository.cs) (filtra queries con `Where(o => o.OwnerId == ownerId)`)
* [SupplyRepository.cs](src/InventoryService/WinesoftPlatform.InventoryService/Infrastructure/Persistence/EFC/Repositories/SupplyRepository.cs) (filtra queries con `Where(s => s.OwnerId == ownerId)`)

### Diagrama de Secuencia
```plantuml
@startuml
autonumber
actor Cliente
participant Controller as "PurchaseOrdersController"
participant Repo as "OrderRepository"
database DB as "MySQL Database"

Cliente -> Controller : GET /api/v1/purchase-orders (JWT Bearer)
Note over Controller : Extrae claim "sid" o "sub" del JWT\npara obtener el OwnerId (ej. OwnerId = 15)
Controller -> Repo : ListByOwnerIdAsync(ownerId = 15)
Repo -> DB : SELECT * FROM orders WHERE owner_id = 15
DB --> Repo : Registros filtrados de base de datos
Repo --> Controller : Lista de órdenes filtradas
Controller --> Cliente : Retorna 200 OK + Órdenes del Tenant 15
@enduml
```

![Diagrama de secuencia 3](../diagrams/03_aislamiento_multi_tenant.png)


---

## 4. Polly (Microsoft.Extensions.Http.Resilience)

### Función
Asegurar la tolerancia a fallos transitorios en las comunicaciones síncronas HTTP entre microservicios, previniendo caídas en cadena en el ecosistema.

### Cómo se cumple
Se decora el cliente HTTP utilizado por `PurchaseService` hacia `InventoryService` utilizando `.AddStandardResilienceHandler` provisto por Polly. Este manejador configura de manera automática políticas de reintentos rápidos con esperas incrementales (Backoff exponencial) y límites de tiempo por llamada.

### Archivos clave
* [PurchaseService/Program.cs](src/PurchaseService/WinesoftPlatform.PurchaseService/Program.cs) (registro de `AddStandardResilienceHandler`)
* [WinesoftPlatform.PurchaseService.csproj](src/PurchaseService/WinesoftPlatform.PurchaseService/WinesoftPlatform.PurchaseService.csproj) (librería `Microsoft.Extensions.Http.Resilience`)

### Diagrama de Secuencia
```plantuml
@startuml
autonumber
participant Purchase as "PurchaseService"
participant Polly as "Polly Resilience Handler"
participant Inventory as "InventoryService"

Purchase -> Polly : GET /api/v1/inventory/supplies/5 (Solicita nombre del insumo)
Polly -> Inventory : Envía petición HTTP (Intento 1)
Note over Inventory : Inventory temporalmente caído o lento
Inventory --> Polly : Retorna 503 Service Unavailable o Timeout
Note over Polly : Polly espera 2 segundos (Backoff exponencial)
Polly -> Inventory : Envía petición HTTP (Intento 2)
Inventory --> Polly : Retorna 200 OK + Datos
Polly --> Purchase : Retorna respuesta exitosa
@enduml
```

![Diagrama de secuencia 4](../diagrams/04_polly_resilience.png)


---

## 5. Rate Limiting (Microsoft.AspNetCore.RateLimiting)

### Función
Proteger el sistema de sobrecargas intencionales o accidentales, ataques de denegación de servicio (DoS) y ataques de fuerza bruta.

### Cómo se cumple
Se definen políticas de límite de peticiones de tipo ventana fija:
* `AuthRateLimit` en `AuthService`: Limita a 5 peticiones por minuto por IP remota para los endpoints críticos de autenticación.
* `GatewayRateLimit` en `GatewayService`: Protege el punto de entrada global limitando a 60 peticiones por minuto por IP remota para todo el tráfico.

### Archivos clave
* [GatewayService/Program.cs](src/GatewayService/WinesoftPlatform.GatewayService/Program.cs) (política `GatewayRateLimit`)
* [AuthService/Program.cs](src/AuthService/WinesoftPlatform.AuthService/Program.cs) (política `AuthRateLimit`)
* [AuthController.cs](src/AuthService/WinesoftPlatform.AuthService/interfaces/REST/AuthController.cs) (atributo `[EnableRateLimiting("AuthRateLimit")]`)

### Diagrama de Secuencia
```plantuml
@startuml
autonumber
actor Cliente
participant Gateway as "GatewayService (YARP)"
participant API as "Microservicio Interno"

loop Menos de 60 peticiones en 1 minuto
    Cliente -> Gateway : Envía Request HTTP
    Gateway -> Gateway : Incrementa contador de IP
    Gateway -> API : Redirige petición
    API --> Cliente : Retorna 200 OK
end

Cliente -> Gateway : Envía Request HTTP (Petición 61 en el mismo minuto)
Gateway -> Gateway : Detecta IP bloqueada temporalmente
Gateway --> Cliente : Retorna 429 Too Many Requests
@enduml
```

![Diagrama de secuencia 5](../diagrams/05_rate_limiting.png)


---

## 6. YARP (Yet Another Reverse Proxy)

### Función
Proveer un único punto de entrada unificado (API Gateway) para los clientes externos, ocultando los puertos y la topología interna de la red de microservicios.

### Cómo se cumple
Se utiliza el reverse proxy YARP en `GatewayService`. Escucha peticiones externas en el puerto `5000` y las enruta dinámicamente hacia las direcciones de Docker internas de cada microservicio en el puerto `8080` (ej: `http://auth-service:8080`), reescribiendo los prefijos de las rutas según la configuración establecida.

### Archivos clave
* [GatewayService/Program.cs](src/GatewayService/WinesoftPlatform.GatewayService/Program.cs) (registro de proxy)
* [GatewayService/appsettings.json](src/GatewayService/WinesoftPlatform.GatewayService/appsettings.json) (definición de rutas y clusters)

### Diagrama de Secuencia
```plantuml
@startuml
autonumber
actor FE as "Frontend (Cliente)"
participant GW as "GatewayService (YARP)"
participant Auth as "AuthService"
participant Inv as "InventoryService"

FE -> GW : GET http://localhost:5000/api/auth/login
GW -> GW : Mapea ruta "auth-route" -> "auth-cluster"
GW -> Auth : Proxy a http://auth-service:8080/api/v1/auth/login
Auth --> GW : Retorna JWT
GW --> FE : Retorna JWT original al cliente

FE -> GW : GET http://localhost:5000/api/inventory/supplies
GW -> GW : Mapea ruta "inventory-route" -> "inventory-cluster"
GW -> Inv : Proxy a http://inventory-service:8080/api/v1/inventory/supplies
Inv --> GW : Retorna Suministros
GW --> FE : Retorna respuesta original
@enduml
```

![Diagrama de secuencia 6](../diagrams/06_yarp_gateway.png)


---

## 7. Serilog + Correlation ID

### Función
Facilitar el rastreo y análisis de logs en sistemas distribuidos, permitiendo correlacionar múltiples entradas de log generadas en diferentes microservicios como resultado de una misma solicitud de usuario.

### Cómo se cumple
Un middleware transversal intercepta cada petición en el API Gateway. Si no existe un identificador `X-Correlation-Id`, se genera un UUID único y se adjunta a las cabeceras HTTP. Cuando el Gateway delega la petición a los microservicios internos mediante YARP, la cabecera se propaga. Los microservicios leen esta cabecera y empujan el valor en el contexto de diagnóstico de Serilog (`LogContext`).

### Archivos clave
* [CorrelationIdMiddleware.cs](src/Shared/WinesoftPlatform.Shared/Infrastructure/Middleware/CorrelationIdMiddleware.cs) (lógica del middleware)
* [GatewayService/Program.cs](src/GatewayService/WinesoftPlatform.GatewayService/Program.cs) (uso de middleware y configuración de logger)

### Diagrama de Secuencia
```plantuml
@startuml
autonumber
actor Cliente
participant Gateway as "GatewayService (YARP)"
participant Mid as "CorrelationIdMiddleware"
participant Purchase as "PurchaseService"

Cliente -> Gateway : POST /api/purchases (sin Correlation ID)
Gateway -> Mid : Procesa petición
Note over Mid : Genera CorrelationId = "c123-abc"
Mid -> Mid : Añade CorrelationId a las cabeceras e inyecta en LogContext de Gateway
Note over Gateway : YARP propaga el header X-Correlation-Id
Gateway -> Purchase : POST http://purchase-service:8080/api/v1/purchase-orders (Header: X-Correlation-Id: c123-abc)
Purchase -> Mid : Procesa petición en PurchaseService
Note over Mid : Lee cabecera "c123-abc" y la empuja al LogContext local
Purchase -> Purchase : Registra compra (Serilog escribe log con el tag [c123-abc])
Purchase --> Cliente : Retorna 201 Created (incluye CorrelationId en respuesta)
@enduml
```

![Diagrama de secuencia 7](../diagrams/07_serilog_correlation_id.png)


---

## 8. Migraciones formales de Entity Framework Core

### Función
Gestionar la evolución del esquema de la base de datos de manera incremental, controlada y versionada, evitando pérdidas de datos en entornos de desarrollo y producción.

### Cómo se cumple
Se reemplazó el uso de la función destructiva `EnsureCreated()` por un esquema formal de migraciones de EF Core. Durante la compilación y arranque de cada contenedor de microservicio, se ejecuta la migración pendiente de base de datos MySQL de forma automática mediante la llamada `dbContext.Database.Migrate()`.

### Archivos clave
* [InventoryDbContextModelSnapshot.cs](src/InventoryService/WinesoftPlatform.InventoryService/Migrations/InventoryDbContextModelSnapshot.cs)
* [PurchaseDbContextModelSnapshot.cs](src/PurchaseService/WinesoftPlatform.PurchaseService/Migrations/PurchaseDbContextModelSnapshot.cs)
* [InventoryService/Program.cs](src/InventoryService/WinesoftPlatform.InventoryService/Program.cs) (llamada `Database.Migrate()`)
* [PurchaseService/Program.cs](src/PurchaseService/WinesoftPlatform.PurchaseService/Program.cs) (llamada `Database.Migrate()`)

### Diagrama de Secuencia
```plantuml
@startuml
autonumber
participant App as "Microservicio ASP.NET"
participant EF as "Entity Framework Core"
database DB as "MySQL Database"

App -> EF : Llama a dbContext.Database.Migrate() en el arranque
EF -> DB : Consulta la tabla interna __EFMigrationsHistory
DB --> EF : Retorna lista de migraciones registradas
Note over EF : Evalúa diferencias con las clases compiladas en local
alt Existen migraciones pendientes
    EF -> DB : Aplica scripts SQL incrementales (ej: añadir columna)
    DB --> EF : Éxito en ejecución de consultas
    EF -> DB : Inserta registro de nueva migración en __EFMigrationsHistory
end
EF --> App : Base de datos lista para operar
@enduml
```

![Diagrama de secuencia 8](../diagrams/08_migraciones_ef_core.png)


---

## 9. GitHub Actions (.github/workflows/ci.yml)

### Función
Automatizar las validaciones de integración continua (CI) en el repositorio para evitar mezclar cambios que rompan la compilación, introduzcan vulnerabilidades NuGet conocidas o impidan la generación de las imágenes Docker.

### Cómo se cumple
El workflow está configurado en [.github/workflows/ci.yml](.github/workflows/ci.yml). Al realizar un push o pull request a las ramas `main` o `develop`, se inicia un agente runner que restaura paquetes, compila el proyecto controlando warnings críticos (`/warnaserror:NU1902`), ejecuta las pruebas y construye localmente los archivos Docker para cada servicio.

### Archivos clave
* [ci.yml](.github/workflows/ci.yml)

### Diagrama de Secuencia
```plantuml
@startuml
autonumber
actor Dev as "Programador"
participant Repo as "GitHub Repository"
participant Runner as "GitHub Actions Runner"
participant Docker as "Docker CLI Builder"

Dev -> Repo : Realiza push a la rama "develop"
Repo -> Runner : Dispara evento y reserva contenedor de compilación
Runner -> Runner : Descarga repositorio (actions/checkout)
Runner -> Runner : Configura SDK de .NET (10.0.x)
Runner -> Runner : Ejecuta dotnet restore & dotnet build (con flag de bloqueo en vulnerabilidades)
Runner -> Runner : Corre batería de pruebas unitarias
Runner -> Docker : Construye imágenes Docker de microservicios usando los Dockerfile
Docker --> Runner : Imágenes construidas con éxito
Runner --> Repo : Reporta estado exitoso (Pasa el check de CI en la PR)
@enduml
```

![Diagrama de secuencia 9](../diagrams/09_github_actions.png)


---

## 10. Actualización de paquetes JWT (Vulnerabilidad CVE-2024-21319)

### Función
Resolver vulnerabilidades críticas conocidas que podrían derivar en un ataque de denegación de servicio (DoS) o evasión de controles al validar tokens JSON Web Token.

### Cómo se cumple
Se actualizaron las referencias de paquetes en los archivos `.csproj` a versiones seguras. El compilador de dotnet controla que no se utilicen versiones vulnerables de estas librerías mediante la validación estricta de errores NuGet del compilador (`/warnaserror:NU1902`) que se ejecuta durante el pipeline en GitHub Actions.

### Archivos clave
* [WinesoftPlatform.AuthService.csproj](src/AuthService/WinesoftPlatform.AuthService/WinesoftPlatform.AuthService.csproj) (paquete `System.IdentityModel.Tokens.Jwt` en versión `8.0.0` o superior)
* [WinesoftPlatform.InventoryService.csproj](src/InventoryService/WinesoftPlatform.InventoryService/WinesoftPlatform.InventoryService.csproj) (paquete `Microsoft.AspNetCore.Authentication.JwtBearer` en versión `9.0.10` o superior)

### Diagrama de Secuencia
```plantuml
@startuml
autonumber
participant NuGet as "NuGet Registry"
participant Csproj as ".csproj File"
participant Compiler as "Dotnet Compiler / CI"

Csproj -> NuGet : Solicita versiones seguras
NuGet --> Csproj : Entrega paquetes actualizados
Compiler -> Csproj : Compila el código con flag --warnaserror:NU1902
Note over Compiler : El compilador audita dependencias locales
alt Paquetes vulnerables (v6.30.0) detectados
    Compiler --> Csproj : Lanza error NU1902 y cancela compilación
else Paquetes seguros (v8.0.0 / v9.0.10) detectados
    Compiler --> Csproj : Genera binarios listos de forma exitosa
end
@enduml
```

![Diagrama de secuencia 10](../diagrams/10_actualizacion_paquetes_jwt.png)


---

## 11. RabbitMQ + MassTransit

### Función
Habilitar una arquitectura guiada por eventos asíncronos para desacoplar los servicios en operaciones de escritura, aumentando la disponibilidad y escalabilidad horizontal del backend.

### Cómo se cumple
Cuando `PurchaseService` crea una orden, publica de manera asíncrona un evento de integración `OrderCreated` a RabbitMQ utilizando la abstracción de MassTransit. Los microservicios interesados (`InventoryService` y `AnalyticsService`) implementan consumidores (`IConsumer<OrderCreated>`) para actualizar sus estados locales en segundo plano, sin requerir una comunicación síncrona en tiempo real.

### Archivos clave
* [OrderCreated.cs](src/Shared/WinesoftPlatform.Shared/Domain/Events/OrderCreated.cs) (clase del evento)
* [OrderCommandService.cs](src/PurchaseService/WinesoftPlatform.PurchaseService/Application/Internal/CommandServices/OrderCommandService.cs) (publica evento al confirmar compra)
* [OrderCreatedConsumer.cs (Inventory)](src/InventoryService/WinesoftPlatform.InventoryService/Application/Internal/Consumers/OrderCreatedConsumer.cs) (resta stock de insumos)
* [OrderCreatedConsumer.cs (Analytics)](src/AnalyticsService/WinesoftPlatform.AnalyticsService/Application/Internal/Consumers/OrderCreatedConsumer.cs) (invalida reportes estadísticos y actualiza KPIs)

### Diagrama de Secuencia
```plantuml
@startuml
autonumber
actor Cliente
participant Purchase as "PurchaseService"
queue Rabbit as "RabbitMQ (Broker)"
participant Inventory as "InventoryService"
participant Analytics as "AnalyticsService"

Cliente -> Purchase : POST /api/purchases (Crea Orden de Compra)
Purchase -> Purchase : Guarda orden localmente en BD MySQL
Purchase -> Rabbit : Publica evento asíncrono "OrderCreated" (vía MassTransit)
Purchase --> Cliente : Retorna 201 Created (inmediato)

par Distribución Asíncrona
    Rabbit -> Inventory : Distribuye evento a cola de Inventory
    Inventory -> Inventory : Ejecuta OrderCreatedConsumer (Descuenta stock físico)
else Distribución Asíncrona
    Rabbit -> Analytics : Transfiere a cola de Analytics
    Analytics -> Analytics : Ejecuta OrderCreatedConsumer (Recalcula métricas de compra)
end
@enduml
```

![Diagrama de secuencia 11](../diagrams/11_rabbitmq_masstransit.png)

---

## 12. Redis

### Función
Cachear en memoria estructurada de lectura rápida los reportes complejos generados por el microservicio de analítica, reduciendo peticiones duplicadas y carga de cálculo en MySQL.

### Cómo se cumple
Se inyecta la abstracción `IDistributedCache` integrada con StackExchange.Redis en `AnalyticsService`. Al realizar una solicitud de reportes o KPIs, se comprueba si la información serializada está disponible en Redis bajo una llave que incluye el ID del dueño de negocio (`OwnerId`). Si no existe (Cache Miss), se realiza la lectura y agregación pesada en la base de datos MySQL, guardando el resultado serializado en caché con un tiempo de expiración determinado.

### Archivos clave
* [AnalyticsCacheService.cs](src/AnalyticsService/WinesoftPlatform.AnalyticsService/Infrastructure/Services/AnalyticsCacheService.cs) (abstracción del caché)
* [AnalyticsQueryService.cs](src/AnalyticsService/WinesoftPlatform.AnalyticsService/Application/Internal/QueryServices/AnalyticsQueryService.cs) (gestiona cache hits y miss)
* [AnalyticsService/Program.cs](src/AnalyticsService/WinesoftPlatform.AnalyticsService/Program.cs) (registro de `AddStackExchangeRedisCache`)

### Diagrama de Secuencia
```plantuml
@startuml
autonumber
actor Cliente
participant Analytics as "AnalyticsQueryService"
database Cache as "Redis Cache"
database DB as "MySQL Database"

Cliente -> Analytics : GET /api/v1/analytics/kpis (JWT Bearer)
Note over Analytics : Extrae OwnerId = 15
Analytics -> Cache : Consulta llave "KPI_Report_Owner_15"
alt Cache Hit (Llave existe en Redis)
    Cache --> Analytics : Retorna JSON del reporte cacheado
    Analytics --> Cliente : Retorna 200 OK + Reporte (Latencia < 15ms)
else Cache Miss (Llave ausente o expirada)
    Cache --> Analytics : Retorna nulo
    Analytics -> DB : Realiza consulta SQL compleja con agregados
    DB --> Analytics : Retorna registros crudos
    Analytics -> Analytics : Calcula y genera JSON del reporte
    Analytics -> Cache : Guarda "KPI_Report_Owner_15" (Expira en 10 minutos)
    Analytics --> Cliente : Retorna 200 OK + Reporte (Latencia > 500ms)
end
@enduml
```

![Diagrama de secuencia 12](../diagrams/12_redis_cache.png)

