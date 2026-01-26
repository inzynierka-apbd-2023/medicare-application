using Azure.Provisioning.AppContainers;
using System.Globalization;

var builder = DistributedApplication.CreateBuilder(args);

// Ensure that decimals are generated with dots, not commas (e.g. 0.25 for CPU)
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

// External resources
// Uses local SQL Server container for development, Azure SQL Database for production
var sql = builder.AddAzureSqlServer("sql")
                 .RunAsContainer();

// Define ONE shared database for all services (Schema isolation used)
var sharedDb = sql.AddDatabase("MedicareDb");

var rabbitmqPassword = builder.AddParameter("azurerabbitmqpassword", secret: true);
var rabbitmq = builder.AddRabbitMQ("rabbitmq", password: rabbitmqPassword)
                      .WithEnvironment("RABBITMQ_DEFAULT_USER", "guest")
                      .WithEnvironment("RABBITMQ_DEFAULT_PASS", rabbitmqPassword)
                      .WithManagementPlugin()
                      .PublishAsAzureContainerApp((infra, app) =>
                      {
                          app.Template.Scale = new ContainerAppScale
                          {
                              MinReplicas = 1,
                              MaxReplicas = 1
                          };
                          app.Template.Containers[0].Value.Resources = new()
                          {
                              Cpu = 0.25,
                              Memory = "0.5Gi"
                          };
                      });


var jwtSecret = builder.AddParameter("azurejwtsecret", secret: true);

// SMTP Configuration for email sending (password reset, welcome emails)
var smtpUsername = builder.AddParameter("azuresmtpusername", secret: true);
var smtpPassword = builder.AddParameter("azuresmtppassword", secret: true);
var frontendBaseUrl = builder.AddParameter("azurefrontendbaseurl");

// CORS and Webhook Configuration
var corsAllowedOrigins = builder.AddParameter("azurecorsallowedorigins");
var webhooksSecretMock = builder.AddParameter("azurewebhookssecretmock", secret: true);
var webhooksSecretStripe = builder.AddParameter("azurewebhookssecretstripe", secret: true);

// Services - ALL share the same 'sharedDb'
var userService = builder.AddProject<Projects.UserService>("userservice")
                         .WithReference(sharedDb)
                         .WithEnvironment("AZURE_SQL_CONNECTIONSTRING", sharedDb.Resource.ConnectionStringExpression)
                         .WithReference(rabbitmq)
                         .WaitFor(sharedDb)
                         .WaitFor(rabbitmq)
                         .WithEnvironment("Jwt__SecretKey", jwtSecret)
                         .WithEnvironment("Jwt__Issuer", "UserService")
                         .WithEnvironment("Jwt__Audience", "MedicareApp")
                         .WithEnvironment("Cors__AllowedOrigins__0", corsAllowedOrigins)
                         .PublishAsAzureContainerApp((infra, app) =>
                         {
                             app.Template.Scale = new ContainerAppScale
                             {
                                 MinReplicas = 1,
                                 MaxReplicas = 1
                             };
                             app.Template.Containers[0].Value.Resources = new()
                             {
                                 Cpu = 0.25,
                                 Memory = "0.5Gi"
                             };
                         });


var practitionerService = builder.AddProject<Projects.PractitionerService>("practitionerservice")
                                 .WithReference(sharedDb)
                                 .WithEnvironment("AZURE_SQL_CONNECTIONSTRING", sharedDb.Resource.ConnectionStringExpression)
                                 .WithReference(rabbitmq)
                                 .WaitFor(sharedDb)
                                 .WaitFor(rabbitmq)
                                 .WaitFor(userService)
                                 .WithEnvironment("Jwt__SecretKey", jwtSecret)
                                 .WithEnvironment("Jwt__Issuer", "UserService")
                                 .WithEnvironment("Jwt__Audience", "MedicareApp")
                                 .WithEnvironment("Cors__AllowedOrigins__0", corsAllowedOrigins)
                                 .PublishAsAzureContainerApp((infra, app) =>
                                 {
                                     app.Template.Scale = new ContainerAppScale
                                     {
                                         MinReplicas = 1,
                                         MaxReplicas = 1
                                     };
                                     app.Template.Containers[0].Value.Resources = new()
                                     {
                                         Cpu = 0.25,
                                         Memory = "0.5Gi"
                                     };
                                 });

        

