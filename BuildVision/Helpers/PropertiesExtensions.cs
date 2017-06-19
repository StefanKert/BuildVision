using System;

using EnvDTE;

namespace BuildVision.Helpers
{
    public static class PropertiesExtensions
    {

        public static Property GetPropertyOrDefault(this Properties properties, string propertyName)
        {
            try
            {
                return properties.Item(propertyName);
            }
            catch (ArgumentException)
            {
                // not found.
                return null;
            }
        }

        public static T GetPropertyOrDefault<T>(this Properties properties, string propertyName)
            where T : class
        {
            var property = GetPropertyOrDefault(properties, propertyName);
            if (property == null)
                return null;

            return (T)property.Value;
        }

        public static object TryGetPropertyValueOrDefault(this Properties properties, string propertyName)
        {
            try
            {
                return properties.Item(propertyName).Value;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}