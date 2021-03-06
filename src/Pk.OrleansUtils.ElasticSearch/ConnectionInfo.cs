﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nest;

namespace Pk.OrleansUtils.ElasticSearch
{
    public class ConnectionInfo
    {
        public string Index { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public ConnectionInfo()
        {
            Host = "localhost";
            Port = 9200;
        }

        public bool IsValid()
        {
            return (!String.IsNullOrEmpty(Index) && !String.IsNullOrEmpty(Host) && Port > 0);
        }

        internal ConnectionSettings GetConnectionSettings()
        {
            var cs = new ConnectionSettings(new UriBuilder("http", Host, Port, "/").Uri, Index);
#if DEBUG
            cs.EnableTrace(true);
#endif
            return cs;

        }


    }
}