# NGSDMS Project and Entity Generator

This tool automatically generates both complete .NET projects and entity-related code for NGSDMS API projects. It follows the same patterns and structure used in the existing codebase to ensure consistency.


## Features

- **Project Generation**: Creates a full .NET 8.0 solution with API, Domain, Application, and Infrastructure projects
- **Entity Generation**: Creates all entity-related files following a clean architecture pattern
- **Docker Support**: Includes Dockerfile and docker-compose.yml for containerized deployment
- **Authentication**: Includes Keycloak integration for authentication and authorization
- **Database Integration**: Configures Entity Framework Core with SQL Server
- **Dependency Management**: Automatically installs all required NuGet packages
