<div align="center">
  <a href="./README.md">
    <img src="https://img.shields.io/badge/Read_in_English-EN-blue?style=for-the-badge" alt="Read in English">
  </a>
</div>

# [BookMe] - Red Social
![BookMeGif](bookme.gif)

![Angular](https://img.shields.io/badge/Angular-DD0031?style=for-the-badge&logo=angular&logoColor=white)
![.NET Core](https://img.shields.io/badge/.NET_Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)
![Dapper](https://img.shields.io/badge/Dapper-Raw_SQL-lightgrey?style=for-the-badge)
![SignalR](https://img.shields.io/badge/SignalR-Real_Time-blue?style=for-the-badge&logo=signalr&logoColor=white)
![Azure](https://img.shields.io/badge/Azure_AI-0078D4?style=for-the-badge&logo=microsoftazure&logoColor=white)

Plataforma de red social desarrollada para explorar y dominar la implementación de consultas SQL crudas de alto rendimiento y la comunicación bidireccional en tiempo real mediante WebSockets.

## Arquitectura de Datos
```mermaid
erDiagram
Users ||--o{ Posts : "creates"
Users ||--o{ Comments : "writes"
Users ||--o{ CommentReplies : "replies"
Users ||--o{ Notifications : "receives"
Users ||--o{ ChatMessages : "sends"
Users ||--o{ Follows : "initiates"
Posts ||--o{ Comments : "has"
Comments ||--o{ CommentReplies : "has"
Chats ||--o{ ChatMessages : "contains"

Users {
    int Id PK
    string Username
    string Password
    string ImageUrl
    string Status
    string RoleName
}

Posts {
    int Id PK
    int UserId FK
    string Description
    string ImageUrl
    datetime PostedDate
    int LikesCount
}

Comments {
    int Id PK
    int PostId FK
    int UserId FK
    string Content
    datetime CommentDate
    int LikesCount
}

CommentReplies {
    int Id PK
    int CommentId FK
    int UserId FK
    string Content
    datetime RepliedAt
}

Chats {
    string Id PK
    int User1Id FK
    int User2Id FK
    string LastMessage
}

ChatMessages {
    int Id PK
    string ChatId FK
    int SenderId FK
    string Message
    datetime SentAt
    bool IsRead
}

Notifications {
    int Id PK
    int UserId FK
    int ActorId FK
    string Type
    bool IsRead
}

```

## Descripción general

Este proyecto nace con el objetivo técnico de profundizar en el manejo avanzado de bases de datos y sistemas en tiempo real dentro del ecosistema .NET. A diferencia de implementaciones tradicionales con ORMs como EF Core, este sistema utiliza Dapper para ejecutar consultas SQL crudas (Raw SQL), permitiendo un control total sobre la optimización y el rendimiento de las queries.

Adicionalmente, integra SignalR para gestionar eventos en tiempo real, permitiendo características como chat instantáneo y notificaciones en vivo, replicando la experiencia de usuario fluida de las redes sociales modernas.

### Aspectos técnicos destacados
- **Optimización de Consultas (Raw SQL):** Uso extensivo de Dapper para mapeo de objetos y ejecución de queries complejas, priorizando el rendimiento.
- **Comunicación en Tiempo Real:** Implementación de WebSockets vía SignalR para mensajería instantánea y actualizaciones de estado sin recargar la página.
- **Seguridad de Contenido con IA:** Integración con Azure AI Safety Content para moderar automáticamente el contenido de los posts, asegurando un entorno seguro.
- **Paginación Eficiente:** Estrategias de carga diferida y paginación en comentarios, respuestas y chats para optimizar el ancho de banda y la velocidad de carga.

## Funcionalidades principales

### Gestión de contenido (Feed)
El núcleo de la interacción social permite a los usuarios compartir y consumir contenido de manera dinámica.
- **Publicaciones inteligentes:** Creación de posts validados en tiempo real por Azure AI para filtrar contenido inapropiado.
- **Interacciones anidadas:** Sistema robusto de comentarios y respuestas a comentarios. Ambas secciones cuentan con paginación independiente para evitar la sobrecarga de datos en posts que cuentan con demasiada información.

### Perfil
Herramientas completas para gestionar la identidad digital y las conexiones.
- **Personalización de perfil:** Cambiar la foto de perfil.
- **Gestión de seguidores:** Envío y aceptación de solicitudes de seguimiento.
- **Privacidad y bloqueo:** Funcionalidad para bloquear usuarios no deseados, impidiendo interacciones futuras.

### Comunicación y notificaciones
- **Chat en tiempo Real:** Sistema de mensajería privada con seguidores. El historial de chat está paginado para garantizar una carga rápida incluso en conversaciones largas.
- **Centro de notificaciones:** Alertas instantáneas sobre interacciones (likes, comentarios, solicitudes) recibidas a través de SignalR.

## Despliegue y Demo en Vivo

> 💡 Nota: Para la mejor experiencia, recomiendo usar la **Demo en Vivo** a continuación. La versión desplegada está precargada con usuarios y contenido, lo que permite visualizar las funcionalidades en tiempo real y el rendimiento de las consultas en un escenario realista. Si buscas una demostración de Docker/Microservicios, revisa mi repositorio contenerizado [Scrum Task Manager](https://github.com/alejandropg845/scrum-task-manager).

<div align="center">
  <br/>
  <a href="https://book-me-client-d4btcufdayh5gadf.canadacentral-01.azurewebsites.net/bookmecontent/main-content/homepage" target="_blank">
    <img src="https://img.shields.io/badge/Ver_Demo_en_Vivo-Visitar%20App-0078D4?style=for-the-badge&logo=microsoftazure&logoColor=white" alt="Ver Demo en Vivo">
  </a>
  <br/>
</div>