var patientService = builder.AddProject<Projects.PatientService>("patientservice")
                            .WithReference(sharedDb)
                            .WithEnvironment("AZURE_SQL_CONNECTIONSTRING", sharedDb.Resource.ConnectionStringExpression)
                            .WithReference(rabbitmq)
                            .WaitFor(sharedDb)
                            .WaitFor(rabbitmq)
                            .WaitFor(userService)
                            .WithEnvironment("Jwt__SecretKey", jwtSecret)
                            .WithEnvironment("Jwt__Issuer", "UserService")
                            .WithEnvironment("Jwt__Audience", "MedicareApp")
                            .WithEnvironment("Cors__AllowedOrigins__0", corsAllowedOrigins)
                            .PublishAsAzureContainerApp((infra, app) =>
                            {
                                app.Template.Scale = new ContainerAppScale
                                {
                                    MinReplicas = 1,
                                    MaxReplicas = 1
                                };
                                app.Template.Containers[0].Value.Resources = new()
                                {
                                    Cpu = 0.25,
                                    Memory = "0.5Gi"
                                };
                            });

   

var catalogService = builder.AddProject<Projects.MedicalCatalogService>("medicalcatalogservice")
                            .WithReference(sharedDb)
                            .WithEnvironment("AZURE_SQL_CONNECTIONSTRING", sharedDb.Resource.ConnectionStringExpression)
                            .WithReference(rabbitmq)
                            .WaitFor(sharedDb)
                            .WaitFor(rabbitmq)
                            .WaitFor(userService)
                            .WithEnvironment("Jwt__SecretKey", jwtSecret)
                            .WithEnvironment("Jwt__Issuer", "UserService")
                            .WithEnvironment("Jwt__Audience", "MedicareApp")
                            .WithEnvironment("Cors__AllowedOrigins__0", corsAllowedOrigins)
                            .PublishAsAzureContainerApp((infra, app) =>
                            {
                                app.Template.Scale = new ContainerAppScale
                                {
                                    MinReplicas = 1,
                                    MaxReplicas = 1
                                };
                                app.Template.Containers[0].Value.Resources = new()
                                {
                                    Cpu = 0.25,
                                    Memory = "0.5Gi"
                                };
                            });

   

var billingService = builder.AddProject<Projects.BillingService>("billingservice")
                            .WithReference(sharedDb)
                            .WithEnvironment("AZURE_SQL_CONNECTIONSTRING", sharedDb.Resource.ConnectionStringExpression)
                            .WithReference(rabbitmq)
                            .WaitFor(sharedDb)
                            .WaitFor(rabbitmq)
                            .WaitFor(userService)
                            .WithEnvironment("Jwt__SecretKey", jwtSecret)
                            .WithEnvironment("Jwt__Issuer", "UserService")
                            .WithEnvironment("Jwt__Audience", "MedicareApp")
                            .WithEnvironment("Cors__AllowedOrigins__0", corsAllowedOrigins)
                            .WithEnvironment("Webhooks__Secrets__mock", webhooksSecretMock)
                            .WithEnvironment("Webhooks__Secrets__stripe", webhooksSecretStripe)
                            .PublishAsAzureContainerApp((infra, app) =>
                            {
                                app.Template.Scale = new ContainerAppScale
                                {
                                    MinReplicas = 1,
                                    MaxReplicas = 1
                                };
                                app.Template.Containers[0].Value.Resources = new()
                                {
                                    Cpu = 0.25,
                                    Memory = "0.5Gi"
                                };
                            });

   

var documentsService = builder.AddProject<Projects.DocumentsService>("documentsservice")
                              .WithReference(sharedDb)
                              .WithEnvironment("AZURE_SQL_CONNECTIONSTRING", sharedDb.Resource.ConnectionStringExpression)
                              .WithReference(rabbitmq)
                              .WaitFor(sharedDb)
                              .WaitFor(rabbitmq)
                              .WaitFor(userService)
                              .WithEnvironment("Jwt__SecretKey", jwtSecret)
                              .WithEnvironment("Jwt__Issuer", "UserService")
                              .WithEnvironment("Jwt__Audience", "MedicareApp")
                              .WithEnvironment("Cors__AllowedOrigins__0", corsAllowedOrigins)
                              .PublishAsAzureContainerApp((infra, app) =>
                              {
                                  app.Template.Scale = new ContainerAppScale
                                  {
                                      MinReplicas = 1,
                                      MaxReplicas = 1
                                  };
                                  app.Template.Containers[0].Value.Resources = new()
                                  {
                                      Cpu = 0.25,
                                      Memory = "0.5Gi"
                                  };
                              });

     

