using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace BuildVision.Common
{
    public class GenericXmlSerializer<T> where T : class, new()
    {
        public virtual T Deserialize(string xml)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (TextReader reader = new StringReader(xml))
            {
                return serializer.Deserialize(reader) as T;
            }
        }

        public virtual string Serialize(T settings)
        {
            using (var ms = new MemoryStream())
            using (var tw = new XmlTextWriter(ms, null))
            {
                new XmlSerializer(typeof(T)).Serialize(tw, settings);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }
    }
}
