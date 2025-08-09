# Medicare Application Development Database

This directory contains a complete Docker setup for running a Microsoft SQL Server development database for the Medicare application. The database includes schema, seed data, and is configured for easy local development.

## ?? Quick Start

### Prerequisites

- **Docker Desktop** installed and running
- **Git** for cloning the repository
- **Terminal/Command Prompt** with Docker access

### 1. Start the Database

```bash
# Navigate to the dev-db directory
cd dev-db

# Start the database services
docker compose up -d
```

### 2. Verify the Setup

```bash
# Check if containers are running
docker compose ps

# View logs to ensure successful startup
docker compose logs -f
```

### 3. Connect to Database

**Connection Details:**
- **Server:** `localhost,1435` (or your custom port from .env)
- **Database:** `medicare_dev`
- **Username:** `sa`
- **Password:** `123123123` (or your custom password from .env)

## ?? Detailed Setup Guide

### Step 1: Environment Configuration

The database setup uses environment variables defined in the `.env` file:

```properties
# Database port (change if 1435 is in use)
DB_PORT=1435

# SA password (change for production!)
SA_PASSWORD=123123123

# Database name
DB_NAME=medicare_dev
```

**To customize:**
1. Copy the `.env` file to `.env.local` (optional)
2. Modify the values as needed
3. Update your connection strings accordingly

### Step 2: Start the Services

The Docker Compose setup includes two services:

1. **mssql** - SQL Server 2022 Developer Edition
2. **seed** - One-time job to initialize schema and data

```bash
# Start in background (recommended)
docker compose up -d

# Start with logs visible (for debugging)
docker compose up

# Start only the database (without seeding)
docker compose up mssql -d
```

### Step 3: Database Initialization

The database is automatically initialized with:

- **Schema creation** from SQL files in the `seed/` directory
- **Sample data** for development and testing
- **Proper indexes and constraints** for realistic performance

Initialization order:
1. `00_settings.sql` - Database settings and configuration
2. `01_users_and_people.sql` - User accounts and person entities
3. `02_scheduling.sql` - Appointment and scheduling tables
4. `03_documents.sql` - Document management schema
5. `04_comms.sql` - Communication and notification tables
6. `05_billing.sql` - Billing and payment structures
7. `06_medical_domain.sql` - Medical records and clinical data
8. `99_sample_data.sql` - Sample data for testing

## ?? Management Commands

### Database Lifecycle

```bash
# Stop the database
docker compose down

# Stop and remove all data (fresh start)
docker compose down -v

# Restart services
docker compose restart

# View real-time logs
docker compose logs -f

# View logs for specific service
docker compose logs -f mssql
```

### Connecting to Database

**Using Docker:**
```bash
# Connect via docker exec
docker exec -it mssql-dev /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 123123123 -C

# Run a quick test query
docker exec -it mssql-dev /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 123123123 -d medicare_dev -C -Q "SELECT COUNT(*) as TableCount FROM INFORMATION_SCHEMA.TABLES"
```

**Using External Tools:**
- **Azure Data Studio:** `localhost,1435`
- **SQL Server Management Studio:** `localhost,1435`
- **VS Code SQL Tools:** `localhost,1435`

### Data Management

```bash
# Re-seed database (if schema changes)
docker compose restart seed

# Backup database
docker exec mssql-dev /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 123123123 -C -Q "BACKUP DATABASE medicare_dev TO DISK = '/var/opt/mssql/backup/medicare_dev.bak'"

# View database size
docker exec mssql-dev /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 123123123 -d medicare_dev -C -Q "SELECT DB_NAME() as DatabaseName, (SELECT SUM(size) FROM sys.master_files WHERE database_id = DB_ID()) * 8 / 1024 as SizeMB"
```

## ??? Development Workflow

### For Application Development

1. **Start database:** `docker compose up -d`
2. **Connect your app** to `localhost,1435`
3. **Develop and test** against realistic data
4. **Reset when needed:** `docker compose down -v && docker compose up -d`

### For Database Development

1. **Modify seed files** in the `seed/` directory
2. **Test changes:** `docker compose down -v && docker compose up -d`
3. **Verify schema:** Connect and check tables/data
4. **Commit changes** to version control

### Application Connection Strings

**Entity Framework (.NET):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1435;Database=medicare_dev;User Id=sa;Password=123123123;TrustServerCertificate=true;"
  }
}
```

**Node.js (mssql package):**
```javascript
const config = {
  server: 'localhost',
  port: 1435,
  database: 'medicare_dev',
  user: 'sa',
  password: '123123123',
  options: {
    encrypt: false,
    trustServerCertificate: true
  }
};
```

## ?? Troubleshooting

### Common Issues

**Port Already in Use:**
```bash
# Change DB_PORT in .env file
DB_PORT=1436

# Or find what's using the port
netstat -an | grep 1435
```

**Container Won't Start:**
```bash
# Check Docker Desktop is running
docker version

# View detailed logs
docker compose logs mssql

# Check system resources
docker system df
```

**Database Connection Failed:**
```bash
# Verify container is healthy
docker compose ps

# Test connectivity
docker exec mssql-dev /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 123123123 -C -Q "SELECT @@VERSION"

# Check if database exists
docker exec mssql-dev /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 123123123 -C -Q "SELECT name FROM sys.databases"
```

**Seeding Failed:**
```bash
# Check seed container logs
docker compose logs seed

# Manually run seeding
docker compose up seed --force-recreate

# Verify seed files are accessible
docker exec mssql-dev ls -la /seed/
```

### Performance Issues

**Slow Queries:**
- Database runs in Docker with limited resources
- Consider increasing Docker Desktop memory allocation
- Use database indexes appropriately in development

**Container Resource Usage:**
```bash
# Check resource usage
docker stats mssql-dev

# Monitor disk usage
docker system df
```

## ?? Database Schema

### Core Tables

- **Users & Authentication:** User accounts, roles, permissions
- **People & Entities:** Patients, doctors, staff information
- **Scheduling:** Appointments, availability, calendar management
- **Medical Records:** Patient history, diagnoses, treatments
- **Documents:** File storage metadata and organization
- **Communications:** Messages, notifications, alerts
- **Billing:** Payments, insurance, subscription management

### Sample Data

The database includes realistic sample data for:
- 50+ test patients with medical histories
- 20+ healthcare providers across specialties
- 200+ scheduled appointments over 6 months
- Various document types and billing scenarios
- Complete user authentication scenarios

## ?? Security Notes

**Development Only:**
- Uses simple passwords for easy development
- No SSL/TLS encryption configured
- SA account enabled with known password
- **Never use this setup in production!**

**For Production:**
- Use Azure SQL Database or properly secured SQL Server
- Enable encryption and proper authentication
- Use managed identities or secure connection strings
- Follow SQL Server security best practices

## ?? Contributing

### Adding New Tables

1. Create SQL files in `seed/` directory
2. Follow naming convention: `XX_feature_name.sql`
3. Include both schema and sample data
4. Test with fresh database: `docker compose down -v && docker compose up -d`
5. Update this README if needed

### Modifying Existing Schema

1. Update the appropriate SQL file in `seed/`
2. Consider migration scripts for existing data
3. Test thoroughly with fresh initialization
4. Document breaking changes

---

## ?? Support

If you encounter issues:

1. **Check this README** for troubleshooting steps
2. **View container logs:** `docker compose logs -f`
3. **Verify Docker setup:** `docker version` and `docker compose version`
4. **Test basic connectivity:** Try connecting with SQL tools
5. **Create an issue** in the repository with logs and error details

---

**Happy coding! ????**