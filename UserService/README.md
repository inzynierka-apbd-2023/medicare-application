# Medicare User Service

A .NET 8 Web API microservice for managing user authentication and user data in the Medicare application.

## Features

- **User Management**: CRUD operations for user accounts
- **Authentication**: JWT-based authentication with login/register endpoints
- **Authorization**: Role-based access control (Admin, Doctor, Patient)
- **Security**: BCrypt password hashing, JWT token validation
- **Database**: Entity Framework Core with SQL Server
- **Documentation**: Swagger/OpenAPI integration
- **Health Checks**: Database connectivity monitoring
- **Docker Support**: Containerized deployment

## Quick Start

### Prerequisites

- .NET 8 SDK
- SQL Server (or Docker for containerized database)
- Docker (optional, for containerized deployment)

### Running Locally

1. **Start the database** (using Docker):
   ```bash
   cd ../dev-db
   docker-compose up -d
   ```

2. **Run the application**:
   ```bash
   cd UserService
   dotnet run
   ```

3. **Access the API**:
   - Swagger UI: `http://localhost:5000`
   - API Base URL: `http://localhost:5000/api`

### Running with Docker

1. **Build and run all services**:
   ```bash
   # From the root directory
   docker-compose up --build
   ```

2. **Access the services**:
   - User Service API: `http://localhost:5001`
   - Database: `localhost:1433`

## API Endpoints

### Authentication

- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration
- `POST /api/auth/test-token` - Generate test JWT token (development)

### Users (Requires Authentication)

- `GET /api/users` - Get all users
- `GET /api/users/{id}` - Get user by ID
- `GET /api/users/username/{username}` - Get user by username
- `POST /api/users` - Create new user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user (Admin only)
- `HEAD /api/users/{id}` - Check if user exists

### Health Check

- `GET /health` - Service health status

## Configuration

### Environment Variables

- `ASPNETCORE_ENVIRONMENT` - Environment (Development/Production)
- `ConnectionStrings__DefaultConnection` - Database connection string

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=MedicareDB;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true;Encrypt=false;"
  },
  "Jwt": {
    "Issuer": "MedicareApp",
    "Audience": "MedicareUsers",
    "SecretKey": "YourSecretKey",
    "ExpiryInHours": 24
  }
}
```

## Azure SQL with Azure AD Default (this repo)

The service uses Azure AD Default authentication to connect to Azure SQL. No passwords are stored; credentials flow from your developer login (az login) or the managed identity in production.

- Environments
  - Test: connects to `medicare-db-dev` with `Authentication=Active Directory Default`
  - Production: connects to `medicare-db` with the same auth method

- Connection string shape
  `Server=tcp:<server>.database.windows.net,1433;Initial Catalog=<db>;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=Active Directory Default;`

- Program.cs behavior
  - Reads connection from `ConnectionStrings:DefaultConnection` or env vars `AZURE_SQL_CONNECTIONSTRING` / `ConnectionStrings__DefaultConnection`.
  - Enables SQL retry with `EnableRetryOnFailure`.
  - Applies `context.Database.Migrate()` automatically when not Production.

### Local run against Azure dev DB

1) Azure CLI login and subscription

```bash
az login
az account set --subscription "Azure for Students"
```

1) Run the API in Test against dev DB

```bash
export ASPNETCORE_ENVIRONMENT=Test
export ConnectionStrings__DefaultConnection="Server=tcp:medicareapp-dbserver.database.windows.net,1433;Initial Catalog=medicare-db-dev;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=Active Directory Default;"
dotnet run --project UserService/UserService/UserService.csproj --no-launch-profile --urls http://0.0.0.0:5099
```

1) Verify endpoints

```bash
curl http://127.0.0.1:5099/health
curl -X POST http://127.0.0.1:5099/api/auth/register -H 'Content-Type: application/json' \
  -d '{"username":"demo_user","email":"demo@example.com","password":"Password123!","firstName":"Demo","lastName":"User","role":"Patient"}'
```

### Production (App Service + Managed Identity)

1) Enable system-assigned MI on the App Service.
2) In `medicare-db`, create the MI user and grant roles (reader/writer).
3) App settings:
   - `ASPNETCORE_ENVIRONMENT=Production`
   - Connection string (DefaultConnection, type SQLAzure) with `Authentication=Active Directory Default`.


## Database Schema

### Users Table

- `Id` (GUID) - Primary key
- `Username` (string) - Unique username
- `Email` (string) - User email
- `PasswordHash` (string) - BCrypt hashed password
- `FirstName` (string) - User's first name
- `LastName` (string) - User's last name
- `PhoneNumber` (string, optional) - Contact number
- `Role` (enum) - Admin, Doctor, or Patient
- `DateOfBirth` (DateTime, optional) - Birth date
- `IsActive` (bool) - Account status
- `CreatedAt` (DateTime) - Creation timestamp
- `UpdatedAt` (DateTime) - Last update timestamp

## Security

- **Password Hashing**: BCrypt with salt rounds
- **JWT Tokens**: HS256 algorithm with configurable expiry
- **Authorization**: Role-based access control
- **CORS**: Configurable allowed origins
- **HTTPS**: Enforced in production

## Development

### Project Structure

```text
UserService/
??? Controllers/          # API controllers
??? Data/                # Entity Framework context
??? DTOs/                # Data Transfer Objects
??? Models/              # Entity models
??? Services/            # Business logic services
??? Program.cs           # Application configuration
??? appsettings.json     # Configuration files
```

### Testing the API

1. **Register a new user**:

  ```bash
   curl -X POST http://localhost:5001/api/auth/register \
     -H "Content-Type: application/json" \
     -d '{
       "username": "john_doe",
       "email": "john@example.com",
       "password": "password123",
       "firstName": "John",
       "lastName": "Doe",
       "role": 2
     }'
   ```

1. **Login**:

  ```bash
   curl -X POST http://localhost:5001/api/auth/login \
     -H "Content-Type: application/json" \
     -d '{
       "username": "john_doe",
       "password": "password123"
     }'
   ```

1. **Access protected endpoints** (use token from login response):

  ```bash
   curl -X GET http://localhost:5001/api/users \
     -H "Authorization: Bearer YOUR_JWT_TOKEN"
   ```

## Deployment

The service is designed to be deployed as a Docker container alongside the SQL Server database. The provided `docker-compose.yml` includes:

- Automatic database initialization
- Health checks for both services
- Network isolation
- Volume persistence for database data

## Monitoring

- Health check endpoint: `/health`
- Database connectivity verification
- Swagger documentation for API exploration
- Structured logging with configurable levels

## Security Considerations

- Use strong JWT secret keys in production
- Enable HTTPS/TLS in production
- Configure CORS appropriately for your frontend domains
- Regular password policy enforcement
- Monitor authentication logs for suspicious activity
