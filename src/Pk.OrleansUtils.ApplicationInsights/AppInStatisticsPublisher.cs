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

namespace Pk.OrleansUtils.ApplicationInsights
{
    //, 
    public class AppInStatisticsPublisher : IConfigurableSiloMetricsDataPublisher, IConfigurableStatisticsPublisher,IProvider, IConfigurableClientMetricsDataPublisher, ISiloMetricsDataPublisher
    {
        public AppInStatisticsPublisher()
        {

        }
        class AppInInitializer : IContextInitializer
        {
            public string DeploymentId { get; set; }
            public AppInInitializer()
            {

            }

            public AppInInitializer(string deploymentId)
            {
                DeploymentId = deploymentId;
            }

            public void Initialize(TelemetryContext ctx)
            {
                ctx.Component.Version = DeploymentId;
            }
        }


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
        public bool Initialized { get; private set; }

        public void AddConfiguration(string deploymentId, string hostName, string clientId, IPAddress address)
        {
            //throw new NotImplementedException();
            TelemetryConfiguration.Active.ContextInitializers.Add(new AppInInitializer(DeploymentId));
            var tc = new TelemetryConfiguration();
            Telemetry = new TelemetryClient();
            Telemetry.InstrumentationKey = InstrumentationKey;
            Initialized = true;
        }

        public void AddConfiguration(string deploymentId, bool isSilo, string siloName, SiloAddress address, IPEndPoint gateway, string hostName)
        {
            SiloName = siloName;
            DeploymentId = deploymentId;
            HostName = hostName;
            TelemetryConfiguration.Active.ContextInitializers.Add(new AppInInitializer(DeploymentId));
            var tc = new TelemetryConfiguration();
            Telemetry = new TelemetryClient();
            Telemetry.InstrumentationKey = InstrumentationKey;
            Initialized = true;
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
            return Task.CompletedTask;
        }

        public Task Init(ClientConfiguration config, IPAddress address, string clientId)
        {
            // throw new NotImplementedException();
            return Task.CompletedTask;
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
            Telemetry.TrackMetric("CpuUsage", metricsData.CpuUsage);
            Telemetry.TrackMetric("AvailablePhysicalMemory", metricsData.AvailablePhysicalMemory);
            Telemetry.TrackMetric("MemoryUsage", metricsData.MemoryUsage);
            Telemetry.TrackMetric("ReceivedMessages", metricsData.ReceivedMessages);
            Telemetry.TrackMetric("ReceiveQueueLength", metricsData.ReceiveQueueLength);
            Telemetry.TrackMetric("SendQueueLength", metricsData.SendQueueLength);
            Telemetry.TrackMetric("SentMessages", metricsData.SentMessages);
            Telemetry.TrackMetric("TotalPhysicalMemory", metricsData.TotalPhysicalMemory);
            Telemetry.TrackMetric("ConnectedGatewayCount", metricsData.ConnectedGatewayCount);
            return Task.CompletedTask;
        }

        public Task ReportMetrics(ISiloPerformanceMetrics metricsData)
        {
            if (!Initialized) return Task.CompletedTask;
            var pageView = new PageViewTelemetry(SiloName);
            pageView.Properties.Add("deploymentId", DeploymentId);
            Telemetry.TrackMetric("ActivationCount", metricsData.ActivationCount);
            Telemetry.TrackMetric("CpuUsage", metricsData.CpuUsage);
            Telemetry.TrackMetric("AvailablePhysicalMemory", metricsData.AvailablePhysicalMemory);
            Telemetry.TrackMetric("ClientCount", metricsData.ClientCount);
            Telemetry.TrackMetric("MemoryUsage", metricsData.MemoryUsage);
            Telemetry.TrackMetric("ReceivedMessages", metricsData.ReceivedMessages);
            Telemetry.TrackMetric("ReceiveQueueLength", metricsData.ReceiveQueueLength);
            Telemetry.TrackMetric("RecentlyUsedActivationCount", metricsData.RecentlyUsedActivationCount);
            Telemetry.TrackMetric("RequestQueueLength", metricsData.RequestQueueLength);
            Telemetry.TrackMetric("SendQueueLength", metricsData.SendQueueLength);
            Telemetry.TrackMetric("SentMessages", metricsData.SentMessages);
            Telemetry.TrackMetric("TotalPhysicalMemory", metricsData.TotalPhysicalMemory);
            var props = new Dictionary<string, string>();
            props.Add("DeploymentId", DeploymentId);
            props.Add("HostName", HostName);
            Telemetry.TrackEvent("ReportMetrics",props);
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
