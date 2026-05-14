# Vintara - WineSoft Platform (Backend)

Este repositorio contiene el código fuente del backend (Server-side) para **WineSoft**, una plataforma de gestión de inventarios y pedidos B2B orientada al sector de licores. El sistema está diseñado utilizando una arquitectura basada en **Microservicios** y **Domain-Driven Design (DDD)**.

## 🚀 Tecnologías y Herramientas

* **Framework Core:** .NET 8 (C#)
* **Arquitectura:** Microservicios, Domain-Driven Design (DDD), CQRS, RESTful APIs.
* **Persistencia de Datos:** PostgreSQL y Entity Framework Core (Code-First).
* **Despliegue y Orquestación:** Docker y Docker Compose.
* **Documentación de API:** Swagger (OpenAPI Specification).
* **Generación de Reportes:** QuestPDF.

##  Estructura del Proyecto (Bounded Contexts)
La solución se divide en contextos delimitados (Bounded Contexts) para asegurar la cohesión y el bajo acoplamiento:

```text
src/
 ├── AnalyticsService/       # Reportes, proyecciones y analíticas del inventario.
 ├── AuthService/            # Gestión de identidad, registro y autenticación JWT.
 ├── InventoryService/       # Control de suministros, stock y simulación de alertas IoT.
 ├── ProfilesService/        # Gestión de perfiles de usuario (Dueño de negocio, Proveedor).
 ├── PurchaseService/        # Orquestación y gestión de órdenes de compra.
 └── Shared/                 # Lógica transversal, interfaces compartidas y utilidades.
##  Estructura del Proyecto (Bounded Contexts)

La solución está dividida en microservicios independientes para asegurar la cohesión, alta disponibilidad y bajo acoplamiento:

* **`AnalyticsService`**: Generación de reportes PDF (QuestPDF), KPIs de rotación, niveles de suministros y alertas de bajo stock.
* **`AuthService`**: Gestión de identidad, registro, inicio de sesión y emisión de tokens JWT.
* **`InventoryService`**: Control de suministros (Supplies), stock físico y recepción de eventos/telemetría IoT (Observer).
* **`ProfilesService`**: Gestión de perfiles de usuario (Dueño de negocio, Proveedor) y datos fiscales.
* **`PurchaseService`**: Orquestación y gestión de órdenes de compra.
* **`Shared`**: Lógica transversal, interfaces de repositorios base y configuración de Entity Framework.
##  Requisitos Previos

Antes de ejecutar el proyecto, asegúrate de tener instalado:
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* [Docker Desktop](https://www.docker.com/products/docker-desktop/)
* IDE Recomendado: Visual Studio 2022, JetBrains Rider o VS Code.

##  Instalación y Ejecución Local

El proyecto está configurado para ejecutarse nativamente en contenedores mediante `docker-compose`, incluyendo la base de datos PostgreSQL (`init.sql`).

1. **Clonar el repositorio:**
   ```bash
   git clone [https://github.com/TU_ORGANIZACION/vintara-backend.git](https://github.com/TU_ORGANIZACION/vintara-backend.git)
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
