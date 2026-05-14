# 🍷 Vintara - WineSoft Platform (Backend)

Este repositorio contiene el código fuente del backend (Server-side) para **WineSoft**, una plataforma de gestión de inventarios y pedidos B2B orientada al sector de licores. El sistema está diseñado utilizando una arquitectura basada en **Microservicios** y **Domain-Driven Design (DDD)**.

## 🚀 Tecnologías y Herramientas

* **Framework Core:** .NET 8 (C#)
* **Arquitectura:** Microservicios, Domain-Driven Design (DDD), CQRS, RESTful APIs.
* **Persistencia de Datos:** PostgreSQL y Entity Framework Core (Code-First).
* **Despliegue y Orquestación:** Docker y Docker Compose.
* **Documentación de API:** Swagger (OpenAPI Specification).
* **Generación de Reportes:** QuestPDF.

## 📂 Estructura del Proyecto (Bounded Contexts)
La solución se divide en contextos delimitados (Bounded Contexts) para asegurar la cohesión y el bajo acoplamiento:

```text
src/
 ├── AnalyticsService/       # Reportes, proyecciones y analíticas del inventario.
 ├── AuthService/            # Gestión de identidad, registro y autenticación JWT.
 ├── InventoryService/       # Control de suministros, stock y simulación de alertas IoT.
 ├── ProfilesService/        # Gestión de perfiles de usuario (Dueño de negocio, Proveedor).
 ├── PurchaseService/        # Orquestación y gestión de órdenes de compra.
 └── Shared/                 # Lógica transversal, interfaces compartidas y utilidades.
