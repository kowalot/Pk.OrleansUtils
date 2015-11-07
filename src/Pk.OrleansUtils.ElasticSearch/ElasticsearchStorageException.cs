using System;
using System.Runtime.Serialization;

namespace Pk.OrleansUtils.ElasticSearch
{
    [Serializable]
    internal class ElasticsearchStorageException : Exception
    {
        public ElasticsearchStorageException()
        {
        }

        public ElasticsearchStorageException(string message) : base(message)
        {
        }

        public ElasticsearchStorageException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ElasticsearchStorageException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}