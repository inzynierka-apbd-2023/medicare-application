var builder = DistributedApplication.CreateBuilder(args);

// External resources
// Uses local SQL Server container for development, Azure SQL Database for production
var sql = builder.AddAzureSqlServer("sql")
                 .RunAsContainer();

// Define separate databases for each service (11 services use SQL)
var userDb = sql.AddDatabase("UserServiceDb");
var practitionerDb = sql.AddDatabase("PractitionerServiceDb");
var patientDb = sql.AddDatabase("PatientServiceDb");
var catalogDb = sql.AddDatabase("MedicalCatalogDb");
var billingDb = sql.AddDatabase("BillingDb");
var documentsDb = sql.AddDatabase("DocumentsDb");
var appointmentDb = sql.AddDatabase("AppointmentDb");
var recordsDb = sql.AddDatabase("MedicalRecordsDb");
var labDb = sql.AddDatabase("LabDb");
var notificationDb = sql.AddDatabase("NotificationDb");
var messagingDb = sql.AddDatabase("MessagingDb");
var archiveDb = sql.AddDatabase("ArchiveDb");

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
                      .WithManagementPlugin();

// JWT Secret Key - uses Aspire's secure parameter management
// For local development: uses the default value below
// For Azure deployment: set via 'azd env set JWT_SECRET <value>' or Azure Key Vault
// Generated cryptographically secure 64-character key (change for production!)
var jwtSecret = builder.AddParameter("jwt-secret", secret: true);

// Services - each with its own dedicated database
var userService = builder.AddProject<Projects.UserService>("userservice")
                         .WithReference(userDb)
                         .WithReference(rabbitmq)
                         .WaitFor(userDb)
                         .WaitFor(rabbitmq)
                         .WithEnvironment("Jwt__SecretKey", jwtSecret)
                         .WithEnvironment("Jwt__Issuer", "UserService")
                         .WithEnvironment("Jwt__Audience", "MedicareApp");

var practitionerService = builder.AddProject<Projects.PractitionerService>("practitionerservice")
                                 .WithReference(practitionerDb) 
                                 .WithReference(rabbitmq)
                                 .WaitFor(practitionerDb)
                                 .WaitFor(rabbitmq)
                                 .WithEnvironment("Jwt__SecretKey", jwtSecret)
                                 .WithEnvironment("Jwt__Issuer", "UserService")
                                 .WithEnvironment("Jwt__Audience", "MedicareApp");

var patientService = builder.AddProject<Projects.PatientService>("patientservice")
                            .WithReference(patientDb)
                            .WithReference(rabbitmq)
                            .WaitFor(patientDb)
                            .WaitFor(rabbitmq)
                            .WithEnvironment("Jwt__SecretKey", jwtSecret)
                            .WithEnvironment("Jwt__Issuer", "UserService")
                            .WithEnvironment("Jwt__Audience", "MedicareApp");

var catalogService = builder.AddProject<Projects.MedicalCatalogService>("medicalcatalogservice")
                            .WithReference(catalogDb)
                            .WaitFor(catalogDb)
                            .WithEnvironment("Jwt__SecretKey", jwtSecret)
                            .WithEnvironment("Jwt__Issuer", "UserService")
                            .WithEnvironment("Jwt__Audience", "MedicareApp");

var billingService = builder.AddProject<Projects.BillingService>("billingservice")
                            .WithReference(billingDb)
                            .WaitFor(billingDb)
                            .WithEnvironment("Jwt__SecretKey", jwtSecret)
                            .WithEnvironment("Jwt__Issuer", "UserService")
                            .WithEnvironment("Jwt__Audience", "MedicareApp");

var documentsService = builder.AddProject<Projects.DocumentsService>("documentsservice")
                              .WithReference(documentsDb)
                              .WithReference(rabbitmq)
                              .WaitFor(documentsDb)
                              .WaitFor(rabbitmq)
                              .WithEnvironment("Jwt__SecretKey", jwtSecret)
                              .WithEnvironment("Jwt__Issuer", "UserService")
                              .WithEnvironment("Jwt__Audience", "MedicareApp");

var appointmentService = builder.AddProject<Projects.AppointmentService>("appointmentservice")
                                .WithReference(appointmentDb)
                                .WithReference(rabbitmq)
                                .WaitFor(appointmentDb)
                                .WaitFor(rabbitmq)
                                .WithEnvironment("Jwt__SecretKey", jwtSecret)
                                .WithEnvironment("Jwt__Issuer", "UserService")
                                .WithEnvironment("Jwt__Audience", "MedicareApp");

var recordsService = builder.AddProject<Projects.MedicalRecordsService>("medicalrecordsservice")
                            .WithReference(recordsDb)
                            .WaitFor(recordsDb)
                            .WithEnvironment("Jwt__SecretKey", jwtSecret)
                            .WithEnvironment("Jwt__Issuer", "UserService")
                            .WithEnvironment("Jwt__Audience", "MedicareApp");

var labService = builder.AddProject<Projects.LabService>("labservice")
                        .WithReference(labDb)
                        .WaitFor(labDb)
                        .WithEnvironment("Jwt__SecretKey", jwtSecret)
                        .WithEnvironment("Jwt__Issuer", "UserService")
                        .WithEnvironment("Jwt__Audience", "MedicareApp");

var archiveService = builder.AddProject<Projects.ArchiveService>("archiveservice")
                            .WithReference(archiveDb)
                            .WithReference(rabbitmq)
                            .WaitFor(archiveDb)
                            .WaitFor(rabbitmq)
                            .WithEnvironment("Jwt__SecretKey", jwtSecret)
                            .WithEnvironment("Jwt__Issuer", "UserService")
                            .WithEnvironment("Jwt__Audience", "MedicareApp");

var notificationService = builder.AddProject<Projects.NotificationService>("notificationservice")
                                 .WithReference(notificationDb)
                                 .WithReference(rabbitmq)
                                 .WaitFor(notificationDb)
                                 .WaitFor(rabbitmq)
                                 .WithEnvironment("Jwt__SecretKey", jwtSecret)
                                 .WithEnvironment("Jwt__Issuer", "UserService")
                                 .WithEnvironment("Jwt__Audience", "MedicareApp");

var messagingService = builder.AddProject<Projects.MessagingService>("messagingservice")
                              .WithReference(messagingDb)
                              .WaitFor(messagingDb)
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

