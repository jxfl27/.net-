using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Server
{
    /// <summary>
    /// 将对象序列化和反序列化，以便在网络上传输
    /// </summary>
    class XmlUtils
    {
        //反序列化
        public static T DeserializeObject<T> (string xml)
        {
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));
                StringReader sr = new StringReader(xml);
                T obj = (T)xs.Deserialize(sr);
                sr.Close();
                sr.Dispose();
                return obj;
            }
            catch (Exception ex)
            {
                Console.WriteLine("反序列化出错"+ex.Message);
                throw new Exception("不支持的数据格式。"+ex.Message);
            }
        }

        //序列化
        public static string XmlSerializer<T>(T t)
        {
            XmlSerializerNamespaces xsn = new XmlSerializerNamespaces();
            xsn.Add(string.Empty, string.Empty);
            XmlSerializer xs = new XmlSerializer(typeof(T));
            StringWriter sw = new StringWriter();
            xs.Serialize(sw, t, xsn);
            string str = sw.ToString();
            sw.Close();
            sw.Dispose();
            return str;
        }
    }
}
