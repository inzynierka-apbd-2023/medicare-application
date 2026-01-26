@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param appointmentservice_containerport string

param sql_outputs_sqlserverfqdn string

@secure()
param azurerabbitmqpassword_value string

@secure()
param azurejwtsecret_value string

param azurecorsallowedorigins_value string

param outputs_azure_container_registry_managed_identity_id string

param outputs_managed_identity_client_id string

param outputs_azure_container_apps_environment_id string

param outputs_azure_container_registry_endpoint string

param appointmentservice_containerimage string

resource appointmentservice 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'appointmentservice'
  location: location
  properties: {
    configuration: {
      secrets: [
        {
          name: 'connectionstrings--rabbitmq'
          value: 'amqp://${'guest'}:${azurerabbitmqpassword_value}@rabbitmq:5672'
        }
        {
          name: 'jwt--secretkey'
          value: azurejwtsecret_value
        }
      ]
      activeRevisionsMode: 'Single'
      ingress: {
        external: false
        targetPort: appointmentservice_containerport
        transport: 'http'
      }
      registries: [
        {
          server: outputs_azure_container_registry_endpoint
          identity: outputs_azure_container_registry_managed_identity_id
        }
      ]
    }
    environmentId: outputs_azure_container_apps_environment_id
    template: {
      containers: [
        {
          image: appointmentservice_containerimage
          name: 'appointmentservice'
          env: [
            {
              name: 'OTEL_DOTNET_EXPERIMENTAL_OTLP_EMIT_EXCEPTION_LOG_ATTRIBUTES'
              value: 'true'
            }
            {
              name: 'OTEL_DOTNET_EXPERIMENTAL_OTLP_EMIT_EVENT_LOG_ATTRIBUTES'
              value: 'true'
            }
            {
              name: 'OTEL_DOTNET_EXPERIMENTAL_OTLP_RETRY'
              value: 'in_memory'
            }
            {
              name: 'ASPNETCORE_FORWARDEDHEADERS_ENABLED'
              value: 'true'
            }
            {
              name: 'HTTP_PORTS'
              value: appointmentservice_containerport
            }
            {
              name: 'ConnectionStrings__MedicareDb'
              value: '${'Server=tcp:${sql_outputs_sqlserverfqdn},1433;Encrypt=True;Authentication="Active Directory Default"'};Database=MedicareDb'
            }
            {
              name: 'AZURE_SQL_CONNECTIONSTRING'
              value: '${'Server=tcp:${sql_outputs_sqlserverfqdn},1433;Encrypt=True;Authentication="Active Directory Default"'};Database=MedicareDb'
            }
            {
              name: 'ConnectionStrings__rabbitmq'
              secretRef: 'connectionstrings--rabbitmq'
            }
            {
              name: 'Jwt__SecretKey'
              secretRef: 'jwt--secretkey'
            }
            {
              name: 'Jwt__Issuer'
              value: 'UserService'
            }
            {
              name: 'Jwt__Audience'
              value: 'MedicareApp'
            }
            {
              name: 'Cors__AllowedOrigins__0'
              value: azurecorsallowedorigins_value
            }
            {
              name: 'AZURE_CLIENT_ID'
              value: outputs_managed_identity_client_id
            }
          ]
          resources: {
            cpu: '0.25'
            memory: '0.5Gi'
          }
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${outputs_azure_container_registry_managed_identity_id}': { }
    }
  }
}