If you want to use Elastic as Reminder's SystemStore, please follow configuration example below


  <SystemStore SystemStoreType="Custom"  DataConnectionString="host=localhost;datacenter=dc1;index=orleans_reminders"
             MembershipTableAssembly="Pk.OrleansUtils.Consul"
             ReminderTableAssembly="Pk.OrleansUtils.ElasticSearch"
             DataConnectionStringForReminders="host=localhost;datacenter=dc1;index=orleans_reminders"
                 />

for adding Elasticsearch grains storage provider modify configuration file with lines below

    <StorageProviders>
      <Provider Type="Pk.OrleansUtils.ElasticSearch.ElasticStorageProvider" Name="Default" DataConnectionString="index=orleans;host=localhost" />
    </StorageProviders>
