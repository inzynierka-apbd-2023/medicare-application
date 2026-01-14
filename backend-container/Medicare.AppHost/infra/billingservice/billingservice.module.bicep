@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param billingservice_containerport string

param sql_outputs_sqlserverfqdn string

@secure()
param rabbitmq_password_value string

param jwt_secret_value string

param outputs_azure_container_registry_managed_identity_id string

param outputs_managed_identity_client_id string

param outputs_azure_container_apps_environment_id string

param outputs_azure_container_registry_endpoint string

param billingservice_containerimage string

resource billingservice 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'billingservice'
  location: location
  properties: {
    configuration: {
      secrets: [
        {
          name: 'connectionstrings--rabbitmq'
          value: 'amqp://${'medicare'}:${rabbitmq_password_value}@rabbitmq:5672'
        }
      ]
      activeRevisionsMode: 'Single'
      ingress: {
        external: false
        targetPort: billingservice_containerport
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
          image: billingservice_containerimage
          name: 'billingservice'
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
              value: billingservice_containerport
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
              value: jwt_secret_value
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
        minReplicas: 0
        maxReplicas: 1
        rules: [
          {
            name: 'rabbitmq-billing-user-created-scaler'
            custom: {
              type: 'rabbitmq'
              metadata: {
                queueName: 'billing.user_created'
                queueLength: '1'
              }
              auth: [
                {
                  secretRef: 'connectionstrings--rabbitmq'
                  triggerParameter: 'host'
                }
              ]
            }
          }
          {
            name: 'rabbitmq-billing-payment-requests-scaler'
            custom: {
              type: 'rabbitmq'
              metadata: {
                queueName: 'billing.payment_requests'
                queueLength: '1'
              }
              auth: [
                {
                  secretRef: 'connectionstrings--rabbitmq'
                  triggerParameter: 'host'
                }
              ]
            }
          }
          {
            name: 'rabbitmq-billing-appointment-created-scaler'
            custom: {
              type: 'rabbitmq'
              metadata: {
                queueName: 'billing.appointment_created'
                queueLength: '1'
              }
              auth: [
                {
                  secretRef: 'connectionstrings--rabbitmq'
                  triggerParameter: 'host'
                }
              ]
            }
          }
        ]
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