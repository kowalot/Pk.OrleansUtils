
Pk.OrleansUtils.Consul - Consul based membership table for Orleans 

Add to your Orleans server configuration file

  <SystemStore SystemStoreType="Custom"  DataConnectionString="host=localhost;datacenter=dc1;index=orleans_reminders"
             MembershipTableAssembly="Pk.OrleansUtils.Consul"
             ReminderTableAssembly="Pk.OrleansUtils.ElasticSearch"
             DeploymentId="DevHost"
             DataConnectionStringForReminders="host=localhost;datacenter=dc1;index=orleans_reminders"
                 />

Orleans clients can use following configuration

<ClientConfiguration xmlns="urn:orleans">
  <SystemStore SystemStoreType="Custom"
               DeploymentId="DevHost"
               CustomGatewayProviderAssemblyName="Pk.OrleansUtils.Consul"
               DataConnectionString="host=localhost;datacenter=dc1"
  />
</ClientConfiguration>

