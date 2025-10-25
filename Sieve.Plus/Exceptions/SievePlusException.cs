using System;

namespace Sieve.Plus.Exceptions
{
    public class SievePlusException : Exception
    {
        public SievePlusException(string message) : base(message)
        {
        }

        public SievePlusException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public SievePlusException()
        {
        }

        protected SievePlusException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }
}
