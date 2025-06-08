# TaskHive

TaskHive is a modern project management system built with .NET 8, following clean architecture principles. It provides a robust API for managing projects, tasks, and team collaboration.

## 🚀 Technologies

- **.NET 8** - Last LTS version of .NET framework
- **ASP.NET Core** - Web framework for building web APIs
- **PostgreSQL** - Relational database
- **Docker** - Containerization
- **JWT Authentication** - Secure API authentication
- **Swagger/OpenAPI** - API documentation
- **Clean Architecture** - Separation of concerns and maintainable codebase

## 🏗️ Architecture

The solution follows Clean Architecture principles with the following layers:

- **TaskHive.API** - Presentation layer (Controllers, Middleware)
- **TaskHive.Application** - Application layer (Use Cases, DTOs)
- **TaskHive.Domain** - Domain layer (Entities, Interfaces)
- **TaskHive.Infra** - Infrastructure layer (Repositories, External Services)

## 🛠️ Prerequisites

- .NET 8 SDK
- Docker and Docker Compose
- PostgreSQL (if running locally)
- Git

## 🚀 Getting Started

1. Clone the repository:
```bash
git clone https://github.com/yourusername/TaskHive.git
cd TaskHive
```

2. Run with Docker:
```bash
docker-compose up -d
```

3. Or run locally:
```bash
# Restore dependencies
dotnet restore

# Run the application
dotnet run --project src/TaskHive.API/TaskHive.API.csproj
```

The API will be available at:
- Local: https://localhost:5000
- Docker: http://localhost:5000
- Swagger UI: https://localhost:5000/swagger

## 📚 API Documentation

The API documentation is available through Swagger UI at `/swagger` when running the application. It provides:

- Detailed endpoint descriptions
- Request/response schemas
- Authentication requirements
- Try-it-out functionality

## 🔒 Authentication

The API uses JWT (JSON Web Token) authentication. To access protected endpoints:

1. Register a new user using the `/api/users/signup` endpoint
2. Sign in using the `/api/users/signin` endpoint
3. Use the returned JWT token in the Authorization header:
```
Authorization: Bearer your-jwt-token
```

## 🏗️ Project Structure

```
TaskHive/
├── src/
│   ├── TaskHive.API/           # API Controllers and Middleware
│   ├── TaskHive.Application/   # Use Cases and DTOs
│   ├── TaskHive.Domain/        # Domain Entities and Interfaces
│   └── TaskHive.Infra/         # Infrastructure Implementation
├── tests/                      # Test Projects
├── Dockerfile                  # Docker configuration
└── docker-compose.yml         # Docker Compose configuration
```

## 🧪 Testing

Run the tests using:
```bash
dotnet test
```

## 🔄 CI/CD

The project includes:
- Docker containerization
- Health checks for container orchestration
- Environment-specific configurations
- Security headers and CORS policies

## 📦 Dependencies

### Main Dependencies
- Microsoft.AspNetCore.Authentication.JwtBearer
- Npgsql.EntityFrameworkCore.PostgreSQL
- Swashbuckle.AspNetCore
- AspNetCore.HealthChecks.UI

### Development Dependencies
- Microsoft.NET.Test.Sdk
- xunit
- Moq

## 🔐 Security Features

- JWT Authentication
- HTTPS Redirection
- Security Headers
- CORS Configuration
- Non-root Docker user
- Input validation
- Error handling middleware

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👥 Authors

- Carlos Figueiredo - Initial work

## 🙏 Acknowledgments

- Clean Architecture principles by Robert C. Martin
- ASP.NET Core team for the amazing framework
- PostgreSQL team for the robust database 