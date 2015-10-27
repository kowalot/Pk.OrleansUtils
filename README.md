# Pk.OrleansUtils - Orleans utils

## Pk.OrleansUtils.Consul - Consul based membership table for Orleans 

Add to your Orleans server configuration file

``` xml
  <SystemStore SystemStoreType="Custom"  DataConnectionString="host=localhost;datacenter=dc1;index=orleans_reminders"
             MembershipTableAssembly="Pk.OrleansUtils.Consul"
             DeploymentId="DevHost"
             ReminderTableAssembly="..."
             DataConnectionStringForReminders="..."
                 />
```

Orleans clients can use following configuration

``` xml

<ClientConfiguration xmlns="urn:orleans">
  <SystemStore SystemStoreType="Custom"
               DeploymentId="DevHost"
               CustomGatewayProviderAssemblyName="Pk.OrleansUtils.Consul"
               DataConnectionString="host=localhost;datacenter=dc1"
  />
</ClientConfiguration>

```



## Pk.OrleansUtils.Elastic - ElasticSearch based grains and remainders storage provider

 Add to your Orleans server configuration file following attributes


``` xml
    <StorageProviders>
      <!-- .... -->
      <Provider Type="Pk.OrleansUtils.ElasticSearch.ElasticStorageProvider" Name="Default" DataConnectionString="index=orleans;Host=localhost" />
      <!-- .... -->
    </StorageProviders>
```                


``` xml

  <SystemStore SystemStoreType="Custom"  
             ReminderTableAssembly="Pk.OrleansUtils.ElasticSearch"
             DeploymentId="DevHost"
             DataConnectionStringForReminders="host=localhost;datacenter=dc1;index=orleans_reminders"
                 />
```                

## Known issues

Due to some limitations in Orleans 1.0.10, DataConnectionStringForReminders is exact copy of  DataConnectionString if you configure it via configuration file.

It has been eliminated by https://github.com/dotnet/orleans/pull/925
 