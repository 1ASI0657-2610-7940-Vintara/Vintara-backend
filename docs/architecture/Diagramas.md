# Diagramas de Arquitectura Vintara - C4, UML & Vistas

Este documento contiene los diagramas arquitectónicos del backend de **Vintara (WineSoft)** modelados en **PlantUML**. Puede visualizarlos usando la extensión de PlantUML en VS Code (presionando Alt+D en el bloque de código) o copiando el código en el servidor oficial de PlantUML.

---

## 1. C4 Context Diagram

```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Context.puml

LAYOUT_WITH_LEGEND()

title C4 Context Diagram - Plataforma WineSoft

Person(owner, "Dueño de Negocio (Owner)", "Administra la bodega, inventario y visualiza reportes analíticos.")
Person(operator, "Operador (Operator)", "Registra movimientos de stock e interactúa con alertas en tiempo real.")

System(winesoft, "Plataforma WineSoft", "Sistema centralizado para gestión de inventarios, alertas IoT, analítica de datos y perfiles.")
System(iot_simulator, "Servidor de Simulación IoT", "Genera lecturas simuladas de telemetría (temperatura, humedad, presión, nivel) y las envía al sistema.")

Rel(owner, winesoft, "Visualiza reportes, gestiona inventario y perfiles", "HTTPS")
Rel(operator, winesoft, "Registra stock, gestiona alertas", "HTTPS")
Rel(iot_simulator, winesoft, "Envía telemetría de sensores en tiempo real", "HTTP / REST")

@enduml
```

