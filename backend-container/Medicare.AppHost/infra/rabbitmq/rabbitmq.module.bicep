@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

@secure()
param azurerabbitmqpassword_value string

param outputs_azure_container_registry_managed_identity_id string

param outputs_managed_identity_client_id string

param outputs_azure_container_apps_environment_id string

resource rabbitmq 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'rabbitmq'
  location: location
  properties: {
    configuration: {
      secrets: [
        {
          name: 'rabbitmq-default-pass'
          value: azurerabbitmqpassword_value
        }
      ]
      activeRevisionsMode: 'Single'
      ingress: {
        external: false
        targetPort: 5672
        transport: 'tcp'
        additionalPortMappings: [
          {
            external: false
            targetPort: 15672
          }
        ]
      }
    }
    environmentId: outputs_azure_container_apps_environment_id
    template: {
      containers: [
        {
          image: 'docker.io/library/rabbitmq:4.0-management'
          name: 'rabbitmq'
          env: [
            {
              name: 'RABBITMQ_DEFAULT_USER'
              value: 'guest'
            }
            {
              name: 'RABBITMQ_DEFAULT_PASS'
              secretRef: 'rabbitmq-default-pass'
            }
            {
              name: 'AZURE_CLIENT_ID'
              value: outputs_managed_identity_client_id
            }
            {
              name: 'RABBITMQ_SERVER_ADDITIONAL_ERL_ARGS'
              value: '-rabbit loopback_users []'
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