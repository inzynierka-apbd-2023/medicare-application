var builder = DistributedApplication.CreateBuilder(args);

// External resources
var sql = builder.AddSqlServer("sql")
                 .AddDatabase("DefaultConnection");

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
                      .WithManagementPlugin();

// Shared configuration
var jwtSecret = "your_dev_secret_key_change_me_at_least_32_chars_long";

// Services
var userService = builder.AddProject<Projects.UserService>("userservice")
                         .WithReference(sql)
                         .WithReference(rabbitmq)
                         .WaitFor(sql)
                         .WaitFor(rabbitmq)
                         .WithEnvironment("Jwt__SecretKey", jwtSecret);

var practitionerService = builder.AddProject<Projects.PractitionerService>("practitionerservice")
                                 .WithReference(sql) 
                                 .WithReference(rabbitmq)
                                 .WaitFor(sql)
                                 .WaitFor(rabbitmq)
                                 .WithEnvironment("Jwt__SecretKey", jwtSecret);

var patientService = builder.AddProject<Projects.PatientService>("patientservice")
                            .WithReference(sql)
                            .WithReference(rabbitmq)
                            .WaitFor(sql)
                            .WaitFor(rabbitmq)
                            .WithEnvironment("Jwt__SecretKey", jwtSecret);

var catalogService = builder.AddProject<Projects.MedicalCatalogService>("medicalcatalogservice")
                            .WithReference(sql)
                            .WaitFor(sql)
                            .WithEnvironment("Jwt__SecretKey", jwtSecret);

var billingService = builder.AddProject<Projects.BillingService>("billingservice")
                            .WithReference(sql)
                            .WaitFor(sql)
                            .WithEnvironment("Jwt__SecretKey", jwtSecret);

var documentsService = builder.AddProject<Projects.DocumentsService>("documentsservice")
                              .WithReference(sql)
                              .WithReference(rabbitmq)
                              .WaitFor(sql)
                              .WaitFor(rabbitmq)
                              .WithEnvironment("Jwt__SecretKey", jwtSecret);

var appointmentService = builder.AddProject<Projects.AppointmentService>("appointmentservice")
                                .WithReference(sql)
                                .WithReference(rabbitmq)
                                .WaitFor(sql)
                                .WaitFor(rabbitmq)
                                .WithEnvironment("Jwt__SecretKey", jwtSecret);

var recordsService = builder.AddProject<Projects.MedicalRecordsService>("medicalrecordsservice")
                            .WithReference(sql)
                            .WaitFor(sql)
                            .WithEnvironment("Jwt__SecretKey", jwtSecret);

var labService = builder.AddProject<Projects.LabService>("labservice")
                        .WithReference(sql)
                        .WaitFor(sql)
                        .WithEnvironment("Jwt__SecretKey", jwtSecret);

var archiveService = builder.AddProject<Projects.ArchiveService>("archiveservice")
                            .WithReference(rabbitmq)
                            .WaitFor(rabbitmq)
                            .WithEnvironment("Jwt__SecretKey", jwtSecret);

var notificationService = builder.AddProject<Projects.NotificationService>("notificationservice")
                                 .WithReference(sql)
                                 .WithReference(rabbitmq)
                                 .WaitFor(sql)
                                 .WaitFor(rabbitmq)
                                 .WithEnvironment("Jwt__SecretKey", jwtSecret);

var messagingService = builder.AddProject<Projects.MessagingService>("messagingservice")
                              .WithReference(sql)
                              .WaitFor(sql)
                              .WithEnvironment("Jwt__SecretKey", jwtSecret);

var pdfService = builder.AddProject<Projects.PdfService>("pdfservice")
                        .WithReference(rabbitmq)
                        .WaitFor(rabbitmq)
                        .WithEnvironment("Jwt__SecretKey", jwtSecret);

// Frontend (Docker or Project?)
// If the frontend is a NodeJS app (Vite), we usually run it as an external executable or container. 
// For now, assuming we might run it via npm run dev or add it as a container if it's Dockerized.
// Since User requested "efficient configuration", Aspire usually manages backend.
// Adding Frontend as simple container if needed, or NodeApp if using Aspire.Hosting.NodeJs (v9) but we are on v8.2.
// We will just expose the backend services for now. 
// If there is a .csproj for frontend (unlikely for Vite), we'd reference it. 
// Checking file structure earlier: frontend-container/medicare-frontend contains package.json, not csproj.

// Orchestration complete
builder.Build().Run();

