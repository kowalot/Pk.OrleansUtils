using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Orleans.Runtime.Configuration;
using Orleans.Providers;

namespace Pk.OrleansUtils.ElasticSearch
{
    public class ElasticSearchStatisticsPublisher : IConfigurableStatisticsPublisher, IConfigurableSiloMetricsDataPublisher, IConfigurableClientMetricsDataPublisher, IProvider
    {
        public string Name
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void AddConfiguration(string deploymentId, string hostName, string clientId, IPAddress address)
        {
            throw new NotImplementedException();
        }

        public void AddConfiguration(string deploymentId, bool isSilo, string siloName, SiloAddress address, IPEndPoint gateway, string hostName)
        {
            throw new NotImplementedException();
        }

        public Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            throw new NotImplementedException();
        }

        public Task Init(ClientConfiguration config, IPAddress address, string clientId)
        {
            throw new NotImplementedException();
        }

        public Task Init(string deploymentId, string storageConnectionString, SiloAddress siloAddress, string siloName, IPEndPoint gateway, string hostName)
        {
            throw new NotImplementedException();
        }

        public Task Init(bool isSilo, string storageConnectionString, string deploymentId, string address, string siloName, string hostName)
        {
            throw new NotImplementedException();
        }

        public Task ReportMetrics(IClientPerformanceMetrics metricsData)
        {
            throw new NotImplementedException();
        }

        public Task ReportMetrics(ISiloPerformanceMetrics metricsData)
        {
            throw new NotImplementedException();
        }

        public Task ReportStats(List<ICounter> statsCounters)
        {
            throw new NotImplementedException();
        }
    }
}
