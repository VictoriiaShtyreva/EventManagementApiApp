# Event Management System API

Link to AzureFunction Repo: https://github.com/VictoriiaShtyreva/EventManagementFunctionApp

![Build Status](https://img.shields.io/badge/build-passing-brightgreen)
![License](https://img.shields.io/badge/license-MIT-blue)
![Azure](https://img.shields.io/badge/Azure-Enabled-blue)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-blue)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Enabled-blue)
![Nuget](https://img.shields.io/badge/NuGet-004880?style=for-the-badge&logo=nuget&logoColor=white)
![Swagger](https://img.shields.io/badge/Swagger-85EA2D?style=for-the-badge&logo=Swagger&logoColor=white)
![Postman](https://img.shields.io/badge/Postman-FF6C37?style=for-the-badge&logo=Postman&logoColor=white)

## Project Description

The Event Management System is a web application built with ASP.NET Core, Entity Framework Core, and PostgreSQL. It uses ASP.NET Core Identity, EntraID, and Microsoft Graph for user authentication and authorization, and integrates with Azure services for storage and monitoring.

## Table of Contents

- [Features](#features)
- [Getting Started](#getting-started)
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

## Getting Started

This guide will help you set up the Event Management API project on your local machine for development and testing purposes.

### Prerequisites

Before you begin, ensure you have the following installed on your system:

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/download/)
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

   Copy `appsettings.json` to `appsettings.Development.json` and update the necessary fields:

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

6. **Azure Functions**

This project also utilizes Azure Functions for certain tasks. You can find the related repository [here](https://github.com/VictoriiaShtyreva/EventManagementFunctionApp).

### Running the Application

To run the application, use the following command:

```sh
dotnet run
```

This will start the application and you can access the API at `https://localhost:5001`.

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

## Screenhots

## Video Demo

## Contact Information
