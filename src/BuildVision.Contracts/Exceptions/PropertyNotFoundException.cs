using System;

namespace BuildVision.Contracts.Exceptions
{
    public class PropertyNotFoundException : Exception
    {
        public PropertyNotFoundException(string propertyName, Type type)
        {
            PropertyName = propertyName;
            Type = type;
        }

        public string PropertyName { get; }

        public Type Type { get; }

        public override string Message => string.Format("Property '{0}' not found in '{1}' type.", PropertyName, Type);
    }
}