var appointmentService = builder.AddProject<Projects.AppointmentService>("appointmentservice")
                                .WithReference(sharedDb)
                                .WithEnvironment("AZURE_SQL_CONNECTIONSTRING", sharedDb.Resource.ConnectionStringExpression)
                                .WithReference(rabbitmq)
                                .WaitFor(sharedDb)
                                .WaitFor(rabbitmq)
                                .WaitFor(userService)
                                .WithEnvironment("Jwt__SecretKey", jwtSecret)
                                .WithEnvironment("Jwt__Issuer", "UserService")
                                .WithEnvironment("Jwt__Audience", "MedicareApp")
                                .WithEnvironment("Cors__AllowedOrigins__0", corsAllowedOrigins)
                                .PublishAsAzureContainerApp((infra, app) =>
                                {
                                    app.Template.Scale = new ContainerAppScale
                                    {
                                        MinReplicas = 1,
                                        MaxReplicas = 1
                                    };
                                    app.Template.Containers[0].Value.Resources = new()
                                    {
                                        Cpu = 0.25,
                                        Memory = "0.5Gi"
                                    };
                                });

       

var recordsService = builder.AddProject<Projects.MedicalRecordsService>("medicalrecordsservice")
                            .WithReference(sharedDb)
                            .WithEnvironment("AZURE_SQL_CONNECTIONSTRING", sharedDb.Resource.ConnectionStringExpression)
                            .WaitFor(sharedDb)
                            .WaitFor(userService)
                            .WithEnvironment("Jwt__SecretKey", jwtSecret)
                            .WithEnvironment("Jwt__Issuer", "UserService")
                            .WithEnvironment("Jwt__Audience", "MedicareApp")
                            .WithEnvironment("Cors__AllowedOrigins__0", corsAllowedOrigins)
                            .PublishAsAzureContainerApp((infra, app) =>
                            {
                                app.Template.Scale = new ContainerAppScale
                                {
                                    MinReplicas = 1,
                                    MaxReplicas = 1
                                };
                                app.Template.Containers[0].Value.Resources = new()
                                {
                                    Cpu = 0.25,
                                    Memory = "0.5Gi"
                                };
                            });

   

var labService = builder.AddProject<Projects.LabService>("labservice")
                        .WithReference(sharedDb)
                        .WithEnvironment("AZURE_SQL_CONNECTIONSTRING", sharedDb.Resource.ConnectionStringExpression)
                        .WithReference(rabbitmq)
                        .WaitFor(sharedDb)
                        .WaitFor(rabbitmq)
                        .WaitFor(userService)
                        .WithEnvironment("Jwt__SecretKey", jwtSecret)
                        .WithEnvironment("Jwt__Issuer", "UserService")
                        .WithEnvironment("Jwt__Audience", "MedicareApp")
                        .WithEnvironment("Cors__AllowedOrigins__0", corsAllowedOrigins)
                        .PublishAsAzureContainerApp((infra, app) =>
                        {
                            app.Template.Scale = new ContainerAppScale
                            {
                                MinReplicas = 1,
                                MaxReplicas = 1
                            };
                            app.Template.Containers[0].Value.Resources = new()
                            {
                                Cpu = 0.25,
                                Memory = "0.5Gi"
                            };
                        });

var archiveService = builder.AddProject<Projects.ArchiveService>("archiveservice")
                            .WithReference(sharedDb)
                            .WithEnvironment("AZURE_SQL_CONNECTIONSTRING", sharedDb.Resource.ConnectionStringExpression)
                            .WithReference(rabbitmq)
                            .WaitFor(sharedDb)
                            .WaitFor(rabbitmq)
                            .WaitFor(userService)
                            .WithEnvironment("Jwt__SecretKey", jwtSecret)
                            .WithEnvironment("Jwt__Issuer", "UserService")
                            .WithEnvironment("Jwt__Audience", "MedicareApp")
                            .WithEnvironment("Cors__AllowedOrigins__0", corsAllowedOrigins)
                            .PublishAsAzureContainerApp((infra, app) =>
                            {
                                app.Template.Scale = new ContainerAppScale
                                {
                                    MinReplicas = 1,
                                    MaxReplicas = 1
                                };
                                app.Template.Containers[0].Value.Resources = new()
                                {
                                    Cpu = 0.25,
                                    Memory = "0.5Gi"
                                };
                            });

   

