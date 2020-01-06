using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace FlyingFive.Utils
{
    /// <summary>
    /// 序列化工具
    /// </summary>
    public partial class SerializationHelper
    {
        #region Xml Serialization

        /// <summary>
        /// 序列化对象为xml字符串
        /// </summary>
        /// <param name="obj">要序列化的对象</param>
        /// <returns></returns>
        public static string SerializeObjectToXml(object obj)
        {
            var xml = new System.Xml.Serialization.XmlSerializer(obj.GetType(), "");
            using (var ms = new MemoryStream())
            {
                System.Xml.Serialization.XmlSerializerNamespaces xns = new System.Xml.Serialization.XmlSerializerNamespaces();
                xns.Add("", "");
                xml.Serialize(ms, obj, xns);
                string xmlString = Encoding.UTF8.GetString(ms.ToArray());
                return xmlString;
            }
        }

        /// <summary>
        /// 从Xml字符串还原对象
        /// </summary>
        /// <param name="xmlString">对象的Xml格式字符串</param>
        /// <returns></returns>
        public static T DeserializeObjectFromXml<T>(string xmlString) where T : class
        {
            T obj = default(T);
            obj = DeserializeObjectFromXml(xmlString, typeof(T)) as T;
            return obj;
        }

        /// <summary>
        /// 从Xml字符串还原对象
        /// </summary>
        /// <param name="xmlString">对象的Xml格式字符串</param>
        /// <param name="type">对象类型</param>
        /// <returns></returns>
        public static object DeserializeObjectFromXml(string xmlString, Type type)
        {
            var xml = new System.Xml.Serialization.XmlSerializer(type);
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(xmlString)))
            {
                ms.Position = 0;
                var obj = xml.Deserialize(ms);
                return obj;
            }
        }

        #endregion

        #region Base64 Serialization

        /// <summary>
        /// 对象序列化为Base64字符串
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public static string SerializeObjectToBase64(object obj)
        {
            string base64Str = "";
            IFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, obj);
                stream.Position = 0;
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                stream.Flush();
                stream.Close();
                base64Str = Convert.ToBase64String(buffer);
            }
            return base64Str;
        }

        /// <summary>
        /// Base64字符串反序列化为对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="base64Str">Base64字符串</param>
        /// <returns></returns>
        public static T DeserializeObjectFromBase64<T>(string base64Str) where T : class
        {
            T obj = default(T);
            IFormatter formatter = new BinaryFormatter();
            byte[] buffer = Convert.FromBase64String(base64Str);
            using (MemoryStream stream = new MemoryStream(buffer))
            {
                obj = formatter.Deserialize(stream) as T;
                stream.Flush();
                stream.Close();
            }
            return obj;
        }

        #endregion

        #region Binary Serialization

        /// <summary>
        /// 对象序列化成二进制数组
        /// </summary>
        /// <param name="obj">要序列化对象</param>
        /// <returns></returns>
        public static byte[] SerializeObjectToBytes(object obj)
        {
            using (var ms = new MemoryStream())
            {
                var b = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                b.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// 从二进制数组还原对象
        /// </summary>
        /// <param name="data">对象的二进制格式数据</param>
        /// <returns></returns>
        public static T DeserializeObjectFromBytes<T>(byte[] data) where T : class
        {
            T obj = default(T);
            using (var ms = new MemoryStream(data))
            {
                var b = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                obj = b.Deserialize(ms) as T;
            }
            return obj;
        }

        #endregion
    }
}
