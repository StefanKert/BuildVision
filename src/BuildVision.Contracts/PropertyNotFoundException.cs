using System;

namespace BuildVision.Contracts
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

        public override string Message
        {
            get { return string.Format("Property '{0}' not found in '{1}' type.", PropertyName, Type); }
        }
    }
}
