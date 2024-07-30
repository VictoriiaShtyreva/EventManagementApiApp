# Event Management System API

![Build Status](https://img.shields.io/badge/build-passing-brightgreen)
![License](https://img.shields.io/badge/license-MIT-blue)
![Azure](https://img.shields.io/badge/Azure-Enabled-blue)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-blue)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Enabled-blue)
![Nuget](https://img.shields.io/badge/NuGet-004880?style=for-the-badge&logo=nuget&logoColor=white)
![Swagger](https://img.shields.io/badge/Swagger-85EA2D?style=for-the-badge&logo=Swagger&logoColor=white)
![Postman](https://img.shields.io/badge/Postman-FF6C37?style=for-the-badge&logo=Postman&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2CA5E0?style=for-the-badge&logo=docker&logoColor=white")

## Project Description

The Event Management System is a web application built with ASP.NET Core, Entity Framework Core, and PostgreSQL. It uses ASP.NET Core Identity, EntraID, and Microsoft Graph for user authentication and authorization, and integrates with Azure services for storage and monitoring.

## Table of Contents

- [Features](#features)
- [Getting Started with Docker](#getting-started-with-docker)
- [Database Structure](#database-structure)
- [Workflow](#workflow)
- [API Endpoints](#api-endpoints)
- [Screenhots](#screenhots)
- [Video Demo](#video-demo)
- [Contact Information](#contact-information)

## Features

- User registration and authentication with ASP.NET Core Identity.
- Role-based authorization (Admin, EventProvider, User).
- EntraID & Microsoft Graph to manage all registration, login, and authentication.
  - UserId taken from EntraID.
- CRUD operations for events.
- Event registration with FIFO processing using Azure Service Bus and Azure Functions.
- Storage of event metadata and user interactions in Cosmos DB for NoSQL.
- Storage of images and documents in Azure Blob Storage.
- Monitoring and diagnostics with Azure Application Insights.

## Getting Started with Docker

This guide will help you set up the Event Management API project on your local machine for development and testing purposes.

### Prerequisites

Before you begin, ensure you have the following installed on your system:

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/download/)
- [Docker](https://www.docker.com/)
- [Azure](https://learn.microsoft.com/en-us/azure/?product=popular)

### Configuration

1. **Clone the repository**

   ```sh
   git clone https://github.com/your-username/event-management-api.git
   cd event-management-api
   ```

2. **Set up the database**

   Make sure PostgreSQL is running. Create a new database for the project. Update the connection string in `appsettings.json` with your database details.

3. **Azure Services Setup**

   - **Azure Blob Storage**: Create two containers: `eventimages` and `eventdocuments`.
   - **Azure Service Bus**: Create a queue named `appqueue`.
   - **Azure Cosmos DB**: Set up a Cosmos DB account with containers for `EventMetadata` and `UserInteractions`.

4. **Update Configuration Files**

   Create `appsettings.json` and copy to `appsettings.Development.json` and update the necessary fields:

   ```json
   {
     "EntraId": {
       "Instance": "https://login.microsoftonline.com/",
       "Domain": "your-domain.onmicrosoft.com",
       "TenantId": "your-tenant-id",
       "ClientId": "your-client-id",
       "ClientSecret": "your-client-secret",
       "ServicePrincipalId": "your-service-principal-id",
       "CallbackPath": "/signin-oidc",
       "Scopes": {
         "access_as_user": "api://your-client-id/access_as_user",
         "access_as_admin": "api://your-client-id/access_as_admin",
         "access_as_event_provider": "api://your-client-id/access_as_event_provider"
       },
       "AppRoles": {
         "User": "role-id-for-user",
         "Admin": "role-id-for-admin",
         "EventProvider": "role-id-for-event-provider"
       }
     },
     "ConnectionStrings": {
       "DefaultConnection": "Host=your-host;Database=your-database;Port=5432;User Id=your-username;Password=your-password;Ssl Mode=Require;"
     },
     "ApplicationInsights": {
       "ConnectionString": "your-application-insights-connection-string"
     },
     "ServiceBus": {
       "QueueName": "appqueue",
       "ConnectionString": "your-service-bus-connection-string"
     },
     "BlobStorage": {
       "ConnectionString": "your-blob-storage-connection-string",
       "EventImagesContainer": "eventimages",
       "EventDocumentsContainer": "eventdocuments"
     },
     "CosmosDb": {
       "Account": "your-cosmos-db-account",
       "Key": "your-cosmos-db-key",
       "EventMetadataContainer": "EventMetadata",
       "UserInteractionsContainer": "UserInteractions",
       "DatabaseName": "EventManagement"
     }
   }
   ```

5. **Run Database Migrations**

Ensure your database is set up correctly:

```sh
dotnet ef database update
```

6. **Build the Docker Image**

Open a terminal, navigate to the root directory of your project, and run the following command to build the Docker image:

```sh
docker build -t event-management-api .
```

7. **Run the Docker Container**

Run the following command to start the container:

```sh
docker run -d -p 8080:80 event-management-api
```

This will start the container and map port 8080 on your host to port 80 in the container.

8. **Access the Application**

Open your web browser and navigate to `http://localhost:8080` to see your application running.

## Database Structure

The following diagram illustrates the architecture of the Event Management API, highlighting the interaction between various components:
![Database Structure](/readme-doc/images/1.png)

### Flow of Data

**User Registration**: Users are registered via Azure Active Directory. Upon registration, roles are assigned to users based on their intended access level (User, Admin, EventProvider). By default all new users are assigned as `User`.

**Event Creation and Management**: Events are created and managed in the Azure Database for PostgreSQL Servers. Documents and images related to events are uploaded to Azure Blob Storage and linked to the respective events. The URLs of these documents and images are stored in the Azure Database for PostgreSQL Servers.

**User Interactions**: Users can register for events, which logs interactions in the UserInteraction entity stored in Azure Cosmos DB. Event metadata, such as type and category, is also stored in Azure Cosmos DB for efficient querying and management. Azure Service Bus is used for handling messaging and event-driven processes within the system.

**Integration with Azure Services**: The application leverages Azure Blob Storage for storing event-related media.

## Workflow

The following diagram illustrates the workflow of the Event Management API, showcasing the interaction between users, application services, and various Azure components:
![Workflow](/readme-doc/images/2.png)

## API Endpoints

All the endpoints of the API are documented and can be tested directly on the generated Swagger page. From there you can view each endpoint URL, their HTTP methods, request body structures and authorization requirements. Access the Swagger page from this [link](https://event-management-system-2024.azurewebsites.net/index.html).
![Swagger](/readme-doc/images/3.png)

### Important Note

Please note that the Azure Cosmos DB for PostgreSQL Cluster used in this project has been stopped, as this project is being developed for educational purposes. Due to this, it is currently impossible to perform CRUD operations with events. This limitation affects the following operations:

- Creating new events
- Reading event data
- Updating existing events
- Deleting events

When the database service is active, the API endpoints will fully support these operations, allowing for seamless interaction with the event data.

## Screenhots

### Azure Resources groups

The screenshot captures the essential Azure resources grouped into three main resource groups, each serving specific purposes:

- `appsvc_windows_centralus`: Hosts the main application services and provides monitoring and logging capabilities.
- `EventManagementAPI`: Contains the databases (both SQL and NoSQL) and the messaging service essential for the event management application.
- `eventregistrationfuncti2`: Manages the Azure Functions and related resources for handling event registrations, along with storage and monitoring services.

![Azure Resources groups](/readme-doc/screenshots/Azure%20Resources%20groups.png)

### Azure Cosmos DB account

These screenshots provide the structure and content of the EventMetadata and UserInteractions collections in Azure Cosmos DB.

- `EventMetadata`: Stores metadata information about events, including type, category, and related event IDs.
- `UserInteractions`: Records interactions of users with events, such as registration actions, with details on the user and the event involved.

![Azure Cosmos DB account.EventMetadata](/readme-doc/screenshots/Azure%20Cosmos%20DB%20account.EvenMetadata.png)
![Azure Cosmos DB account.UserInteractions](/readme-doc/screenshots/Azure%20Cosmos%20DB%20account.UserInteractions.png)

### Azure Cosmos DB for PostgreSQL Cluster

This screenshot captures the essential details and performance metrics of Azure Cosmos DB for PostgreSQL Cluster, which is a critical component of Event Management API project. The database is configured as a single node with no replicas or high availability, and it is hosted in the North Europe region within the EventManagementAPI resource group.

Key points:

- The database uses Citus 12.1 on PostgreSQL 16.
- Monitoring shows low CPU and storage utilization.
- Backup is configured as zone-redundant to ensure data durability.

![Azure Cosmos DB for PostgreSQL Cluster](/readme-doc/screenshots/Azure%20Cosmos%20DB%20for%20PostgreSQL%20Cluster.png)

### Application Insights

These screenshots provide a comprehensive view of how to monitor and analyze the performance, failures, and custom events in app using Application Insights:

- `Failures Overview`: Helps identify and diagnose issues causing failed requests.
- `Performance Overview`: Provides insights into the duration of operations and identifies potential performance bottlenecks.
- `Custom Metrics Query`: Shows how to query and analyze custom telemetry data such as `UserRegistered` and `UserEmailUpdated events`.

![Application Insights](/readme-doc/screenshots/Application%20Insights.png)
![Application Insights2](/readme-doc/screenshots/Application%20Insights2.png)
![Application Insights3](/readme-doc/screenshots/Application%20Insights3.png)
![Application Insights4](/readme-doc/screenshots/Application%20Insights4.png)

### Azure Service Bus

The screenshot provides an overview of the eventmanagement2024 Service Bus Namespace used in Event Management API project.

![Azure Service Bus](/readme-doc/screenshots/Azure%20Service%20Bus.png)

### Azure Storage account

The Azure Blob Storage containers play a crucial role in managing and storing media files (documents and images) related to events in the Event Management API project. Here is a summary of their functions:

- The `eventdocuments` and `eventimage`s containers are used to store files uploaded by users or event organizers.
- Each event has a unique folder identified by the `event ID`, ensuring all related files are organized and easily accessible.

![Storage account](/readme-doc/screenshots/Storage%20account.png)
![Storage account. EventDocuments](/readme-doc/screenshots/Storage%20account.EventDocuments.png)
![Storage account. EventImages](/readme-doc/screenshots/Storage%20account.EventImages.png)

### Azure Function App

Link to Azure Function Repo: https://github.com/VictoriiaShtyreva/EventManagementFunctionApp

These screenshots provide a detailed view of the `ServiceBusQueueTrigger` function on the Azure portal.

![Azure Function App](/readme-doc/screenshots/Azure%20Function%20App.png)
![Azure Function App2](/readme-doc/screenshots/Azure%20Function%20App2.png)

### Azure App Service

This Azure App Service configuration demonstrates the deployment environment for Event Management API.

![Azure App Service](/readme-doc/screenshots/Azure%20App%20Service.png)

### Azure Entra Id

These screenshots show the Event Management API registered as an enterprise application in Microsoft Entra ID.

![EntraId](/readme-doc/screenshots/Entra%20Id.png)
![EntraId2](/readme-doc/screenshots/Entra%20Id2.png)
![EntraId3](/readme-doc/screenshots/EntraId3.png)

## Video Demo

In this video demo, we will walk you through the crucial API endpoints of the Event Management System project, showcasing how they work and their interactions with various Azure services. Below are the key highlights that will be covered in the demo:

### User Registration Endpoint

Endpoint: `POST /api/v1/accounts/register`

[![Watch the video](https://img.youtube.com/vi/eE7LiRByAFw/0.jpg)](https://youtu.be/eE7LiRByAFw)

### Authentication and Authorization using Entra ID (Azure Active Directory) in Postman

Endpoint: `GET /api/v1/events/most-register`

[![Watch the video](https://img.youtube.com/vi/IUxLC9btDJ8/0.jpg)](https://youtu.be/IUxLC9btDJ8)

### Event Registration Endpoint (user not provide email for sending comfirmation email)

Endpoint: `POST /api/v1/events/{id}/register`

[![Watch the video](https://img.youtube.com/vi/cWye3_N2COY/0.jpg)](https://youtu.be/cWye3_N2COY)

### Event Unregistration Endpoint (user provide email for sending comfirmation email)

Endpoint: `DELETE /api/v1/events/{id}/unregister`

[![Watch the video](https://img.youtube.com/vi/HgSofRkij_4/0.jpg)](https://youtu.be/HgSofRkij_4)

### Event Create Endpoint using Swagger

Endpoint: `POST /api/v1/events`

[![Watch the video](https://img.youtube.com/vi/aGuZX61eRzY/0.jpg)](https://youtu.be/aGuZX61eRzY)

### Event Upload Images Endpoint using Swagger

Endpoint: `POST /api/v1/events/{id}/upload-images`

[![Watch the video](https://img.youtube.com/vi/M0-GkCaGxk0/0.jpg)](https://youtu.be/M0-GkCaGxk0)

### Event Upload Documents Endpoint using Swagger

Endpoint: `POST /api/v1/events/{id}/upload-documents`

[![Watch the video](https://img.youtube.com/vi/txIdEXDpgSE/0.jpg)](https://youtu.be/txIdEXDpgSE)

## Contact Information

For any questions, suggestions, or feedback, feel free to reach out:

- **Project Owner:** Viktoriia Shtyreva
- **Email:** [vikatori1409801@gmail.com](mailto:vikatori1409801@gmail.com)
- **LinkedIn:** [Viktoriia Shtyreva](https://www.linkedin.com/in/viktoriiashtyreva/)
- **GitHub:** [VictoriiaShtyreva](https://github.com/VictoriiaShtyreva)
