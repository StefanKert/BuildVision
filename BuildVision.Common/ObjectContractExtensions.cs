using System.IO;
using System.Reflection;
using System.Runtime.Serialization;

namespace BuildVision.Common
{
    public static class ObjectContractExtensions
    {
        /// <summary>
        /// Clone object using <see cref="DataContractSerializer"/>.
        /// </summary>
        public static T Clone<T>(this T obj)
            where T : class
        {
            var serializer = new DataContractSerializer(typeof(T), null, int.MaxValue, false, true, null);
            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, obj);
                ms.Position = 0;
                return (T)serializer.ReadObject(ms);
            }
        }

        public static void InitFrom<T>(this T target, T source)
            where T : class
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach (var property in properties)
                property.SetValue(target, property.GetValue(source, null), null);
        }

        public static string Serialize<T>(this T obj)
        {
            using (var memoryStream = new MemoryStream())
            using (var reader = new StreamReader(memoryStream))
            {
                var serializer = new DataContractSerializer(typeof(T));
                serializer.WriteObject(memoryStream, obj);
                memoryStream.Position = 0;
                return reader.ReadToEnd();
            }
        }

        public static T Deserialize<T>(this string xml)
        {
            using (var stream = new MemoryStream())
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(xml);
                stream.Write(data, 0, data.Length);
                stream.Position = 0;
                var deserializer = new DataContractSerializer(typeof(T));
                return (T)deserializer.ReadObject(stream);
            }
        }
    }
}