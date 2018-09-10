using System.Reflection;

namespace BuildVision.Common
{
    public abstract class SettingsBase : BindableBase
    {
        public T Clone<T>() where T : SettingsBase, new()
        {
            var serializer = new LegacyConfigurationSerializer<T>();
            return serializer.Deserialize(serializer.Serialize(this as T));
        }

        public void InitFrom<T>(T source) where T : SettingsBase
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach (var property in properties)
                property.SetValue(this, property.GetValue(source, null), null);
        }
    }
}
