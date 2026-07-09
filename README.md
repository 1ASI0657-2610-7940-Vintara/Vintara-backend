# Vintara - WineSoft Platform (Backend)

Este repositorio contiene el código fuente del backend (Server-side) para **WineSoft**, una plataforma de gestión de inventarios y pedidos B2B orientada al sector de licores. El sistema está diseñado utilizando una arquitectura basada en **Microservicios** y **Domain-Driven Design (DDD)**.

## Tecnologías y Herramientas

* **Framework Core:** .NET 10 (C#)
* **Arquitectura:** Microservicios, Domain-Driven Design (DDD), CQRS, RESTful APIs, YARP API Gateway.
* **Persistencia de Datos:** MySQL y Entity Framework Core (Migrations).
* **Despliegue y Orquestación:** Docker, Docker Compose y GitHub Actions (CI).
* **Mensajería Asíncrona:** RabbitMQ y MassTransit.
* **Caché en Memoria:** Redis.
* **Seguridad y Resiliencia:** Autenticación JWT Bearer, Rate Limiting y Polly (Http Resilience).
* **Observabilidad:** Serilog y Correlation ID.
* **Documentación de API:** Swagger (OpenAPI Specification).
* **Generación de Reportes:** QuestPDF.

> [!TIP]
> Para conocer en detalle cómo se implementan cada una de estas tecnologías en la arquitectura del backend, sus diagramas de secuencia e interacciones, consulta el documento de [Arquitectura Detallada](docs/architecture/Arquitectura_Detallada.md).

##  Estructura del Proyecto (Bounded Contexts)

La solución está dividida en microservicios independientes para asegurar la cohesión, alta disponibilidad y bajo acoplamiento:

* **`GatewayService`**: API Gateway centralizado que unifica la entrada al sistema, gestiona CORS a nivel global y redirecciona el tráfico a los servicios internos mediante YARP.
* **`AnalyticsService`**: Generación de reportes PDF (QuestPDF), KPIs de rotación, niveles de suministros y alertas de bajo stock.
* **`AuthService`**: Gestión de identidad, registro, inicio de sesión y emisión de tokens JWT.
* **`InventoryService`**: Control de suministros (Supplies), stock físico y recepción de eventos/telemetría IoT (Observer).
* **`ProfilesService`**: Gestión de perfiles de usuario (Dueño de negocio, Proveedor) y datos fiscales.
* **`Shared`**: Lógica transversal, interfaces de repositorios base y configuración de Entity Framework.

## 🌐 Puertos y API Gateway

La plataforma utiliza **YARP (Yet Another Reverse Proxy)** para unificar el acceso de los clientes y el frontend en un único punto de entrada:

| Servicio | Puerto Interno (Docker) | Puerto Expuesto Local | Ruta en Gateway |
|---|---|---|---|
| **Gateway (YARP)** | 8080 | **5000** | `/` (Entrada Principal) |
| `AuthService` | 8080 | 5001 | `/api/auth/*` |
| `InventoryService` | 8080 | 5002 | `/api/inventory/*` |
| `ProfilesService` | 8080 | 5004 | `/api/profiles/*` |
| `AnalyticsService` | 8080 | 5005 | `/api/analytics/*` |
| `IoTSimulator` | 8080 | 5006 | - |

> [!IMPORTANT]
> **El frontend debe consumir únicamente el Gateway en el puerto `5000`** (`http://localhost:5000`). No se debe intentar consumir los puertos individuales directamente (5001-5006) en producción o desarrollo de frontend, ya que las políticas CORS están centralizadas y controladas únicamente a través del Gateway.
##  Requisitos Previos

Antes de ejecutar el proyecto, asegúrate de tener instalado:
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* [Docker Desktop](https://www.docker.com/products/docker-desktop/)
* IDE Recomendado: Visual Studio 2022, JetBrains Rider o VS Code.

##  Instalación y Ejecución Local

El proyecto está configurado para ejecutarse nativamente en contenedores mediante `docker-compose`, incluyendo la base de datos PostgreSQL (`init.sql`).

1. **Clonar el repositorio:**
   ```bash
   git clone [https://github.com/1ASI0657-2610-7940-Vintara/Vintara-backend.git](https://github.com/1ASI0657-2610-7940-Vintara/Vintara-backend.git)
   cd vintara-backend
### Bloque 4: Normas de Colaboración (GitFlow y Commits)
```markdown
##  Convenciones de Desarrollo (Software Configuration Management)

El equipo sigue pautas estrictas de SCM para mantener la calidad y trazabilidad del código:

### 1. Estrategia de Ramas (GitFlow)
* `main`: Rama de producción (Releases estables con Semantic Versioning, ej. `v1.0.0`).
* `develop`: Rama de integración principal.
* `feature/<nombre-tarea>`: Ramas para el desarrollo de nuevas características (ej. `feature/inventory-iot`).

### 2. Mensajes de Commit (Conventional Commits)
Todos los commits deben seguir este formato:
* `feat:` Nuevas funcionalidades (ej. `feat: agregar endpoint de alertas IoT`).
* `fix:` Corrección de errores.
* `test:` Adición de pruebas BDD/Gherkin (ej. `test: escenarios de inventario`).
* `docs:` Cambios en la documentación.
* `refactor:` Refactorización de código sin alterar comportamiento.

## Equipo de Desarrollo (Vintara)
* **Joan Fernando Teves Samaniego** - *Backend & Cloud / IoT*
* **Antonio Rodrigo Duran Diaz** - *Backend & Security*
* **John Árevalo Meza** - *Backend & DevOps*
