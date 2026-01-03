var builder = DistributedApplication.CreateBuilder(args);

// External resources
// Uses local SQL Server container for development, Azure SQL Database for production
var sql = builder.AddAzureSqlServer("sql")
                 .RunAsContainer();

// Define separate databases for each service (11 services use SQL)
// Define ONE shared database for all services (Schema isolation used)
var sharedDb = sql.AddDatabase("MedicareDb");

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
                      .WithManagementPlugin();

// JWT Secret Key - uses Aspire's secure parameter management
// For local development: uses the default value below
// For Azure deployment: set via 'azd env set JWT_SECRET <value>' or Azure Key Vault
// Generated cryptographically secure 64-character key (change for production!)
var jwtSecret = builder.AddParameter("jwt-secret", secret: true);

// Services - ALL share the same 'sharedDb'
var userService = builder.AddProject<Projects.UserService>("userservice")
                         .WithReference(sharedDb)
                         .WithEnvironment("AZURE_SQL_CONNECTIONSTRING", sharedDb.Resource.ConnectionStringExpression)
                         .WithReference(rabbitmq)
                         .WaitFor(sharedDb)
                         .WaitFor(rabbitmq)
                         .WithEnvironment("Jwt__SecretKey", jwtSecret)
                         .WithEnvironment("Jwt__Issuer", "UserService")
                         .WithEnvironment("Jwt__Audience", "MedicareApp");

var practitionerService = builder.AddProject<Projects.PractitionerService>("practitionerservice")
                                 .WithReference(sharedDb)
                                 .WithEnvironment("AZURE_SQL_CONNECTIONSTRING", sharedDb.Resource.ConnectionStringExpression)
                                 .WithReference(rabbitmq)
                                 .WaitFor(sharedDb)
                                 .WaitFor(rabbitmq)
                                 .WaitFor(userService) // Wait for UserService to create User_Profile table
                                 .WithEnvironment("Jwt__SecretKey", jwtSecret)
                                 .WithEnvironment("Jwt__Issuer", "UserService")
                                 .WithEnvironment("Jwt__Audience", "MedicareApp");

var patientService = builder.AddProject<Projects.PatientService>("patientservice")
                            .WithReference(sharedDb)
                            .WithEnvironment("AZURE_SQL_CONNECTIONSTRING", sharedDb.Resource.ConnectionStringExpression)
                            .WithReference(rabbitmq)
                            .WaitFor(sharedDb)
                            .WaitFor(rabbitmq)
                            .WithEnvironment("Jwt__SecretKey", jwtSecret)
                            .WithEnvironment("Jwt__Issuer", "UserService")
                            .WithEnvironment("Jwt__Audience", "MedicareApp");

var catalogService = builder.AddProject<Projects.MedicalCatalogService>("medicalcatalogservice")
                            .WithReference(sharedDb)
                            .WithEnvironment("AZURE_SQL_CONNECTIONSTRING", sharedDb.Resource.ConnectionStringExpression)
                            .WaitFor(sharedDb)
                            .WithEnvironment("Jwt__SecretKey", jwtSecret)
                            .WithEnvironment("Jwt__Issuer", "UserService")
                            .WithEnvironment("Jwt__Audience", "MedicareApp");

var billingService = builder.AddProject<Projects.BillingService>("billingservice")
                            .WithReference(sharedDb)
                            .WithEnvironment("AZURE_SQL_CONNECTIONSTRING", sharedDb.Resource.ConnectionStringExpression)
                            .WaitFor(sharedDb)
                            .WithEnvironment("Jwt__SecretKey", jwtSecret)
                            .WithEnvironment("Jwt__Issuer", "UserService")
                            .WithEnvironment("Jwt__Audience", "MedicareApp");

var documentsService = builder.AddProject<Projects.DocumentsService>("documentsservice")
                              .WithReference(sharedDb)
                              .WithEnvironment("AZURE_SQL_CONNECTIONSTRING", sharedDb.Resource.ConnectionStringExpression)
                              .WithReference(rabbitmq)
                              .WaitFor(sharedDb)
                              .WaitFor(rabbitmq)
                              .WithEnvironment("Jwt__SecretKey", jwtSecret)
                              .WithEnvironment("Jwt__Issuer", "UserService")
                              .WithEnvironment("Jwt__Audience", "MedicareApp");