![UML Diagram](http://www.plantuml.com/plantuml/png/~1RPAnRjj038PtFGNhoGOKQydKAO5Z907In8YK5fqCHg8TedT7WKSvTTyfGz6jgr-iECbntBJRuUxu__m_yYmQgdNUPHyurAvh29xCslYnA1IVfsksf-wnYwIr1ADWqrfyqJeCgUOaMkFuiPYTdYpIrSEdcy9ZDD9YThgSfOhlDcsJUdPp_lNkeLf-kQwkbZVpo_djnNYIPSRc26QdiFyC5unhHGydi71ek1Br25yuK2ahox85QPGmbkT0ciFeegFTJu66u9RMKhF0-2uzJKOvZCuRpu6ZAO93U9I6rfW3Xmq5GsM1BMmuTkZu1u9IAseK0GEwtOjnBN4wcXmCMr8qIPvtwTY8mhW_cGnkzxGUlBnis3C5avWQYoRrDo3WOAHOs-utGYq1q94QHg00nkHR0ILqoJ8hjz78ZvyvK9ILvJ3w3ulaMNBwY51JCErpD08jAiAQel7kLqWTl2ME-S7sMghyBIkcVmscbhVGagxOKHz_tmgBBIFxpkqPbAGRJWmQWhA_htktQwbINvSKA56dsZh524DfWpqG8qUUJ7Sl26CZtqFi57DuwZmrsEJGAiMabqFW3Ra9RC7rg3Qf21t48VWqqRedzxeFHyG-_pFPV80Y0V_UWTUugV6hgbgKeycWUHZvaUnXolrOZoJtND-Bl0Dtf3GVmXpJIDj28OhI-xLubOK2xkTbbMAVKMWwx_u0)


---

## 2. C4 Container Diagram

```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Container.puml

LAYOUT_WITH_LEGEND()

title C4 Container Diagram - Plataforma WineSoft

Person(owner, "Dueño de Negocio (Owner)")
Person(operator, "Operador (Operator)")

System_Boundary(winesoft_system, "Límites del Sistema WineSoft") {
    Container(web_app, "Aplicación Web (SPA)", "Vue.js 3, Vite, Pinia", "Interfaz de usuario para interactuar con todas las funcionalidades del sistema.")
    Container(gateway, "API Gateway", "ASP.NET Core, YARP", "Punto de entrada único. Enruta peticiones HTTP hacia los microservicios correspondientes.")
    
    Container(auth_service, "AuthService", "ASP.NET Core, C#", "Maneja autenticación, registro de usuarios, contraseñas y generación de JWT.")
    Container(inventory_service, "InventoryService", "ASP.NET Core, C#", "Gestiona suministros, transacciones de stock, alertas de sensores y lógica de AlertEngine.")
    Container(profiles_service, "ProfilesService", "ASP.NET Core, C#", "Gestiona los perfiles fiscales y empresariales vinculados a los usuarios.")
    Container(analytics_service, "AnalyticsService", "ASP.NET Core, C#", "Gestiona reportes analíticos de rotación, niveles de stock y alertas.")
    
    ContainerDb(mysql_db, "Base de Datos Relacional (MySQL)", "MySQL 8.0", "Almacena datos persistentes en esquemas separados para cada microservicio.")
    ContainerDb(redis_cache, "Caché Distribuido (Redis)", "Redis", "Almacena en caché consultas analíticas frecuentes de inventario.")
    Container(rabbitmq, "Message Broker (RabbitMQ)", "RabbitMQ", "Canal de mensajería asíncrona mediante eventos usando MassTransit.")
}

System(iot_simulator, "Servicio Simulador IoT", "ASP.NET Core Worker", "Genera simulaciones continuas y envía telemetría directamente a los servicios internos.")

Rel(owner, web_app, "Usa", "HTTPS")
Rel(operator, web_app, "Usa", "HTTPS")

Rel(web_app, gateway, "Peticiones de API", "HTTPS / JSON")

Rel(gateway, auth_service, "Enruta /api/auth", "HTTP / JSON")
Rel(gateway, inventory_service, "Enruta /api/inventory", "HTTP / JSON")
Rel(gateway, profiles_service, "Enruta /api/profiles", "HTTP / JSON")
Rel(gateway, analytics_service, "Enruta /api/analytics", "HTTP / JSON")

Rel(auth_service, mysql_db, "Lee/Escribe usuarios", "EF Core / TCP")
Rel(profiles_service, mysql_db, "Lee/Escribe perfiles", "EF Core / TCP")
Rel(inventory_service, mysql_db, "Lee/Escribe suministros y alertas", "EF Core / TCP")

Rel(iot_simulator, auth_service, "Solicita token de servicio", "HTTP / JSON")
Rel(iot_simulator, inventory_service, "Registra telemetría", "HTTP / JSON")

Rel(inventory_service, rabbitmq, "Publica eventos (SupplyStockChanged)", "AMQP / MassTransit")
Rel(rabbitmq, analytics_service, "Consume eventos (SupplyStockChanged)", "AMQP / MassTransit")

Rel(analytics_service, redis_cache, "Consulta/Guarda reportes", "StackExchange.Redis")
Rel(analytics_service, inventory_service, "Consulta datos históricos de inventario", "HTTP / JSON")

@enduml
```

![UML Diagram](http://www.plantuml.com/plantuml/png/~1bLPDRzj64BthLqpL2mKeKe7cKFGKML8T1_vWJ3b6Ji9eEP8sNkuo-s5NBVg3Sl0XoArN_R5Ybg8-QIIzBMTs7X_Vphpb6-VH-b2erazI2nLoWgNtfVkztxVur5j8lmopuCWAeprftnEcw9SADTUySZvNSjOVldwLSkZkwh9VeFDa-yFNqw7H7gKcsoiPltKv-7XpDvdUNqpUJY_7v-FhKT9fjRpqYc3u6hRROIHnOR60Lv0gz3Wtja2ubveoC_UjLahM6PsO9qss2-rHeFLN0pd1DIsCa0QI6qvrsftjrf8iUiExRtYP6mj9N7aJzxMobVDKJCzCq3dQLV8aDJapzrCN4rreNwwV2-d9GKuACidH7Qbs1_vk0S3k8v8dcasnBBlG7fHA2XHo_Kt3FSqWoT91fzs5zeT0lKyEVkt21-cf2wdK4ZbneJtPEVx57nLSG2iDb6WH92TG-80MXD7WJOuE53gO1osaqQXaZlc6fQjOzjgT8suBzFI4A-QMNi1vzSGl7cHfxteyWQ6nr8MFWzkKesdGFWfCsblC4TR_QYbC3yRQ1ezGafVySdBmTZ99OOb28YZZe9326aVsaVCEXB6MN6bqBabxSZMp8teO_79QLH6J2dwPLK-d78S_S-mADNr2mC0jMelT1KiBwRmrUpgwBWldBJfQVqK7ArYGPatZyUG4x-udfu99_KZQ6xlQetLHnxx3xPoSvvC15mgf8n_N1MzHEnGRsN82vuruw08giXwh26bdB357jVwsa08vEk0DOxsGcavvbjRCfIAtHpFTX7wK9HzRIJRMm5mwWRnO0HMb9OTMnkT7gKLGc1i7LKajxoadrAXMNefzKeCwzgEiB9N6ylGnslhPIs6YHjRu-h2rV2IrfoMiQZ6RssmqIugL-woc-QmBxJDqnAKZzCR1BIcinWcIgrNs_ZBEQrp1RxrV8brLe22DaCUAacmSDsvh80taFWSgq84ZdbmMAawmuFav68iJnKQpn58ktLIWMB9QGnJBzHSOSU_8MP2vWUIMTqHMSNN0Y3HmwVeBjxeBYljfgnjxXIKHAgOvGTNRV7YdPsTnDfE--CmVJyxXWk3CcWUoaDp6rDNxYiFcWTT3VXK35wGTVYAxVaP0jtxMml9H5fHBr9w0ulnmww3E3LoXSnEU2kcPo3-rAIVI-AcJHL0R2wzwHXh8Of3D_C9CZbi8xerz85jr4KyuL1YRYMCJa3f41o3zo1mzAIh8Hxwvj2Gy5gpIfiDtDXPTM5UzthebLLz5Eyk_Sz77sGspTgVQixs3NjmMzsspEvDET_xA1f1UR8kW3--oc-kwTbjovA0Rd-vZAVkSgkjtvGVLJMwt3x7DVmUdmOtsOUhqTr2Q_EFWW-hyAKu4EjHYR-GlYVfZ9woSxMu7nXZ_KJLG7oR3TCFcz5DU0AgjyoMW1crVGDgxBdPMrWHRuHuEoL43P4P98Js2DmyKhxcwcvl5Fq9hQeZRwcuz69iNJg2XVCzOqZ3Z_wEj7IHPA4krojZ3XqlK2ygZnmoktZFmdarioEwmcbfbo1POxDpcVy5N3NIAUkJF6vVjdmUq-UwoOjZCetWO_odYctgLLrUq6s2RTAx1D_VCKZg__cRh-szdtWtQlo6TXqBz1m==)


---

## 3. C4 Component Diagrams

### A. GatewayService
```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Component.puml

title C4 Component Diagram - GatewayService

Container(web_app, "Aplicación Web (SPA)", "Vue.js", "Envía solicitudes HTTPS")

Container_Boundary(gateway_boundary, "GatewayService") {
    Component(program, "Program.cs", "ASP.NET Core", "Punto de entrada, arranca el pipeline de ASP.NET y configura los servicios.")
    Component(yarp, "YARP Middleware", "Microsoft.AspNetCore.ReverseProxy", "Carga la configuración de rutas y enruta las peticiones de forma dinámica.")
    Component(cors_policy, "CORS Middleware", "ASP.NET Core CORS", "Valida los orígenes permitidos (localhost, Render, Vercel).")
    Component(appsettings, "appsettings.json", "JSON Config", "Contiene las definiciones de rutas, clústeres y destinos de YARP.")
}

Container(auth_service, "AuthService", "REST API")
Container(inventory_service, "InventoryService", "REST API")
Container(profiles_service, "ProfilesService", "REST API")
Container(analytics_service, "AnalyticsService", "REST API")

Rel(web_app, cors_policy, "Llamadas HTTP", "HTTPS")
Rel(cors_policy, yarp, "Petición autorizada")
Rel(program, yarp, "Configura enrutamiento")
Rel(program, appsettings, "Lee configuraciones")

Rel(yarp, auth_service, "Enruta /api/auth", "HTTP")
Rel(yarp, inventory_service, "Enruta /api/inventory", "HTTP")
Rel(yarp, profiles_service, "Enruta /api/profiles", "HTTP")
Rel(yarp, analytics_service, "Enruta /api/analytics", "HTTP")

@enduml
```

![UML Diagram](http://www.plantuml.com/plantuml/png/~1XLDDRnen4BtpAwPx0ccpN7BgANIBsbJvM46QgYSqs0DCvBKjsmkbLNzCVq1ElVNA7wkyovA58ELcpyppl9dtVEK3kb2LglUEjL2L95Y5OFtxuT3X9bjoM5NpofCJHWVI8HEc75g5Ec8kV92AvyFyygA8eQ-tDyCIVI0tp2zdkIcjqH5Zu_kzm44Hv9TmIC17ngN34YxW4mRQu7PARis2UhtSw82ioVKtD9-XjIaa8wjOeE3TNmtVQ0xzQJ4Q92aaZnLbJpwUndgzUqRmHh7WK4doyFdXePWcWywJim-cqXBTjhzikixc-q0AoJ6HP02_UW3mmhbldOcSKqYAvfI9klLeMcHtumV8ZQDuBoeT34W2qi6Xn1JGETG2WHHOjgHOKyotm2q8enUyh1o2CXvyJO6DpvB12OSjkhYGxwD90RSifQ8DDbrlMJZZpI9a8s_lA4GssOJMv3mLplpOng8StH91uKl3PgcIm5K1FMo1T3o1GW-MGgIXoSU2XN4bWcIz-rEomDVSX75-PgC0SPtv_MHwGh6xA8ZvMa9KB9kvZTizBocsi-HA3YoDXxuo0jNA-932XBGaby8ZEK5gy9e2Mkif1DPBdqBIkMLFtkZOxilq_WxoUlfw8KO79atrl98Mh3iJroj9GQZTlsXjYkkHv0DhK-UZ2f73xwvhiGghMQCWnPchi6ejbK8o6KyVO5HS9uCEXFMQT31ksy5Tjx6tmDQP1Ilo7Mon3xq5HOrg6rXqiQCsTXxScv1w-PZ7YjyeB55Yy_4Yhls04NDKkdTnKVih-W-hO1p_H8dxyiDVsvVcXm_I-BFak9tJucFvRuYwFZUQV3j1y-g9LEF6-KEqF8ofTeHzbmPpJgiky90_YpuZLXVSfizYpwbrHBdDTz2zAzAoAjL_)


### B. AuthService
```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Component.puml

title C4 Component Diagram - AuthService

Container(gateway, "API Gateway", "YARP")
ContainerDb(mysql_db, "Base de Datos", "MySQL")

Container_Boundary(auth_boundary, "AuthService") {
    Component(auth_controller, "AuthController", "REST Controller", "Expone endpoints para login, registro de usuarios, cambio de contraseña y generación de tokens de servicio.")
    Component(auth_query_svc, "AuthQueryService", "Application Query Service", "Procesa consultas de autenticación y encapsula la generación de tokens JWT.")
    Component(auth_cmd_svc, "AuthCommandService", "Application Command Service", "Procesa comandos de registro de usuarios y actualización de datos con encriptación BCrypt.")
    Component(user_repo, "UserRepository", "Infrastructure Repository", "Gestiona la persistencia de la entidad User.")
    Component(auth_db_context, "AuthDbContext", "EF Core DbContext", "Configuración de persistencia para las tablas de autenticación.")
    Component(user_entity, "User", "Domain Model", "Entidad de dominio que representa a los usuarios del sistema.")
}

Rel(gateway, auth_controller, "Solicitudes /api/auth", "HTTP")
Rel(auth_controller, auth_query_svc, "Llama")
Rel(auth_controller, auth_cmd_svc, "Llama")
Rel(auth_query_svc, user_repo, "Busca usuario")
Rel(auth_cmd_svc, user_repo, "Guarda usuario")
Rel(user_repo, auth_db_context, "Usa")
Rel(auth_db_context, user_entity, "Mapea")
Rel(auth_db_context, mysql_db, "Lee/Escribe en winesoft_auth", "TCP/IP")

@enduml
```

![UML Diagram](http://www.plantuml.com/plantuml/png/~1VPJDKXiv4CVlVehEdA3AC1TEUmgC2SiML3bWAhKdLu_K6DLg9A5k1MQtyb2fF08lbkh1nECrpcsc-w_kNtz87ramI-dyv8CBnXTBy22I-8-wplXql7BoKDh2b4qCGa6EJUpgv37ecICMwrrRDoT7SpNTNL_L7R9GhfkJPHEx58EUIHf_8auyGNC2lnmmSxZAsC4Hd1PvkANyrHcQJ9eO15sWVB12eIVifr2TpY_XulMlcaBrz-dDl3hSA6VjGTVper_QTWhL6JA19PYXH5RvTN_x-Qew78LUdiKIBER-08iyBDlrdsRQe5I7yDy403R8hsfjHexUKrxhcry6pNPpVhk0RTFvivu72ZP55uGXOKRmSUN253Aj74kEYbouO7QHfs2mQzrW6z8XqyjtX1vM52YZSIy_WZeb_aE1zOi7Q1UFgyFte1yBvNx9NysQ-RF-lnMgjfIyCoWk1XXyC7BESpJ4g2XSlE2G48kkX3Dhc1ue64nSF8B7FPn_VLdiuJET7T4riUims3ryQ-_xXEg90-1xZOKUq4X1x_xTm5bT52rEIyWkoTfrrkG-oIwmteXbfXIdKDqnvHjAaPt4F2pdPRZFo9ABaP89jfqNn5h2qA94cHqB1UDGCJo2DjIY1Oswfr6s7HQGdcNTgrchoqRFCcpQ9sXY9jWoDZ7SkrKPZMChzUio8eDWwz-RxPuEg4BqpYYjvfd53bs0wsZ93p3hQhJ5iNF1HNWiEfUKYIa8Wbu2tWp7aeU1gqFD-MqokI6_UGPsRzzjzCuuAPOOQaokLecc_dEnq1T2Z--SshaFLnux_ArwivqxsiTDeF5Ud1Ks-5RPLkotK6FrHS5i_oyV2NOdVyTR56FVzd2kCT5UwUZLl2Agpzbar-f31Kyk4CTxMRvrTD7CwqljwUGZ1Liw_nC=)


### C. InventoryService
```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Component.puml

title C4 Component Diagram - InventoryService

Container(gateway, "API Gateway", "YARP")
Container(rabbitmq, "Message Broker", "RabbitMQ")
ContainerDb(mysql_db, "Base de Datos", "MySQL")

Container_Boundary(inventory_boundary, "InventoryService") {
    Component(supplies_ctrl, "SuppliesController", "REST Controller", "Expone CRUD de suministros.")
    Component(movements_ctrl, "StockMovementsController", "REST Controller", "Expone registro e historial de movimientos de stock.")
    Component(alerts_ctrl, "SensorAlertsController", "REST Controller", "Recibe telemetría IoT y expone alertas.")
    
    Component(supply_cmd_svc, "SupplyCommandService", "Command Service", "Maneja creación, actualización y eliminación de suministros.")
    Component(movement_cmd_svc, "StockMovementCommandService", "Command Service", "Registra movimientos y actualiza el stock disponible.")
    Component(alert_cmd_svc, "SensorAlertCommandService", "Command Service", "Procesa la ingesta de telemetría.")
    
    Component(supply_query_svc, "SupplyQueryService", "Query Service", "Consultas de suministros validadas por OwnerId.")
    Component(movement_query_svc, "StockMovementQueryService", "Query Service", "Historial de transacciones de stock.")
    Component(alert_query_svc, "SensorAlertQueryService", "Query Service", "Consultas paginadas de alertas IoT.")
    
    Component(alert_engine, "AlertEngine", "Domain Service", "Clasifica lecturas de sensores (NORMAL, WARNING, CRITICAL) y detecta anomalías.")
    Component(inv_subject, "InventorySubject", "Domain Event Publisher", "Notifica a observadores locales y publica eventos a RabbitMQ.")
    
    Component(supply_repo, "SupplyRepository", "Infrastructure Repository", "Persistencia de Supply.")
    Component(movement_repo, "StockMovementRepository", "Infrastructure Repository", "Persistencia de StockMovement.")
    Component(alert_repo, "SensorAlertRepository", "Infrastructure Repository", "Persistencia de SensorAlert.")
    
    Component(inv_db_context, "InventoryDbContext", "EF Core DbContext", "Acceso a tablas de inventario en MySQL.")
}

Rel(gateway, supplies_ctrl, "HTTP /api/inventory/supplies")
Rel(gateway, movements_ctrl, "HTTP /api/inventory/stockmovements")
Rel(gateway, alerts_ctrl, "HTTP /api/inventory/sensoralerts")

Rel(supplies_ctrl, supply_cmd_svc, "Usa")
Rel(supplies_ctrl, supply_query_svc, "Usa")
Rel(movements_ctrl, movement_cmd_svc, "Usa")
Rel(movements_ctrl, movement_query_svc, "Usa")
Rel(alerts_ctrl, alert_cmd_svc, "Usa")
Rel(alerts_ctrl, alert_query_svc, "Usa")

Rel(supply_cmd_svc, supply_repo, "Usa")
Rel(supply_cmd_svc, inv_subject, "Notifica cambios de stock")
Rel(movement_cmd_svc, movement_repo, "Usa")
Rel(movement_cmd_svc, supply_repo, "Actualiza stock")
Rel(movement_cmd_svc, inv_subject, "Notifica cambios")
Rel(alert_cmd_svc, inv_subject, "Notifica lectura de sensor")

Rel(supply_query_svc, supply_repo, "Usa")
Rel(movement_query_svc, movement_repo, "Usa")
Rel(alert_query_svc, alert_repo, "Usa")

Rel(inv_subject, alert_engine, "Llama OnSensorReadingReceivedAsync / OnSupplyStockChangedAsync")
Rel(inv_subject, rabbitmq, "Publica SupplyStockChanged", "MassTransit")

Rel(alert_engine, alert_repo, "Guarda alerta generada")

Rel(supply_repo, inv_db_context, "Usa")
Rel(movement_repo, inv_db_context, "Usa")
Rel(alert_repo, inv_db_context, "Usa")

Rel(inv_db_context, mysql_db, "Persiste en winesoft_inventory", "TCP/IP")

@endif
```

![UML Diagram](http://www.plantuml.com/plantuml/png/~1fLVDSkCs3BxxAL2TuXadkkJKqpfoclLCd3YACvsUF125EDobI8MadAYTFjAUzX7oOXrAiaN9V-diaII8xyC72A2-68lQ5faO_COb4qLAy69jRduFGurl5ojkNugaCAIPafQalM0g2tE1qjqvDpOLF0cZo_EvstgQtOOP6aiwZ2wNaSfo9TsTtFaVM6u5GNG9smEOS5nfpE0Sfd9DqYfTFf9USqQ3GQIaHIv9dwtGqXkM8mZ6yodSrAjW1C5VutWU33rBZKd2RVOwWc16nk2Au4ghxwITTLmTpXxy6vFaB2lDgrYcoGY2ApG4AS44hJBkognyVBWDXXwPvPKgP8gwFECRnikasHf1q0yZ6C8_0m1eOpupHPuBJcR9h1OZ21wRjOFGIeY6xFNZ0hfRr-_E0qJnqyIHD4N69JTMAtCH37iWcLfJHjAsA5QnxxFDxcUnDAqg121uuSOgpL4ux4ojUSPTfAQYufplaa11scD0qYWzhlPEuiV4U49WIL16Ld_yG9Yg1PH0DR7ADMu3toTnkMHPkZHhjj6uZ5IMeKmtgHb1qEo0jpL3ITyGc2Pa_EEd704oMw3WVzThnq7mZCjc-TbC-6pyN7oAL5pd0Jl2boqp854d0L9kSYLv8kX0FdmURKO-nMAk5IE3812uN96nwABtSdGy7Qy5wRAJa0Utum5KQnynKj8KmgBfoGnh53p550taIiFzcoGzJO-8ty7svJz9uQjVzrQZDCWOLv9ELduNjDNwVqITuuhBAinqM_FkGpWWT0rBSiKbkMxfbjVLohcTg0ovx009DFoPCmH1p1QwqRcYIWRExkxZsVXs17-EuxlftSq8edYwc4RZso6Ka98bPX5GgWp5nu_sQsm9SRbUcYBvHinsccEzvP6wTYSmBnB1pKlT0kwKhRaXgCIGNcDQiHAAeI1N_hapPmXKkJM0i6dnnmjHKwwsDHXJhWnth1peL3vhD5ONJWw2xk6SjE76acIyglpQmP6gsm3v1VShUBwVGqMtmMphxLSGMoy797KPJfDbzJfuxoHvahYMJkzLcg__W4XfWixcc34o2X0i9gAklNgeekOAI48rVXtmluD1JA9z2FHdwDV5OWuXvZpSJkLmOnCCktTtHkFUoqxghMNVHNUmxRrV2LVRkVU3kzuZlJEadWmsGFij_NRIslR3sJDlFc6yttSdplxqE6gswwyDp8kv-q7s1F3ikdrasnaOPWdtdY2zADlx_IzoLv53dCRR4NiSuJX3NwYJTvfUtBRYddgUjeVqsvVNmnhifApJIRpqTGZtXisjm0pXNjOj8oPCkLp5n8YlAHsRKZ88tN75kEfaqGlALNFMCEa0U8_vQZ0mXDtRzPFDc8MRpDnkc6A7N2UUcm9rYiqqXHL9qfZsAxIstMbkUnG-QUc37p3RnkwVUVybcxRiUk8RbsJKirrkcusJO17Dmwdx5nfy8Pdov_y0)


### D. ProfilesService
```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Component.puml

title C4 Component Diagram - ProfilesService

Container(gateway, "API Gateway", "YARP")
ContainerDb(mysql_db, "Base de Datos", "MySQL")

Container_Boundary(profiles_boundary, "ProfilesService") {
    Component(profiles_ctrl, "ProfilesController", "REST Controller", "Endpoints para gestionar perfiles fiscales de negocio.")
    Component(profile_cmd_svc, "ProfileCommandService", "Command Service", "Procesa creación y modificación de perfiles de negocio.")
    Component(profile_query_svc, "ProfileQueryService", "Query Service", "Recupera datos de perfiles por ID u Owner.")
    Component(profile_facade, "ProfilesContextFacade", "Anti-Corruption Layer (ACL)", "Facade pública para exponer los perfiles a otros contextos si es requerido.")
    Component(profile_repo, "ProfileRepository", "Infrastructure Repository", "Persistencia de perfiles de negocio.")
    Component(profiles_db_context, "ProfilesDbContext", "EF Core DbContext", "Configuración de base de datos de perfiles.")
}

Rel(gateway, profiles_ctrl, "HTTP /api/profiles")
Rel(profiles_ctrl, profile_cmd_svc, "Usa")
Rel(profiles_ctrl, profile_query_svc, "Usa")
Rel(profile_facade, profile_query_svc, "Usa")
Rel(profile_cmd_svc, profile_repo, "Usa")
Rel(profile_query_svc, profile_repo, "Usa")
Rel(profile_repo, profiles_db_context, "Usa")
Rel(profiles_db_context, mysql_db, "Persiste en winesoft_profiles", "TCP/IP")

@enduml
```

![UML Diagram](http://www.plantuml.com/plantuml/png/~1bLGzRzim4DtrAwwQRC2EbaoTuiX9Qy115SSPEWadykGGa4ZcICKnYlwgRbtpnmhQSYJRQPfkuhjtV8_teGldaNrJLz4dfKNLI887xwtxd2ICwxELyWzDqJXYORGdxS-4gHDReGuvO-TbfOeaFHzd0Rg_cISrEa-SfETvQcfhTCYnuVx8AryHfEVm6e2fmXLZ3MF8s9Ig8dT7_AG4HL5gj4UbYGShzBJ6pGZYIJQ3BxjJF8Bu-sIHnSEEEIq6zSOzLhaiHX1VeYEG15FqnWNwpURkTXuFUrVdbwRH4dapiAryNhJ82E8ZI_4GVaG0qDdlieJdgfSH1DXK5N7GNLpTBU4GkjBI6gMz0ukCi2BdbT78O8ctDq2fdC3m8GaqhOnGvYmUlgsVYrhcxabq3b9JrwZbtla8uXQ17fIn4UGG11CAzV9BmmPg8rMfH7kMr1dwY8_7XdXpwEGsG3tHxRdlOa6YiSG8ClJfGD8QXjaK6lYsriH_borHeAIZ-jEplzxYGMIYlHgdXhcneT0mnmqn32RfV1Z2EoBObzz5fGJkcaBFGOQXCgwpX60y6mVRRNWEXfq2Si0KtgxaEyLXigRpk21hdFA6jvCyqoMZyzm8tp31OJ0ZTifvqaBX_xR4vRB8MxEz0asBjaJRORo6r3310PWQNQfLmzqa5Eq-dVGfYF-CeWLLtQeURyRNvJA31ArAzf5ukCqu8fvEzBt3zwdzeJiXv_lP-1ZxLVQeQQVCtZt_vEu2RtVaZUVruxt_sNuCW3IibIPdIf-_LdC4yJBDabdu8KONf6LJLty0)


### E. AnalyticsService
```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Component.puml

title C4 Component Diagram - AnalyticsService

Container(gateway, "API Gateway", "YARP")
Container(rabbitmq, "Message Broker", "RabbitMQ")
ContainerDb(redis_cache, "Caché", "Redis")
Container(inventory_service, "InventoryService", "REST API")

Container_Boundary(analytics_boundary, "AnalyticsService") {
    Component(analytics_ctrl, "AnalyticsController", "REST Controller", "Expone endpoints para reportes, alertas de bajo stock y rotación.")
    Component(analytics_cmd_svc, "AnalyticsCommandService", "Command Service", "Genera solicitudes de análisis.")
    Component(analytics_query_svc, "AnalyticsQueryService", "Query Service", "Calcula métricas analíticas e integra caché Redis.")
    Component(stock_consumer, "SupplyStockChangedConsumer", "MassTransit Consumer", "Escucha variaciones de stock desde RabbitMQ para invalidar reportes.")
    Component(pdf_builder, "QuestPdfAnalyticsReportBuilder", "Infrastructure Service", "Renderiza reportes en formato PDF.")
    Component(cache_svc, "AnalyticsCacheService", "Infrastructure Service", "Abstracción sobre IDistributedCache para interactuar con Redis.")
    Component(srv_clients, "ServiceClients", "HTTP Client Service", "Consulta datos directamente al microservicio de inventario.")
    Component(analytics_repo, "AnalyticsRepository", "Infrastructure Repository", "Maneja datos analíticos locales si existen.")
}

Rel(gateway, analytics_ctrl, "HTTP /api/analytics")
Rel(gateway, analytics_ctrl, "HTTP /api/analytics")
Rel(analytics_ctrl, analytics_query_svc, "Usa")
Rel(analytics_ctrl, analytics_cmd_svc, "Usa")

Rel(analytics_query_svc, cache_svc, "Verifica caché")
Rel(analytics_query_svc, srv_clients, "Consulta suministros e historial")
Rel(analytics_query_svc, pdf_builder, "Llama para generar PDF")

Rel(stock_consumer, cache_svc, "Invalida caché al ocurrir cambios de stock")
Rel(rabbitmq, stock_consumer, "Despacha evento", "AMQP")

Rel(srv_clients, inventory_service, "Llamadas HTTP internas", "HTTP / JSON")
Rel(cache_svc, redis_cache, "Almacena/Recupera JSON", "TCP")

@enduml
```

![UML Diagram](http://www.plantuml.com/plantuml/png/~1dLMzRYD73Exr5CnMDY1RZQjK9ylEHS4faMLVW5G2TuQMU3SxiyVXwau9yZ0fNQJA8-Z5WfdLpygo7I3TBeS_7ycFvBkeADfKhlSTU-CQIx1KhUFtWu7Wryi5wx8fcqXYWbVoUcb2DQWT-cHp4TKwBWUZgujf4dsSV1XK69LaCBgQZq9L1vziwkI_fwoEO7G5-mUuOLm8Ld01GuzkhMpYFScA3VLwe-0LsPESBL3fAwxxK0odOtZV_XLzA7uRpgR5UKTJi2nPgozzA2OK8ou8hYLy9adQi_mukUjQt9HdGfRZtA1PKX-A4PhbvYch9_cHT_OhyXfaFOyjo3uKuvriYpjRtju_m70wBiuxMSolG-CjoleCTwdEowqePVOi_-8S_kW1mA5O7JkZuhesAOW4vxPvfl37ejjloGUGjtLWhn5g50IXEeXIx0Cw4iK8bg34Jm6Y1lCPrY11qV3c7tzPdB-EfhBpk3B7WAeAlUtKP2k1ZkWzUHA461mRriPIZez-yvVZoF6ja5yQIbqu3dgNX1t_-RyRS8JED0wXsZofiC48oUFcRytV1EoL5eAGkB1vWao0KnIvDdCJV6mgaZuKzqrTk_Lz4e-Mw1TaHzl757I2CJu8-iWANVbjD8rP8gnG60q7twRVLjvIj0GxmhRTOhz2nnPbtxTJRBLzd9SDEvk1tJKKTMeVznMQPSFhLY616Fj7mQZI66s4khMQaRSa_Fk19K0U7eDKg06cDp-S1iyZTCA49EpuVJtYi8mgQ3BT88PI2COt75MuR9HiThIhX9AWqGO5JF2lDafMS-EOlCRKfJRIg1MaW3y-F4oX_JzYIMgIKmIB6Y9O5ZAA5NabG0SL6mdj_7D87Mkt0Wg7jnYRgjYjIsf5vBGuNgZAyUC4FNtQmTapDaHmmQ2Z2961ld5KocFwPwyt8tTOcYTh8-Szm9e7-wVY_7_RF5TzUKW_HlnF_SCUQRMVgNVSTPdsAma_ijbDxKcKZjan8VPzZat5FX4jf2MmvAX16DrRdexdx8F32bjgBl9EapGWkmoUxuik-F5sfdSh1nq4quYmWC6gvCI_xKxOmZbSk9CzT4EnnhHIA9-aF5EJktGWMnpTx5ywPJaDYn5oi_EGUJnCoW1-kl_bvosCJXB7ztFeAZJaSJ0ZqzHflsUhFXGFempb7NdRLEvV)


### F. IoTSimulatorService
```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Component.puml

title C4 Component Diagram - IoTSimulatorService

Container(auth_service, "AuthService", "REST API")
Container(inventory_service, "InventoryService", "REST API")

Container_Boundary(iot_boundary, "IoTSimulatorService") {
    Component(sim_engine, "SimulationEngine", "IHostedService", "Hilo de fondo que inicializa el bucle de simulación e interactúa con APIs externas.")
    Component(sim_factory, "DeviceSimulatorFactory", "Factory Pattern", "Crea instancias de simuladores según el tipo de lectura.")
    Component(base_sim, "BaseDeviceSimulator", "Base Class", "Implementa comportamiento común para las caminatas aleatorias.")
    Component(concrete_sims, "ConcreteSensorSimulators", "Simulators", "Implementa algoritmos específicos para temperatura, humedad, presión y nivel.")
}

Rel(sim_engine, auth_service, "Petición de token de servicio", "HTTP POST")
Rel(sim_engine, sim_factory, "Solicita simulador")
Rel(sim_factory, concrete_sims, "Instancia")
Rel(concrete_sims, base_sim, "Hereda de")
Rel(sim_engine, concrete_sims, "Ejecuta paso de simulación")
Rel(sim_engine, inventory_service, "Envía telemetría generada", "HTTP POST")

@enduml
```

![UML Diagram](http://www.plantuml.com/plantuml/png/~1RLDDRnf13Btlht3t52GIBZdrb8GG1QbLKQ1dP6OTS3lZsOwzj6dLdvHJRxdonwfP26m8j_77iv_zF5TgcAm9llZ0udnJ4QpCQlqu62JyUR5aMpMBHYcvA4PY5ow6GUrHCkPShVAy60mlpoVPzVNpfq50DKg3uULy64CT9MFgNBymDayml8Hz06uPbma3dCCupgOS6eyMqvJIcXqLnJ2A8GkbCsniDTUjlm_bTMEhNLRPX_9XD9t1zMHSzZeGbZM9nVJKmOrVVQV11_Jy9ZPIONewusZpnSx85TxpB7lmfm20mrndoc5EicJ9JNVv76NKkdBJyNrKewf3uvvzX8hWCKeLuKT3mCAEqVDl1FAmQ9oddA1jESURVm8voIYXiyqBWekI1r6WNqP9K2_AtWbYZ-Zo2lfGtbBklf_dRXl8R7PFcA3bKjarJ8J0eeRY6FL0fOg959IMcnV9J8thTX9Fpfg4xqaiK6ck7FfGtg3I4OjovuQXHzLsMQ7s54WiZnZgc0m3PnspcPlMc10yAZWCB6YeW9voCJwr11V59RAMW-R9TlQKH6FQ4sbxlxKwJD0lOs8BKO6q9hTvVcGNTSl5ADIKC8_VXrKJgCAg3tKYRMLx0k4r-SpiRr4ya7zpC4UtFY7ZhTeLWSNlr3wsSOxjwSnc4vXycSxAthjYRmMVHi-E3G_ATI3xjEFzZ5z5toKVnJjotbEY2g6Y4qoEguw-aMiCeKPjpwLproVGftxpIDQRvxpih8cb_5wIKC8AZzTIN95KJV3_0G==)


---

## 4. C4 Code Diagrams (Diagramas de Clase UML)

### A. AuthService
```plantuml
@startuml
title Class Diagram - AuthService

package WinesoftPlatform.API.Shared.Domain.Model {
    abstract class BaseEntity {
        + Id : int
        + CreatedAt : DateTime
        + UpdatedAt : DateTime
    }

    class User {
        + Username : string
        + Email : string
        + PasswordHash : string
        + FullName : string
        + Phone : string
    }
    
    BaseEntity <|-- User
}

package WinesoftPlatform.AuthService.Domain.Repositories {
    interface IUserRepository {
        + FindByUsernameOrEmailAsync(usernameOrEmail: string) : Task<User>
        + FindByIdAsync(id: int) : Task<User>
        + AddAsync(user: User) : Task
    }
}

package WinesoftPlatform.AuthService.Infrastructure.Persistence.EFC.Configuration {
    class AuthDbContext {
        + Users : DbSet<User>
    }
}

package WinesoftPlatform.AuthService.Infrastructure.Persistence.EFC.Repositories {
    class UserRepository {
        - _context : AuthDbContext
    }
    UserRepository ..|> IUserRepository
    UserRepository --> AuthDbContext
}

package WinesoftPlatform.AuthService.Application.Internal.QueryServices {
    interface IAuthQueryService {
        + LoginAsync(request: LoginRequestDto) : Task<(string, User)>
        + LoginServiceAsync(clientId: string, clientSecret: string) : Task<string>
        + GetUserByIdAsync(id: int) : Task<User>
    }
    
    class AuthQueryService {
        - _userRepository : IUserRepository
        - _configuration : IConfiguration
    }
    AuthQueryService ..|> IAuthQueryService
    AuthQueryService --> IUserRepository
}

package WinesoftPlatform.AuthService.Application.Internal.CommandServices {
    interface IAuthCommandService {
        + RegisterAsync(request: RegisterRequestDto) : Task<User>
    }
    
    class AuthCommandService {
        - _userRepository : IUserRepository
        - _unitOfWork : IUnitOfWork
    }
    AuthCommandService ..|> IAuthCommandService
    AuthCommandService --> IUserRepository
}

package WinesoftPlatform.AuthService.Interfaces.REST {
    class AuthController {
        - _authCommandService : IAuthCommandService
        - _authQueryService : IAuthQueryService
    }
    AuthController --> IAuthCommandService
    AuthController --> IAuthQueryService
}

@enduml
```

![UML Diagram](http://www.plantuml.com/plantuml/png/~1hPLRRzem583VyolEOwSr-G5HLPK2tI9jAuDMVKGcFWIhZitiasrevRzFkO6T1DgfoqCYd_lbCznOOeQAN0OaI2AC9RCM9e9bXkKGmgYWpGBDJv5Y46nP-imoX2UXqEerpIIZjJPvD9ebqMB33F9eedCcLFHLSvJm9m00O2jBXgK4QHNybbcSAXAqQ_Jbyn4I3Z48HOveR90HyX5131D6-21oTDIFMpwitWVLfqxtQD5uYKg1OZb239QCK9cZc-PCo275Z5dxInl-cTdDaFwkaFBRYQ2pZLOTnRvwLozd75SlOLYL5-pF3VkuadRQSzngAqWRWRRfLIX2isOfGb96F5ZuGxyJYj_ksedScwh_aTsfzABmXMtv7o261sQVhqgdwrweXDVUWbVBF6azuloO9gwQRcsR0Rrr18bQ6sR952aL1gCP6YiieKencjwDex5MQv4LXf7Ggkcz1gCCCbcDjIByJJr2R0dLQe7abFxVYXhOrn7MmLM5i4oRIcE_SeUdZdSKlLntjpza5uRNdP1lRNEqtKgHLgEDaX8unMJqlK2pQom6Q2pzNHDlybzq9bGDXi4V1LgAQzcyFar87v2wg86yhE5nuQeycl1ri5GAL9Jmbk9Bg2KBJ0rIZ-tww8RyX5IcUGlWpjq-WdQYun2MXR-CU71ZHmGSc6D8FBgTxBsKDGjTyR1joKEtY7SHCTPvpXG_puHlv54nnwoyFQO3HYiUOEELRPpCzO_xA9IW-_MJDi-LtU7KsKGdtN4NlkAK_NlsaRH3jj5ykdZe_G2MDzveARt_o12Mh5z6VB9cnyS3gV7e4UTEvf2-wlBiKFgcNkHz4DoWuaKk_m8=)


### B. InventoryService
```plantuml
@startuml
title Class Diagram - InventoryService

package WinesoftPlatform.API.Inventory.Domain.Model.Aggregates {
    class Supply {
        + Id : int
        + SupplyName : string
        + Quantity : int
        + Unit : string
        + Supplier : string
        + Price : decimal
        + Date : DateTime
        + OwnerId : int
        + UpdateDetails(...)
        + DeductStock(qty: int)
        + AddStock(qty: int)
    }

    class StockMovement {
        + Id : int
        + SupplyId : int
        + Quantity : int
        + Type : string
        + Reason : string
        + Date : DateTime
        + OwnerId : int
        + Supply : Supply
    }

    class SensorAlert {
        + Id : int
        + DeviceId : string
        + SensorType : string
        + Value : double
        + Unit : string
        + Timestamp : DateTime
        + Status : string
        + IsAnomaly : bool
        + Acknowledged : bool
        + AcknowledgedAt : DateTime?
        + Acknowledge()
    }
    
    StockMovement --> Supply
}

package WinesoftPlatform.API.Inventory.Domain.Repositories {
    interface ISupplyRepository {
        + FindByNameAndSupplierAndOwnerIdAsync(name: string, supplier: string, ownerId: int) : Task<Supply>
        + ListByOwnerIdAsync(ownerId: int) : Task<IEnumerable<Supply>>
    }
    interface IStockMovementRepository {
        + ListByOwnerIdAsync(ownerId: int) : Task<IEnumerable<StockMovement>>
    }
    interface ISensorAlertRepository {
        + AddAsync(alert: SensorAlert) : Task
        + FindByIdAsync(id: int) : Task<SensorAlert>
        + FindAllAsync(ownerId: int, status: string, sensorType: string, page: int, size: int) : Task<IEnumerable<SensorAlert>>
    }
}

package WinesoftPlatform.API.Inventory.Domain.Services {
    interface IInventoryObserver {
        + OnSensorReadingReceivedAsync(...) : Task<SensorAlert>
        + OnSupplyStockChangedAsync(...) : Task<SensorAlert>
    }
    
    class AlertEngine {
        - _sensorAlertRepository : ISensorAlertRepository
    }
    AlertEngine ..|> IInventoryObserver
}

package WinesoftPlatform.API.Inventory.Application.Internal.CommandServices {
    interface IInventorySubject {
        + RegisterObserver(observer: IInventoryObserver)
        + NotifySensorReadingAsync(...) : Task<IEnumerable<SensorAlert>>
        + NotifySupplyStockChangedAsync(...) : Task<IEnumerable<SensorAlert>>
    }
    
    class InventorySubject {
        - _observers : IEnumerable<IInventoryObserver>
    }
    InventorySubject ..|> IInventorySubject
    
    class SupplyCommandService {
        - _supplyRepository : ISupplyRepository
        - _inventorySubject : IInventorySubject
    }
}

@enduml
```

![UML Diagram](http://www.plantuml.com/plantuml/png/~1dLNRSjem47tdA_H8fy4Vm7Het93ECDE459BsCRDOYwiYhLn99kEs-VUERo0RGQ0y68zsz-pksRFojNLWNAxam0adaTr8i9PD1QG651kn6MsHd3R54irM93WOP91i84NsNH1QlNOF4jnQ6nN53xDevnvDjG91qPtcAACuJGscuD2oFmF664kgDCiyosJH79M_zsp6sPW9SjvHxNKF2jcOMMS4fPxrQmxaX2iEmfv8k5101IVGX6mFHYHb5ew9K20zonHSQIZ_7eL2pp9_8JI1ifyo3Wwdw418EuoYw9qFXpnFtDBfP3FyvOegrhV7d8UChmEVlTBXJczH8RdpI0mOZlBtM6H1mXS8LbF8SZb7ZG36pKkWHoIhJIpHlDdX54jzLeR3cLSmnphw1ZAlfgxpbSGp95HsPnseBDpjqe7BRIXmPcFI2ggMLrhx0ekJ3UaNYJn5VjeQEo_jnx3JiDLB-QmUNRMCHfEMyzTBLtg1cRR2QIDswop8eLb3WcnMe-vykjlzMH3_LAroJBnTn9Xueu_O5fGC2HIst5qnstZjJtJjNAy56xD7i9iFTTg9b-kBiEvJqK4EHivkALTeO2MnHPbuvFcj-HGUwV2_ilguHvFlD-58wfWt2Q5q6lkxqwOz64LRe-ZJkO-Tz89YAG_xkcAs4hqtjztAxSyoIB5r5x_n12DU-fQFYtNQVB426jsvpbSMpHPDXySvrUaN25nGki04nHOReifx_0sIvbJBg1hhpG-WzAnWRrVhkw-otL8g2BtwHkpP1gKmFg8H3zn7ZAA_am0J5z0SbukPW1EQebb9Be6CRhHIGFmCxfVvwYSctMjzWQcm3arRpL0tB-D0gVxtybuxiIuwSpka_BJ2Ea1d3F0jlVQcUQBl4Njk-oo_6ZxmOTS-_05cRwZDSR-Kkhlkf7eAwr_YfRXwPxw_w1VYpyklebhZQoIUA_aF)


### C. ProfilesService
```plantuml
@startuml
title Class Diagram - ProfilesService

package WinesoftPlatform.API.Profiles.Domain.Model.ValueObjects {
    class CompanyName {
        + BusinessName : string
        + Branch : string
        + FullName : string
    }
    class FiscalAddress {
        + Street : string
        + Number : string
        + City : string
        + PostalCode : string
        + Country : string
        + FullAddress : string
    }
    class ContactPhone {
        + Number : string
    }
    class TaxIdentity {
        + Number : string
    }
}

package WinesoftPlatform.API.Profiles.Domain.Model.Aggregates {
    class Profile {
        + Id : int
        + Name : CompanyName
        + Address : FiscalAddress
        + Phone : ContactPhone
        + LegalId : TaxIdentity
    }
    Profile --> CompanyName
    Profile --> FiscalAddress
    Profile --> ContactPhone
    Profile --> TaxIdentity
}

package WinesoftPlatform.API.Profiles.Domain.Repositories {
    interface IProfileRepository {
        + AddAsync(profile: Profile) : Task
        + FindByIdAsync(id: int) : Task<Profile>
    }
}

@enduml
```

![UML Diagram](http://www.plantuml.com/plantuml/png/~1dLBHIiCm57tlL-HHaVK3Yenr7OE2pk94d--IsoukJKfoAnRPlykwTajTXsWUSyx9EJdtpXo1fRfK0KbIo185ph65XC92oIOiioQN2jqQxOVa60GLy1qKoDwaHcTooXHGRcmPnbaQzknmOKgGEdmq0bNu2gh6fyqxSdBiAs2CCTxQ9AQiG3ShAB6xFvmxDg_TuNNN0X5pPAKkVDo2vjinP5ahTQdQUvvBwJYeM0YBhazprAx98jBOgwkwtA0TGn99pTXzPXo1IemOpP-OMfDjhdsW3tVr3udH19oohT73uiQ2-i8N-4m5QZga_bst_zUquwAmM03XSDOTUM2R2XOngSbFSXoUjnaUU2vcC4M_zxQIQD2G1pzW0QfrzPhmMkf3JYRJYmW-TccV3PG_p7tGD_vhmSzO6IV9M7agLsf2cmD7bdRi4sauubY8s3MQtrH7MjIdkctRS3j_2QKMyoRj15AqG-fvzvrkUjwI6MfHb-eR)


### D. AnalyticsService
```plantuml
@startuml
title Class Diagram - AnalyticsService

package WinesoftPlatform.AnalyticsService.Domain.Model.Aggregates {
    class AnalyticsReportData {
        + Id : int
        + OwnerId : int
        + ReportType : string
        + GeneratedAt : DateTime
        + ContentJson : string
    }
}

package WinesoftPlatform.AnalyticsService.Infrastructure.Services {
    interface IAnalyticsCacheService {
        + GetCachedReportAsync(key: string) : Task<string>
        + SetCachedReportAsync(key: string, data: string) : Task
        + InvalidateCacheAsync(pattern: string) : Task
    }
    class AnalyticsCacheService {
        - _cache : IDistributedCache
    }
    AnalyticsCacheService ..|> IAnalyticsCacheService
}

@enduml
```

![UML Diagram](http://www.plantuml.com/plantuml/png/~1bL5VIyCm47_lfxX7HPSFK6Iih30gYE86FigPN6jOUYt9TL9qtrtQRgCR7MBUylkN_EuMGT1BKxf8h3Y2fSCG8BLOU2nX1WcZQyNgi2Q_ivgYg4QznOBWpJA5AfSNXv9NlbINKfLM9LfMJvKXfvAYy5IWK83l2010zo-TJAzKLrvI53pmtRc3p40CbcK4FNyn-GbyIDYqDK4CGRpbOiIkYCcZa4a4OaXHQ6DB6WcM5GknF8QAp_txQF-VrXdd7eFuHalZIHtWOslBGZv7JP2Td4lKdtJGdPLVaVIS6QebeMLziwNs-BrRY663OVimNESZw_eFwpqO5BmC6W-UT-YiGQ4-PmYeKOGyJzhsKrkzKcq6xxfZ88OijLtOHoDaUlKeR3f6gP_vbUbrcreGcwPqlm==)


### E. IoTSimulatorService
```plantuml
@startuml
title Class Diagram - IoTSimulatorService

package WinesoftPlatform.IoTSimulatorService.Application.Simulators {
    abstract class BaseDeviceSimulator {
        + DeviceId : string
        + SensorType : string
        + Unit : string
        + OwnerId : int
        # _lastValue : double
        + {abstract} NextValue() : double
    }

    class TemperatureSimulator {
        + NextValue() : double
    }
    class HumiditySimulator {
        + NextValue() : double
    }

    BaseDeviceSimulator <|-- TemperatureSimulator
    BaseDeviceSimulator <|-- HumiditySimulator
}

@enduml
```

![UML Diagram](http://www.plantuml.com/plantuml/png/~1bL3BIiHG3DtVhmYuKQJz074nwYoSZGgjkfHCcod1-oYvkUemzj-bhTOHwu3P178UESaYAeecPusoMe8hYp72ah4HT931AfG5ksHHWnGaBroHCIrMpzWGFBAd63PwPr4tGLm-myulsjPoXSh1vnCOOMS0070TLR1Ig8QzbnXfIRriOduI-pg54LlLS0PHXNspXnNaOv1osz8SUkzPv-QthvvaC6Ilq_m8dYn6VK2RUhSwfBMbFTNkAtS7D_GsyevFVZ8xCxJni99SIuAQv8_B3jXykrmdnpNhzjyMGvjxxlbxbis6Ewpu5SHqnYp8rydP3m==)


---

## 5. Vista Lógica

```plantuml
@startuml
title Vista Lógica - Capas y Flujo de Dependencias (Clean Architecture)

package "Presentation Layer" {
    [Controllers]
    [DTOs / Assemblers]
}

package "Application Layer" {
    [CommandServices]
    [QueryServices]
    [Message Consumers]
}

package "Domain Layer" {
    [Aggregates / Entities]
    [Value Objects]
    [Repository Interfaces]
}

package "Infrastructure Layer" {
    [Repositories Implementation]
    [DbContext (EF Core)]
    [Distributed Cache Services]
}

[Controllers] --> [CommandServices]
[Controllers] --> [QueryServices]
[CommandServices] --> [Repository Interfaces]
[QueryServices] --> [Repository Interfaces]
[Repositories Implementation] ..|> [Repository Interfaces]

@enduml
```

![UML Diagram](http://www.plantuml.com/plantuml/png/~1VP19Ri9044NtFaM95ImWDuYm34X8H6GIcoYBShimJNgmkggZM0cdoX5oiGYG2J73zlSVkjwG1OD4QnBHOWWMcWLXzlDTQeNGXmmhPAXXOkBQGq4meefSGKvfPEXcXj116jHA2oc9WNf9Kg5wmvAWSn-8oGcAzWvcM5Fem6S20F2IUIV16qE1N_VAw7dES0CfCzbyhs-EkjAgCbgThx8MNV54uLqhQkeU8eMwfTqHyxOiyuwZFPaOUOkwtPwMPQ0IXRQV6plHeWzz2pIHO9wlIKcZFLBbMOiFDKoTK5ZYRlzuP-gM0Lb2t05hxHtocXYcjZ9a6uGDg7mBZpu4kkC9P3vGhtdHB47dKQY03DMAu0_09ad-GOT-__OCkpEU5icJpDvruUvM-hhtskqm67nTZYP3SaMqvXS=)


---

## 6. Vista de Comportamiento (Diagramas de Secuencia)

### A. Inicio de Sesión
```plantuml
@startuml
title Secuencia: Inicio de Sesión de Usuario

actor Usuario
participant "Web App" as SPA
participant "API Gateway" as GW
participant "AuthController" as Ctrl
participant "AuthQueryService" as Svc
database "MySQL" as DB

Usuario -> SPA: Introduce Username/Password
SPA -> GW: POST /api/auth/login
GW -> Ctrl: POST /api/v1/auth/login
Ctrl -> Svc: LoginAsync(request)
Svc -> DB: Query User by Username
DB --> Svc: User instance
Svc -> Svc: Verify BCrypt password
Svc -> Svc: Generate JWT Token
Svc --> Ctrl: Token & User Info
Ctrl --> GW: 200 OK
GW --> SPA: 200 OK (JWT Bearer Token)
@enduml
```

![UML Diagram](http://www.plantuml.com/plantuml/png/~1PL3BJiCm4BpxArOz83YW0iSU44ahHUKXWbB8UUjimIArmteTbE_Y4_WnPAUj2fnipundP_R62xA4RQl4I4jGaGvajS4PBAtHna4JGM--lsoyFlk0R9nIgCNnuTaXYz6cGoimgMaDMTTD03rKPVQRpCeb52ZqYKCI5FKVFiZRt5bXrxR4IJ8NRl-BdWBnK17tHjCugjUgGS4rUeB9mr0ztITyaIkromddrp5Hx2Ricg1Z8MABMvgMwFsduqPLPHPrHJs3yh5QmHGxCyKWRzFMlHghYZhICTIneByyrio5spIirpEuZr3c1wjFcJu2UJbJLQyZlyXda8ga7B0UpvX7BN8utpiaqbWlQ3Nj_oRcXTXi1iZdF7G2tQ73aQGWIun2S5klOENUoOxqeKJ2u6ISihGRjuk_MyBLnGKytgNQ-_MD49n6nvoGYKUJCtL3jWdRzWS=)


### B. Registro de Movimiento de Stock
```plantuml
@startuml
title Secuencia: Registro de Movimiento de Stock

actor Operador
participant "Web App" as SPA
participant "API Gateway" as GW
participant "StockMovementsController" as Ctrl
participant "StockMovementCommandService" as Svc
participant "SupplyRepository" as SRepo
participant "InventorySubject" as Subj
participant "RabbitMQ" as Broker

Operador -> SPA: Realiza descuento de stock
SPA -> GW: POST /api/inventory/stockmovements
GW -> Ctrl: POST /api/v1/inventory/stockmovements
Ctrl -> Svc: Handle(CreateStockMovementCommand)
Svc -> SRepo: FindByIdAsync(supplyId)
SRepo --> Svc: Supply entity
Svc -> Svc: DeductStock(quantity)
Svc -> SRepo: Update(Supply)
Svc -> Subj: NotifySupplyStockChangedAsync()
Subj -> Broker: Publish Event (SupplyStockChanged)
Svc --> Ctrl: Return updated supply
Ctrl --> GW: 201 Created
GW --> SPA: 201 Created
@enduml
```

![UML Diagram](http://www.plantuml.com/plantuml/png/~1VP71SjCm48RlVegTdj939vIZ3qnJ0o67K1F3vBoMbbQfB0bfPSOyFIF9Baa7EDh_ftzNd-u2e-Suw8eLQuAEH2GZ5DPme4SLs5kG17ixga6HuVpLiHNFLOM2hOS7HnwbzPL3pqeeXuRXwaWzR9ox0WpGjPlBSDFkO8jCFt7AmFPucUV-lHrf8CEXiOQzrPfyXXls-ZzuOuS1ZUp8ZqfGcJ-ALmUYStewaBD1iVLbYIvzNt8xCwOh-wcB_Oa45oxsfqliW7slUFybn_VUFfElgaKBtBnB0f9ErEeNWgIG51UJ8Plisat2jiSQseVkAwpHgRLQXgypD2mwgkqnmKd3EJxU_lj4OlCUewZX4ngfQTLuGgQ_gRkkkb5aF1cfuQCoydxQoKsOZ5Y5x6wNg1J3pT9Rf08PLZozLAJaFSaeE8zQ_OYOWTT3lZc9JAjIyYUC_Qc6pvRLzwb4kQLvGlD8yqBNLQ8INTJNqCPUg_047v8Ec3lF3yxzBnOFnD4RY7a72UMAixNvNTw-kOMYJEO7M5xr_FyT6Ha7_Hi=)


---

## 7. Vista Física

```plantuml
@startuml
title Vista Física - Nodos e Infraestructura de Red

node "Cliente Web Browser" {
    component "Vue.js App" as Frontend
}

node "Producción / Servidor VPS" {
    node "Docker Engine Network" {
        component "gateway-service (YARP)" as GWContainer
        component "auth-service" as AuthContainer
        component "inventory-service" as InvContainer
        component "analytics-service" as AnalContainer
        component "profiles-service" as ProfContainer
        component "iot-simulator-service" as IotContainer
        database "MySQL Container" as MySQL
        database "Redis Container" as Redis
        queue "RabbitMQ Container" as RabbitMQ
    }
}

Frontend --> GWContainer: HTTPS / REST (Port 5000)
GWContainer --> AuthContainer: HTTP (Port 8080)
GWContainer --> InvContainer: HTTP (Port 8080)
GWContainer --> AnalContainer: HTTP (Port 8080)
GWContainer --> ProfContainer: HTTP (Port 8080)

InvContainer --> RabbitMQ: AMQP
AnalContainer --> RabbitMQ: AMQP
AnalContainer --> Redis: TCP (Port 6379)
AuthContainer --> MySQL: Port 3306
InvContainer --> MySQL: Port 3306

@enduml
```

![UML Diagram](http://www.plantuml.com/plantuml/png/~1ZPB1RXen48RlVehHdi9XcvLGqtGFLIXDsaWbMbX4rADP3ucRnKF7On2gyaWzzH7oOXLEDc81qLn7tpV_oByll6YMCA-LMAaH9jQBXgkdFzvM6bAu8KCU4AxTZ3LwuL19O0q6OOH6AKS68UdN5fqWtE8KFZEjF782lnK0G4Np1Jbq0iaauBkV7dgBHGBQmnMJ4tH6FJPh2YOJgie-_NLm2YNoqXfYc1Hbi-sP-qBL0p9SkZlh46vGLiGF3R8JUgS5LtgT-iss2k7aHszKTE81Ns_xv4HRXtp8r47k6otolI3tHmthbkY4UDtIhjtoU8xJzLfivTjXJjT7jGNJpDROjWgcsV4JIL9lvw7MGjm-asJVD5hqL7k4PB0kXz_XbOXAd1rWHsYits7Zx9Nz5J1iG3sTMXaCTzcNSSGVD_-Z-IkGff-sUylXstXSb70Aeyjo32S5iS3xBCiwQekALgkzP--5Fy_E3_3RjRq1R_ML_vzlDNM0LzlvqMXU9OVUO5YeLk0RWKq7EOpxJTHPzyF7ZcgzJ0HZiJb4fjlDplQFsIFK1JeJvlK_)


---

## 8. Vista Modular

```plantuml
@startuml
title Vista Modular - Acoplamiento y Dependencias

[WinesoftPlatform.GatewayService] --> [WinesoftPlatform.Shared]
[WinesoftPlatform.AuthService] --> [WinesoftPlatform.Shared]
[WinesoftPlatform.InventoryService] --> [WinesoftPlatform.Shared]
[WinesoftPlatform.ProfilesService] --> [WinesoftPlatform.Shared]
[WinesoftPlatform.AnalyticsService] --> [WinesoftPlatform.Shared]
[WinesoftPlatform.IoTSimulatorService] --> [WinesoftPlatform.Shared]

note bottom of [WinesoftPlatform.Shared]
  Biblioteca compartida (Clases Base, BaseEntity, IUnitOfWork).
end note

@enduml
```

![UML Diagram](http://www.plantuml.com/plantuml/png/~1dOx1IWD144JlynLzD93D9qWI5Tc39B1g3f93PwQNDEdfNcPw8_lt4g-Ao5xgLAzUhQjZyJ5BS7OXUEVg22-MHi423MoY3OAPITrWWaSQI1DfPAmXV1nOgLhlUq7lhUJLCpfzujHHkNAa8pJDFVniTMSib8w_y9lHpxFXLg-tcsM-Vb-iPw4w_x-YJCvn_a9hhntdKT2j_7Cag3d1oTmjW_L_D06sV18sfuWGBGzOd1F2tODWfGfRhBJypYTrzca9xPkox_g3bSjY5KWJt6GXh4dJcEKB)


---

## 9. Vista de Despliegue

```plantuml
@startuml
title Vista de Despliegue - Target Cloud (Render & Vercel)

cloud "Vercel CDN" {
    artifact "Frontend Estático" as Vercel
}

cloud "Render Container Services" {
    component "API Gateway (YARP)" as RenderGW
    component "Microservicios Backend" as RenderBackend
    database "MySQL Managed Database" as DB
    database "Redis Cache" as Cache
    queue "Managed RabbitMQ" as MQ
}

Vercel --> RenderGW: HTTPS REST
RenderGW --> RenderBackend: HTTP routing
RenderBackend --> DB: SQL Queries
RenderBackend --> Cache: Redis Client
RenderBackend --> MQ: MassTransit AMQP

@enduml
```

![UML Diagram](http://www.plantuml.com/plantuml/png/~1RP1DJWCn34RtSufFiq1bqGjqWThEb8B4eCwFG2pTn9I8QL8I3mWX3iDPk1Xgcg8AiKlivwVFdWOXByEspyH8pxWpGGYQKNBOzOOt0sECZlo61KNl1erHmrQpnndks2lkpxDCnKP--ACeRtDyP010NimZAK5-wPqLjXgB8DzVOfJBGI4fiizVHv8NpWePonuj-rUZE1oLocrtph8LvBFLDPOa_4Rl63tCcjLvT1uKo_k_U6MKTo7gZ0kOatfcgqz6KYNEQH9QKs3arNjRtw0YInlMA5CzZfNpFsp3sWGKf9uEG7n5vcNWOIzBceRMQoDL7QcgtXyWNMyyllZTO8AhhbkrQ1PjbnsB9qGAVC3WtI36RXAOUf4kvnFibwW7zeR3FqGCEa7AtnksyWzLrHDK54BdoGOZc5NrAikcRFMmxNy0)


---

## 10. Modelo de Datos (Diagrama ER)

```plantuml
@startuml
title Modelo de Datos - Entity Relationship Diagram

entity "users" as User {
    * id : INT <<PK>>
    --
    * username : VARCHAR(255)
    * email : VARCHAR(255)
    * password_hash : VARCHAR(255)
    full_name : VARCHAR(255)
    phone : VARCHAR(50)
}

entity "profiles" as Profile {
    * id : INT <<PK>>
    --
    * business_name : VARCHAR(255)
    * branch : VARCHAR(255)
    * street : VARCHAR(255)
    * number : VARCHAR(50)
    * city : VARCHAR(255)
    * postal_code : VARCHAR(50)
    * country : VARCHAR(255)
    * phone : VARCHAR(50)
    * legal_id : VARCHAR(50)
}

entity "supplies" as Supply {
    * id : INT <<PK>>
    --
    * supply_name : VARCHAR(255)
    * quantity : INT
    * unit : VARCHAR(50)
    * supplier : VARCHAR(255)
    * price : DECIMAL(10,2)
    * date : DATETIME
    * owner_id : INT
}

entity "stock_movements" as StockMovement {
    * id : INT <<PK>>
    --
    * supply_id : INT <<FK>>
    * quantity : INT
    * type : VARCHAR(50)
    * reason : VARCHAR(255)
    * date : DATETIME
    * owner_id : INT
}

entity "sensor_alerts" as SensorAlert {
    * id : INT <<PK>>
    --
    * device_id : VARCHAR(100)
    * sensor_type : VARCHAR(50)
    * value : DOUBLE
    * unit : VARCHAR(20)
    * timestamp : DATETIME
    * status : VARCHAR(20)
    * is_anomaly : TINYINT(1)
    * owner_id : INT
}

User ||--o| Profile : "Posee"
User ||--o{ Supply : "Es dueño de"
Supply ||--o{ StockMovement : "Registra historial"
User ||--o{ SensorAlert : "Recibe alertas"

@enduml
```

![UML Diagram](http://www.plantuml.com/plantuml/png/~1bPJFRjf04CRlVegHJmGL2P2ue2WA3QwA6YYYf59Frc1FODNrhhkpIuH27whFq1Uh_2yXoReYHyytipE_RzPxpHQDTPaChB2IOAvJaXfIWYbQpT23I5bX3x0YYLPenJkHmrJWrc0M15I9eMCo708ot34PU0m00BeWKXZ3RB66oylbbwkhCjhhrM9Hep0Z6CFtoUhcysJL6Ov65xLA6Ghfbt9aVj0cZNV8Er_AlPCoRZiwtsbrAepw5y7lPvBSw7iXgO9PLX_dyMmS2qNChPsxi36e4k_8NM1hYAnVKoxRa7arT2KbnTH-cpHRb76YK_9NQgUiQIlsk5GfahOeuzA95WlPvRaKjONVYe_3UGwMXOV_-FVBOTMZFAEvHqfOxwJr7AQ5q8YaQ3EDRcRpoMrdqFymRBGKRIbDrj5wDe_gg7vGP2hoelifiTN9ppZJUyf8sHgyYCthqBluJt8-DJajxFQG-xTa25ahF_cxwKYnDZ5ACWrR6PaKWVF8KjgBX5xUca7_ULDLXrQQFKfNZlprxkDjvD_xy2dRYepOOfQ_XMIBrh6tId2CIcSe2uVNiyMFsMBT6JJgMtVAX-vuxFNqyUcT64Eur4mKdgYFpIymXZ1YI1tz_LCyi65Gnvki5_Tb3E6AjeAjGTW9jje8bA-EFLb2cPw83K6v9UGm2Av9fIwJ_m0=)