var notificationService = builder.AddProject<Projects.NotificationService>("notificationservice")
                                 .WithReference(sharedDb)
                                 .WithEnvironment("AZURE_SQL_CONNECTIONSTRING", sharedDb.Resource.ConnectionStringExpression)
                                 .WithReference(rabbitmq)
                                 .WaitFor(sharedDb)
                                 .WaitFor(rabbitmq)
                                 .WaitFor(userService)
                                 .WithEnvironment("Smtp__Host", "smtp.gmail.com")
                                 .WithEnvironment("Smtp__Port", "587")
                                 .WithEnvironment("Smtp__Username", smtpUsername)
                                 .WithEnvironment("Smtp__Password", smtpPassword)
                                 .WithEnvironment("Smtp__FromEmail", smtpUsername)
                                 .WithEnvironment("Smtp__FromName", "Medicare App")
                                 .WithEnvironment("FrontendBaseUrl", frontendBaseUrl)
                                 .WithEnvironment("Jwt__SecretKey", jwtSecret)
                                 .WithEnvironment("Jwt__Issuer", "UserService")
                                 .WithEnvironment("Jwt__Audience", "MedicareApp")
                                 .WithEnvironment("Cors__AllowedOrigins__0", corsAllowedOrigins)
                                 .PublishAsAzureContainerApp((infra, app) =>
                                 {
                                     app.Template.Scale = new ContainerAppScale
                                     {
                                         MinReplicas = 1,
                                         MaxReplicas = 1
                                     };
                                     app.Template.Containers[0].Value.Resources = new()
                                     {
                                         Cpu = 0.25,
                                         Memory = "0.5Gi"
                                     };
                                 });

        

var messagingService = builder.AddProject<Projects.MessagingService>("messagingservice")
                              .WithReference(sharedDb)
                              .WithEnvironment("AZURE_SQL_CONNECTIONSTRING", sharedDb.Resource.ConnectionStringExpression)
                              .WithReference(rabbitmq)
                              .WaitFor(sharedDb)
                              .WaitFor(rabbitmq)
                              .WaitFor(userService)
                              .WithEnvironment("Jwt__SecretKey", jwtSecret)
                              .WithEnvironment("Jwt__Issuer", "UserService")
                              .WithEnvironment("Jwt__Audience", "MedicareApp")
                              .WithEnvironment("Cors__AllowedOrigins__0", corsAllowedOrigins)
                              .PublishAsAzureContainerApp((infra, app) =>
                              {
                                  app.Template.Scale = new ContainerAppScale
                                  {
                                      MinReplicas = 1,
                                      MaxReplicas = 1
                                  };
                                  app.Template.Containers[0].Value.Resources = new()
                                  {
                                      Cpu = 0.25,
                                      Memory = "0.5Gi"
                                  };
                              });

     

var pdfService = builder.AddProject<Projects.PdfService>("pdfservice")
                        .WithReference(rabbitmq)
                        .WaitFor(rabbitmq)
                        .WithEnvironment("Jwt__SecretKey", jwtSecret)
                        .WithEnvironment("Jwt__Issuer", "UserService")
                        .WithEnvironment("Jwt__Audience", "MedicareApp")
                        .WithEnvironment("Cors__AllowedOrigins__0", corsAllowedOrigins)
                        .PublishAsAzureContainerApp((infra, app) =>
                        {
                            app.Template.Scale = new ContainerAppScale
                            {
                                MinReplicas = 1,
                                MaxReplicas = 1
                            };
                            app.Template.Containers[0].Value.Resources = new()
                            {
                                Cpu = 0.25,
                                Memory = "0.5Gi"
                            };
                        });

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
       .WaitFor(userService)
       .WaitFor(practitionerService)
       .WaitFor(patientService)
       .WaitFor(catalogService)
       .WaitFor(billingService)
       .WaitFor(documentsService)
       .WaitFor(appointmentService)
       .WaitFor(recordsService)
       .WaitFor(labService)
       .WaitFor(notificationService)
       .WaitFor(messagingService)
       .PublishAsAzureContainerApp((infra, app) =>
       {
           app.Template.Scale = new ContainerAppScale
           {
               MinReplicas = 1,
               MaxReplicas = 1
           };
           app.Template.Containers[0].Value.Resources = new()
           {
               Cpu = 0.25,
               Memory = "0.5Gi"
           };
       });

var app = builder.Build();

app.Run();
