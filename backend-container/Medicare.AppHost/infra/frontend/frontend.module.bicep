@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param outputs_azure_container_apps_environment_default_domain string

param outputs_azure_container_registry_managed_identity_id string

param outputs_managed_identity_client_id string

param outputs_azure_container_apps_environment_id string

param outputs_azure_container_registry_endpoint string

param frontend_containerimage string

resource frontend 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'frontend'
  location: location
  properties: {
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 80
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
          image: frontend_containerimage
          name: 'frontend'
          env: [
            {
              name: 'services__userservice__http__0'
              value: 'http://userservice.internal.${outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__userservice__https__0'
              value: 'https://userservice.internal.${outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__practitionerservice__http__0'
              value: 'http://practitionerservice.internal.${outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__practitionerservice__https__0'
              value: 'https://practitionerservice.internal.${outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__patientservice__http__0'
              value: 'http://patientservice.internal.${outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__patientservice__https__0'
              value: 'https://patientservice.internal.${outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__medicalcatalogservice__http__0'
              value: 'http://medicalcatalogservice.internal.${outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__medicalcatalogservice__https__0'
              value: 'https://medicalcatalogservice.internal.${outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__billingservice__http__0'
              value: 'http://billingservice.internal.${outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__billingservice__https__0'
              value: 'https://billingservice.internal.${outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__documentsservice__http__0'
              value: 'http://documentsservice.internal.${outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__documentsservice__https__0'
              value: 'https://documentsservice.internal.${outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__appointmentservice__http__0'
              value: 'http://appointmentservice.internal.${outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__appointmentservice__https__0'
              value: 'https://appointmentservice.internal.${outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__medicalrecordsservice__http__0'
              value: 'http://medicalrecordsservice.internal.${outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__medicalrecordsservice__https__0'
              value: 'https://medicalrecordsservice.internal.${outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__labservice__http__0'
              value: 'http://labservice.internal.${outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__labservice__https__0'
              value: 'https://labservice.internal.${outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__notificationservice__http__0'
              value: 'http://notificationservice.internal.${outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__notificationservice__https__0'
              value: 'https://notificationservice.internal.${outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__messagingservice__http__0'
              value: 'http://messagingservice.internal.${outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__messagingservice__https__0'
              value: 'https://messagingservice.internal.${outputs_azure_container_apps_environment_default_domain}'
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