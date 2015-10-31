using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Pk.Orleans.ApplicationInsights
{
    //, IConfigurableClientMetricsDataPublisher, IConfigurableStatisticsPublisher, 
    public class AppInStatisticsPublisher : IConfigurableSiloMetricsDataPublisher, IConfigurableStatisticsPublisher,IProvider
    {
        public const string InstrumentationKeyProperty = "InstrumentationKey";
        public const string ReportsStatsEnabledKeyProperty = "ReportsStatsEnabled";

        public TelemetryClient Telemetry { get; set; }

        public string InstrumentationKey { get; set; }

        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
        }

        public string DeploymentId { get; private set; }
        public string SiloName { get; private set; }
        public string HostName { get; private set; }
        public bool ReportStatsEnabled { get; private set; }

        public void AddConfiguration(string deploymentId, string hostName, string clientId, IPAddress address)
        {
            throw new NotImplementedException();
        }

        public void AddConfiguration(string deploymentId, bool isSilo, string siloName, SiloAddress address, IPEndPoint gateway, string hostName)
        {
            SiloName = siloName;
            DeploymentId = deploymentId;
            HostName = hostName;
        }

        public Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            _name = name;
            ReportStatsEnabled = false;
            if (config.Properties.ContainsKey(ReportsStatsEnabledKeyProperty))
            {
                bool val = false;
                if (bool.TryParse(config.Properties[ReportsStatsEnabledKeyProperty],out val))
                {
                    ReportStatsEnabled = val;
                }else
                    throw new AppInException.InvalidConfiguration("Invalid " + ReportsStatsEnabledKeyProperty + "boolean value.");
            }

            if (!config.Properties.ContainsKey(InstrumentationKeyProperty))
                throw new AppInException.InvalidConfiguration("Please define "+ InstrumentationKeyProperty+ " attribute with valid instrumentation key or environment variable name enclosed by % containing the key value.");
            InstrumentationKey = config.Properties[InstrumentationKeyProperty];
            if (!String.IsNullOrEmpty(InstrumentationKey) && InstrumentationKey.StartsWith("%") && InstrumentationKey.EndsWith("%"))
                InstrumentationKey = Environment.GetEnvironmentVariable(InstrumentationKey.Substring(1, InstrumentationKey.Length - 2));
            if (String.IsNullOrEmpty(InstrumentationKey))
                throw new AppInException.InvalidConfiguration("Invalid " + InstrumentationKeyProperty + " value.");
            var tc = new TelemetryConfiguration();
            Telemetry = new TelemetryClient();
            Telemetry.InstrumentationKey = InstrumentationKey;
            return Task.CompletedTask;
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
            var pageView = new PageViewTelemetry(SiloName);
            Telemetry.TrackMetric("ActivationCount", metricsData.ActivationCount);
            Telemetry.TrackMetric("CpuUsage", metricsData.CpuUsage);
            Telemetry.TrackMetric("AvailablePhysicalMemory", metricsData.AvailablePhysicalMemory);
            IDictionary<string, double> metrics = new Dictionary<string, double>();
            Telemetry.TrackPageView(pageView);
            Telemetry.Flush();
            return Task.CompletedTask;
        }

        public Task ReportStats(List<ICounter> statsCounters)
        {
            if (ReportStatsEnabled)
            {
                foreach (var c in statsCounters)
                {
                    var value = c.GetValueString();
                    double dval = 0.0;
                    if (double.TryParse(value, out dval))
                    {
                        Telemetry.TrackMetric(c.Name, dval);
                    }
                }
                Telemetry.Flush();
            }
            return Task.CompletedTask;
        }
    }
}
