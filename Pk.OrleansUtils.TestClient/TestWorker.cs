using Orleans;
using Orleans.Runtime.Configuration;
using Pk.OrleansUtils.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pk.OrleansUtils.TestClient
{
    public class TestWorker
    {
        private string ConfigurationPath;
        public Thread   WorkerThread { get; set; }

        public TestWorker(int id,string v)
        {
            VersionString = "0.0.0.0";
            Id = id;
            this.ConfigurationPath = v;
            this.Active = false;
            WorkerThread = new Thread(ThreadLoop);
            WorkerThread.Start();
        }

        public bool Initialized { get; private set; }

        public bool Active { get; set; }

        private bool _closing = false;

        public int Id { get; set; }

        public int CallsCount { get; set; }

        public string VersionString { get; private set; }
        public long IterationTime { get; private set; }

        public async void ThreadLoop()
        {
            var random = new Random();
            while (true)// entry loop or after exception
            {
                var sw = new Stopwatch();
                try
                {
                    while (!_closing)
                    {
                        while (Active)
                        {
                            sw.Restart();
                            if (!Initialized)
                            {
                                var config = ClientConfiguration.LoadFromFile(ConfigurationPath);
                                config.GatewayListRefreshPeriod = TimeSpan.FromSeconds(2);
                                config.ResendOnTimeout = true;
                                config.ResponseTimeout = TimeSpan.FromSeconds(10);
                                config.MaxSocketAge = TimeSpan.FromSeconds(2);
                                config.MaxResendCount = 3;
                               // config.MaxForwardCount = 3;
                                GrainClient.Initialize(config);
                            }
                            var targetId = (Id+1) * 100000 + CallsCount;
                            var account = GrainClient.GrainFactory.GetGrain<IAccount>(Id);
                            var targetAccount = GrainClient.GrainFactory.GetGrain<IAccount>(targetId);
                            VersionString = await account.GetVersion();
                            var res = await account.TransferMoney(targetAccount,65.45);
                            CallsCount++;
                            sw.Stop();
                            IterationTime = sw.ElapsedMilliseconds;
                        }
                        Thread.Sleep(100);
                    }
                }
                catch (AggregateException ax)
                {
                    VersionString = "FAIL:" + ax.InnerExceptions.First().Message;
                    sw.Stop();
                    IterationTime = sw.ElapsedMilliseconds;
                    Thread.Sleep(5000);
                }
                catch (Exception ex)
                {
                    VersionString = "FAIL:" + ex.Message;
                    sw.Stop();
                    IterationTime = sw.ElapsedMilliseconds;
                    Thread.Sleep(5000);
                }
            }
        }
    }
}
