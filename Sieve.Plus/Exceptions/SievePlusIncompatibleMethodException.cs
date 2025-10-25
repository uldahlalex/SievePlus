using System;

namespace Sieve.Plus.Exceptions
{
    public class SievePlusIncompatibleMethodException : SievePlusException
    {
        public string MethodName { get; protected set; }
        public Type ExpectedType { get; protected set; }
        public Type ActualType { get; protected set; }

        public SievePlusIncompatibleMethodException(
            string methodName,
            Type expectedType,
            Type actualType,
            string message)
            : base(message)
        {
            MethodName = methodName;
            ExpectedType = expectedType;
            ActualType = actualType;
        }

        public SievePlusIncompatibleMethodException(
            string methodName,
            Type expectedType,
            Type actualType,
            string message,
            Exception innerException)
            : base(message, innerException)
        {
            MethodName = methodName;
            ExpectedType = expectedType;
            ActualType = actualType;
        }

        public SievePlusIncompatibleMethodException(string message) : base(message)
        {
        }

        public SievePlusIncompatibleMethodException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public SievePlusIncompatibleMethodException()
        {
        }

        protected SievePlusIncompatibleMethodException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }
}
