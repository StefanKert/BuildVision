using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace BuildVision.Common
{
    public class LegacyConfigurationSerializer<T> : GenericXmlSerializer<T> where T : SettingsBase, new()
    {
        public override T Deserialize(string xml)
        {
            using (var ms = new MemoryStream())
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(xml);
                ms.Write(data, 0, data.Length);
                ms.Position = 0;
                using (var xmlTextReader = new XmlTextReader(ms) { Namespaces = false })
                {
                    var serializer = new XmlSerializer(typeof(T));
                    return serializer.Deserialize(xmlTextReader) as T;
                }
            }
        }
    }
}
