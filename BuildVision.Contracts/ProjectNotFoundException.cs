using System;
using System.Runtime.Serialization;

namespace BuildVision.Contracts
{
    [Serializable]
    public class ProjectNotFoundException : Exception
    {
        public ProjectNotFoundException()
        {
        }

        public ProjectNotFoundException(string message) : base(message)
        {
        }

        public ProjectNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ProjectNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}