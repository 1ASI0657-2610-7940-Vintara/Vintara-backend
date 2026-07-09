TECNOLOGÍAS IMPLEMENTADAS EN EL BACKEND DE VINTARA
====================================================
Proyecto: WineSoft (Vintara-backend)
Contexto: Refuerzo de arquitectura Cloud Native y seguridad del backend

----------------------------------------------------
1. VARIABLES DE ENTORNO (.env)
----------------------------------------------------
Para qué sirve:
Permite separar la configuración sensible (contraseñas, claves secretas)
del código fuente, inyectándola desde el entorno de ejecución en vez de
escribirla directamente en los archivos del repositorio.

Por qué fue necesaria en este backend:
La clave secreta usada para firmar los JWT estaba escrita directamente
en el código (AuthQueryService.cs), con un valor de ejemplo copiado de
otro proyecto. La contraseña de la base de datos (MYSQL_ROOT_PASSWORD)
también estaba escrita en texto plano dentro de docker-compose.yml.
Como el repositorio es público en GitHub, cualquier persona podía ver
ambos secretos. Se movieron a variables de entorno para que cada
despliegue use sus propios valores, sin exponerlos en el código.

----------------------------------------------------
2. AUTENTICACIÓN JWT BEARER EN TODOS LOS MICROSERVICIOS
   (Microsoft.AspNetCore.Authentication.JwtBearer)
----------------------------------------------------
Para qué sirve:
Permite que un microservicio valide el token (JWT) que recibe en cada
petición, confirmando que la persona que llama está realmente
autenticada antes de ejecutar cualquier acción.

Por qué fue necesaria en este backend:
AuthService generaba el token de login correctamente, pero ningún otro
microservicio (Inventory, Purchase, Profiles, Analytics) lo validaba.
Esto significaba que cualquier persona podía llamar directamente a esos
servicios sin haber iniciado sesión, sin que el sistema lo detectara.
Se agregó la validación de JWT en cada uno de ellos para que solo
usuarios autenticados puedan usar la plataforma.

----------------------------------------------------
3. AISLAMIENTO MULTI-TENANT POR OwnerId
----------------------------------------------------
Para qué sirve:
Garantiza que cada dueño de negocio solo pueda ver y modificar sus
propios datos (su inventario, sus compras), nunca los de otro dueño
que use la misma plataforma.

Por qué fue necesaria en este backend:
El segmento objetivo de WineSoft es exclusivamente dueños de negocio
(no hay distintos roles como "proveedor" o "administrador"), pero cada
dueño es un inquilino independiente del sistema. Antes de este cambio,
los registros de Supply y Order no tenían ningún campo que los asociara
a un dueño específico: cualquier usuario autenticado podía ver el
inventario de cualquier otro, incluso accediendo directamente por ID
(vulnerabilidad de tipo IDOR). Se agregó el campo OwnerId, derivado
únicamente del JWT del usuario (nunca de un parámetro enviado por el
cliente), y se filtran todas las consultas por ese valor.

----------------------------------------------------
4. POLLY (Microsoft.Extensions.Http.Resilience)
----------------------------------------------------
Para qué sirve:
Agrega reintentos automáticos, tiempo de espera límite y un "circuit
breaker" (disyuntor) a las llamadas HTTP entre microservicios, evitando
que la falla de un servicio tumbe en cadena a los demás.