var appointmentService = builder.AddProject<Projects.AppointmentService>("appointmentservice")
                                .WithReference(sharedDb)
                                .WithEnvironment("AZURE_SQL_CONNECTIONSTRING", sharedDb.Resource.ConnectionStringExpression)
                                .WithReference(rabbitmq)
                                .WaitFor(sharedDb)
                                .WaitFor(rabbitmq)
                                .WithEnvironment("Jwt__SecretKey", jwtSecret)
                                .WithEnvironment("Jwt__Issuer", "UserService")
                                .WithEnvironment("Jwt__Audience", "MedicareApp");

var recordsService = builder.AddProject<Projects.MedicalRecordsService>("medicalrecordsservice")
                            .WithReference(sharedDb)
                            .WithEnvironment("AZURE_SQL_CONNECTIONSTRING", sharedDb.Resource.ConnectionStringExpression)
                            .WaitFor(sharedDb)
                            .WithEnvironment("Jwt__SecretKey", jwtSecret)
                            .WithEnvironment("Jwt__Issuer", "UserService")
                            .WithEnvironment("Jwt__Audience", "MedicareApp");

var labService = builder.AddProject<Projects.LabService>("labservice")
                        .WithReference(sharedDb)
                        .WithEnvironment("AZURE_SQL_CONNECTIONSTRING", sharedDb.Resource.ConnectionStringExpression)
                        .WaitFor(sharedDb)
                        .WithEnvironment("Jwt__SecretKey", jwtSecret)
                        .WithEnvironment("Jwt__Issuer", "UserService")
                        .WithEnvironment("Jwt__Audience", "MedicareApp");

var archiveService = builder.AddProject<Projects.ArchiveService>("archiveservice")
                            .WithReference(sharedDb)
                            .WithEnvironment("AZURE_SQL_CONNECTIONSTRING", sharedDb.Resource.ConnectionStringExpression)
                            .WithReference(rabbitmq)
                            .WaitFor(sharedDb)
                            .WaitFor(rabbitmq)
                            .WithEnvironment("Jwt__SecretKey", jwtSecret)
                            .WithEnvironment("Jwt__Issuer", "UserService")
                            .WithEnvironment("Jwt__Audience", "MedicareApp");

var notificationService = builder.AddProject<Projects.NotificationService>("notificationservice")
                                 .WithReference(sharedDb)
                                 .WithEnvironment("AZURE_SQL_CONNECTIONSTRING", sharedDb.Resource.ConnectionStringExpression)
                                 .WithReference(rabbitmq)
                                 .WaitFor(sharedDb)
                                 .WaitFor(rabbitmq)
                                 .WithEnvironment("Jwt__SecretKey", jwtSecret)
                                 .WithEnvironment("Jwt__Issuer", "UserService")
                                 .WithEnvironment("Jwt__Audience", "MedicareApp");

var messagingService = builder.AddProject<Projects.MessagingService>("messagingservice")
                              .WithReference(sharedDb)
                              .WithEnvironment("AZURE_SQL_CONNECTIONSTRING", sharedDb.Resource.ConnectionStringExpression)
                              .WaitFor(sharedDb)
                              .WithEnvironment("Jwt__SecretKey", jwtSecret)
                              .WithEnvironment("Jwt__Issuer", "UserService")
                              .WithEnvironment("Jwt__Audience", "MedicareApp");

var pdfService = builder.AddProject<Projects.PdfService>("pdfservice")
                        .WithReference(rabbitmq)
                        .WaitFor(rabbitmq)
                        .WithEnvironment("Jwt__SecretKey", jwtSecret)
                        .WithEnvironment("Jwt__Issuer", "UserService")
                        .WithEnvironment("Jwt__Audience", "MedicareApp");

// Frontend (Docker)
// Deployed as an Nginx container serving the React build
builder.AddDockerfile("frontend", "../../frontend-container/medicare-frontend")
       .WithHttpEndpoint(targetPort: 80, name: "http")
       .WithExternalHttpEndpoints()
       .WithReference(userService)
       .WithReference(practitionerService)
       .WithReference(patientService)
       .WithReference(catalogService)
       .WithReference(billingService)
       .WithReference(documentsService)
       .WithReference(appointmentService)
       .WithReference(recordsService)
       .WithReference(labService)
       .WithReference(notificationService)
       .WithReference(messagingService)
       .WaitFor(userService);

// Orchestration complete
builder.Build().Run();

