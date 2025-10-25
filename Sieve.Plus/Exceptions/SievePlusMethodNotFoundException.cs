using System;

namespace Sieve.Plus.Exceptions
{
    public class SievePlusMethodNotFoundException : SievePlusException
    {
        public string MethodName { get; protected set; }

        public SievePlusMethodNotFoundException(string methodName, string message) : base(message)
        {
            MethodName = methodName;
        }

        public SievePlusMethodNotFoundException(string methodName, string message, Exception innerException) : base(message, innerException)
        {
            MethodName = methodName;
        }

        public SievePlusMethodNotFoundException(string message) : base(message)
        {
        }

        public SievePlusMethodNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public SievePlusMethodNotFoundException()
        {
        }

        protected SievePlusMethodNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }
}