Por qué fue necesaria en este backend:
PurchaseService llama directamente por HTTP a InventoryService para
obtener el nombre de un insumo. El propio código tenía un comentario
señalando esto como deuda técnica pendiente ("in a real scenario you
would have proper error handling, Polly retries"). Sin esta protección,
si InventoryService fallaba o se demoraba, PurchaseService podía
colgarse o devolver un error en cascada. Con Polly, ante un fallo se
devuelve un resultado degradado controlado en vez de romper el flujo.

----------------------------------------------------
5. RATE LIMITING (Microsoft.AspNetCore.RateLimiting, nativo en .NET)
----------------------------------------------------
Para qué sirve:
Limita la cantidad de peticiones que una misma IP puede hacer a un
endpoint en un periodo de tiempo determinado.

Por qué fue necesaria en este backend:
Los endpoints de login y registro de AuthService no tenían ninguna
protección contra intentos repetidos, dejándolos vulnerables a ataques
de fuerza bruta o credential stuffing. Se configuró un límite de 5
intentos por minuto por IP en login/register, y un límite general de
60 peticiones por minuto en el Gateway para proteger a todos los
servicios.

----------------------------------------------------
6. YARP — API GATEWAY NATIVO DE .NET
----------------------------------------------------
Para qué sirve:
Centraliza el acceso a todos los microservicios bajo un único punto de
entrada, en lugar de exponer cada uno por separado. También permite
unificar configuración común como CORS y límites de peticiones.

Por qué fue necesaria en este backend:
El frontend tenía que conocer y consumir directamente los 5-6 puertos
distintos de cada microservicio (5001 a 5006), lo cual expone la
topología interna del sistema y obliga a repetir configuración (como
CORS) en cada servicio. Con YARP se creó un GatewayService que expone
un único puerto (5000) y enruta internamente cada petición al
microservicio correspondiente.

----------------------------------------------------
7. SERILOG + CORRELATION ID
----------------------------------------------------
Para qué sirve:
Serilog estructura y centraliza los logs de cada microservicio. El
Correlation ID es un identificador único que se genera al entrar una
petición al Gateway y se propaga a través de todos los microservicios
que participan en esa misma operación, permitiendo rastrear el camino
completo de una petición en los logs de cada servicio.

Por qué fue necesaria en este backend:
Con 6 microservicios llamándose entre sí (por ejemplo, Purchase llama a
Inventory), un fallo en cualquier punto de esa cadena era difícil de
rastrear porque cada servicio generaba sus logs por separado, sin forma
de saber qué logs correspondían a la misma petición original.

----------------------------------------------------
8. MIGRACIONES FORMALES DE ENTITY FRAMEWORK CORE
----------------------------------------------------
Para qué sirve:
Permite aplicar cambios al esquema de la base de datos (nuevas tablas,
columnas, etc.) de forma controlada y versionada, sin perder los datos
que ya existen.

Por qué fue necesaria en este backend:
El proyecto usaba EnsureCreated(), que solo crea el esquema si la base
de datos está vacía, pero no sabe aplicar cambios sobre una base de
datos que ya tiene datos. Al agregar el campo OwnerId (ver punto 3), fue
necesario borrar por completo los volúmenes de Docker para que el
cambio se aplicara, lo cual no es viable en un entorno con datos reales.
Se migró a migraciones formales de EF Core (dotnet ef migrations add)
para poder evolucionar el esquema sin perder información.

----------------------------------------------------
9. GITHUB ACTIONS (.github/workflows/ci.yml)
----------------------------------------------------
Para qué sirve:
Automatiza tareas como compilar el proyecto, validar dependencias y
construir imágenes Docker cada vez que se sube código nuevo al
repositorio, mostrando el resultado directamente en cada Pull Request.

Por qué fue necesaria en este backend:
El equipo trabaja con más de una persona usando Pull Requests antes de
integrar cambios a la rama develop. Sin un pipeline de CI, un cambio que
rompiera la compilación de un microservicio podía pasar desapercibido en
la revisión visual del código y mezclarse de todos modos, afectando a
los demás integrantes del equipo recién cuando ellos actualizaran su
copia local. Con GitHub Actions, cada Pull Request muestra automáticamente
si el proyecto compila correctamente antes de aprobar el merge.

----------------------------------------------------
10. ACTUALIZACIÓN DE PAQUETES JWT (CVE-2024-21319)
----------------------------------------------------
Para qué sirve:
Mantener actualizadas las librerías que manejan la generación y
validación de tokens de seguridad, ya que vulnerabilidades conocidas en
estas librerías pueden ser explotadas si no se corrigen.

Por qué fue necesaria en este backend:
Los paquetes Microsoft.IdentityModel.JsonWebTokens y
System.IdentityModel.Tokens.Jwt estaban en una versión (6.30.0) con una
vulnerabilidad de denegación de servicio conocida y documentada
(CVE-2024-21319), señalada automáticamente por NuGet al compilar el
proyecto (warning NU1902). Se actualizaron a una versión que corrige
esta vulnerabilidad.

----------------------------------------------------
11. RABBITMQ + MASSTRANSIT
----------------------------------------------------
Para qué sirve:
RabbitMQ es un sistema de mensajería que permite que un microservicio
publique un evento (un mensaje) sin necesidad de saber quién lo va a
recibir ni esperar una respuesta inmediata. MassTransit es una capa que
simplifica el trabajo de publicar y consumir esos mensajes en .NET, sin
tener que programar a mano la conexión con RabbitMQ.

Por qué fue necesaria en este backend:
La comunicación entre PurchaseService e InventoryService era HTTP
síncrono: al crear una orden de compra, PurchaseService llamaba
directamente a InventoryService y esperaba su respuesta antes de
continuar. Esto acopla a ambos servicios en tiempo real — si Inventory
está lento o caído, Purchase se ve afectado de inmediato. Se reemplazó
la actualización de stock por un evento OrderCreated: PurchaseService
publica el evento y continúa sin esperar, e InventoryService lo recibe
y descuenta el stock cuando puede procesarlo. La lectura de datos (por
ejemplo, obtener el nombre de un insumo) se mantuvo como HTTP síncrono,
ya que ese caso sí necesita una respuesta inmediata.

----------------------------------------------------
12. REDIS
----------------------------------------------------
Para qué sirve:
Redis es una base de datos en memoria que se usa como caché: guarda
temporalmente el resultado de una consulta para no tener que repetirla
contra la base de datos principal cada vez que alguien la pide.

Por qué fue necesaria en este backend:
AnalyticsService genera reportes y KPIs (por ejemplo, productos más
vendidos) que no cambian de un segundo a otro, pero antes se recalculaban
contra MySQL en cada petición, generando carga innecesaria. Se agregó
Redis para cachear esos resultados por un tiempo determinado (5-10
minutos), invalidando el caché cuando ocurre un evento relevante (como
una nueva orden), reduciendo así la carga sobre la base de datos.

Nota: el sistema de roles (Proveedor/Administrador) que se mencionó en
una primera versión de esta lista fue descartado, ya que el segmento
objetivo de WineSoft son únicamente dueños de negocio. En su lugar se
implementó el aislamiento multi-tenant por OwnerId (punto 3), que es lo
que realmente correspondía a este modelo de negocio.
