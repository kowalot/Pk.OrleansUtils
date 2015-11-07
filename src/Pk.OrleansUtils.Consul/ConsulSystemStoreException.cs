using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Pk.OrleansUtils.Consul
{
    public class ConsulSystemStoreException
    {
        [Serializable]
        internal class ConsulServiceRegistrationFailed : Exception
        {
            public ConsulServiceRegistrationFailed()
            {
            }

            public ConsulServiceRegistrationFailed(string message) : base(message)
            {
            }

            public ConsulServiceRegistrationFailed(string message, Exception innerException) : base(message, innerException)
            {
            }

            protected ConsulServiceRegistrationFailed(SerializationInfo info, StreamingContext context) : base(info, context)
            {
            }
        }

        [Serializable]
        internal class DuplicatedEntry : Exception
        {
            public DuplicatedEntry()
            {
            }

            public DuplicatedEntry(string message) : base(message)
            {
            }

            public DuplicatedEntry(string message, Exception innerException) : base(message, innerException)
            {
            }

            protected DuplicatedEntry(SerializationInfo info, StreamingContext context) : base(info, context)
            {
            }
        }

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

        [Serializable]
        internal class UpdateRowFailedNoEntry : Exception
        {
            public UpdateRowFailedNoEntry()
            {
            }

            public UpdateRowFailedNoEntry(string message) : base(message)
            {
            }

            public UpdateRowFailedNoEntry(string message, Exception innerException) : base(message, innerException)
            {
            }

            protected UpdateRowFailedNoEntry(SerializationInfo info, StreamingContext context) : base(info, context)
            {
            }
        }

        [Serializable]
        internal class WrongVersionException : Exception
        {
            public WrongVersionException()
            {
            }

            public WrongVersionException(string message) : base(message)
            {
            }

            public WrongVersionException(string message, Exception innerException) : base(message, innerException)
            {
            }

            protected WrongVersionException(SerializationInfo info, StreamingContext context) : base(info, context)
            {
            }
        }
    }
}
