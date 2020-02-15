using System;

namespace BuildVision.Contracts.Exceptions
{
    [Serializable]
    public class PropertyNotFoundException : Exception
    {
        public string PropertyName { get; }

        public Type PropertyType { get; }

        public override string Message => string.Format("Property '{0}' not found in '{1}' type.", PropertyName, PropertyType);

        public PropertyNotFoundException(string propertyName, Type type)
        {
            PropertyName = propertyName;
            PropertyType = type;
        }
    }
}
