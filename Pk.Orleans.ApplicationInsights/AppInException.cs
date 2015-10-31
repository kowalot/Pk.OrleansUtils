using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Pk.Orleans.ApplicationInsights
{
    public class AppInException
    {
        [Serializable]
        internal class InvalidConfiguration : Exception
        {
            public InvalidConfiguration()
            {
            }

            public InvalidConfiguration(string message) : base(message)
            {
            }

            public InvalidConfiguration(string message, Exception innerException) : base(message, innerException)
            {
            }

            protected InvalidConfiguration(SerializationInfo info, StreamingContext context) : base(info, context)
            {
            }
        }
    }
}
