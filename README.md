<div align="center">
  <a href="./README.esp.md">
    <img src="https://img.shields.io/badge/Leer_en_Espa%C3%B1ol-ES-red?style=for-the-badge" alt="Leer en Español">
  </a>
</div>

# [BookMe] - Social Network
![BookMeGif](bookme.gif)

![Angular](https://img.shields.io/badge/Angular-DD0031?style=for-the-badge&logo=angular&logoColor=white)
![.NET Core](https://img.shields.io/badge/.NET_Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)
![Dapper](https://img.shields.io/badge/Dapper-Raw_SQL-lightgrey?style=for-the-badge)
![SignalR](https://img.shields.io/badge/SignalR-Real_Time-blue?style=for-the-badge&logo=signalr&logoColor=white)
![Azure](https://img.shields.io/badge/Azure_AI-0078D4?style=for-the-badge&logo=microsoftazure&logoColor=white)

A social networking platform developed to master high-performance raw SQL query implementation and bidirectional real-time communication using WebSockets.

## Data arquitecture
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

## Overview

This project was built with the technical goal of deepening expertise in advanced database management and real-time systems within the .NET ecosystem. Unlike traditional implementations using full ORMs like EF Core, this system leverages Dapper to execute raw SQL queries, providing granular control over query optimization and performance.

Additionally, it integrates SignalR to handle real-time events, enabling features such as instant chat and live notifications, replicating the fluid user experience of modern social networks.

### Key technical aspects
- **Query Optimization (Raw SQL):** Extensive use of Dapper for object mapping and complex query execution, prioritizing performance.
- **Real-Time Communication:** Implementation of WebSockets via SignalR for instant messaging and status updates without page reloads.
- **AI Content Safety:** Integration with Azure AI Safety Content to automatically moderate post content, ensuring a safe environment.
- **Efficient Pagination:** Lazy loading and pagination strategies applied to comments, replies, and chat history to optimize bandwidth and load speeds.

## Main features

### Content management (Feed)
The core of social interaction allows users to share and consume content dynamically.
- **Smart Posts:** Creation of posts validated in real-time by Azure AI to filter inappropriate content.
- **Nested Interactions:** Robust system for comments and replies. Both sections feature independent pagination to avoid data overload in threads with extensive activity.

### Profile
Complete tools for managing digital identity and connections.
- **Profile Customization:** Ability to change profile pictures.
- **Follower Management:** Sending and accepting follow requests.
- **Privacy & Blocking:** Functionality to block unwanted users, preventing future interactions.

### Communication & notifications
- **Real-Time Chat:** Private messaging system with followers. Chat history is paginated to ensure fast loading even in long conversations.
- **Notification Center:** Instant alerts for interactions (likes, comments, requests) received via SignalR.

## Deployment (Azure)

Both the frontend and backend applications have been deployed using Microsoft Azure App Services.
Regarding the database, since Azure tends to "sleep" instances during inactivity, I opted to use a MonsterASP.NET instance for better persistence availability.

<a href="https://book-me-client-d4btcufdayh5gadf.canadacentral-01.azurewebsites.net/bookmecontent/main-content/homepage" target="_blank">
  <img src="https://img.shields.io/badge/View_Live_Demo-Visit_App-0078D4?style=for-the-badge" alt="View Live Demo">
</a>

<br/>
<br/>

Made by [Alejandro.NET](https://alejandropg845.github.io/resume)
