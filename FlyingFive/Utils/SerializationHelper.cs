using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Xml.Schema;

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
        /// <param name="omitXmlDeclaration">是否忽略xml声明</param>
        /// <param name="omitDefaultNameapce">是否忽略默认的xml命名空间</param>
        /// <returns></returns>
        public static string SerializeObjectToXml(object obj, bool omitXmlDeclaration = false, bool omitDefaultNameapce = true)
        {
            if (obj == null) { throw new ArgumentNullException("参数obj不能为null"); }
            var xml = new System.Xml.Serialization.XmlSerializer(obj.GetType(), "");
            var setting = new XmlWriterSettings() { CheckCharacters = false, Encoding = Encoding.UTF8, OmitXmlDeclaration = omitXmlDeclaration, Indent = true, NamespaceHandling = NamespaceHandling.Default, NewLineChars = Environment.NewLine, IndentChars = "\t" };
            using (var ms = new MemoryStream())
            using (var writer = XmlWriter.Create(ms, setting))
            {
                System.Xml.Serialization.XmlSerializerNamespaces xns = new System.Xml.Serialization.XmlSerializerNamespaces();
                if (omitDefaultNameapce) { xns.Add("", ""); }
                xml.Serialize(writer, obj, xns);
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
        /// <param name="xmlString">Xml格式字符串</param>
        /// <param name="type">对象类型</param>
        /// <returns></returns>
        public static object DeserializeObjectFromXml(string xmlString, Type type)
        {
            if (string.IsNullOrWhiteSpace(xmlString)) { throw new ArgumentException("xmlString不能为空"); }
            if (type == null) { throw new ArgumentNullException("type"); }
            var settings = new XmlReaderSettings() { IgnoreComments = true, CheckCharacters = false, CloseInput = true, IgnoreWhitespace = true };
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlString)))
            using (var reader = XmlReader.Create(stream, settings))
            {
                var xml = new System.Xml.Serialization.XmlSerializer(type);
                var obj = xml.Deserialize(reader);
                return obj;
            }
        }

        /// <summary>
        /// 从字节序列流反序列化还原为对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="stream">字节序列流</param>
        /// <param name="closeInput">还原后是否立即关闭流</param>
        /// <returns></returns>
        public static T DeserializeObjectFromXml<T>(Stream stream, bool closeInput = true) where T : class
        {
            T obj = default(T);
            obj = DeserializeObjectFromXml(stream, typeof(T), closeInput) as T;
            return obj;
        }


        /// <summary>
        /// 从字节序列流反序列化还原为对象
        /// </summary>
        /// <param name="stream">字节序列流</param>
        /// <param name="type">对象类型</param>
        /// <param name="closeInput">还原后是否立即关闭流</param>
        /// <returns></returns>
        public static object DeserializeObjectFromXml(Stream stream, Type type, bool closeInput = true)
        {
            if (stream == null || stream.Length < 1)
            {
                throw new ArgumentException("stream参数不能为null或长度小于1");
            }
            if (type == null) { throw new ArgumentNullException("type"); }
            stream.Seek(0, SeekOrigin.Begin);
            var settings = new XmlReaderSettings() { IgnoreComments = true, CheckCharacters = false, CloseInput = closeInput, IgnoreWhitespace = true };
            using (var reader = XmlReader.Create(stream, settings))
            {
                var xml = new System.Xml.Serialization.XmlSerializer(type);
                var obj = xml.Deserialize(reader);
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
            var buffer = SerializeObjectToBytes(obj);
            var base64Str = Convert.ToBase64String(buffer);
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
            var formatter = new BinaryFormatter();
            var buffer = Convert.FromBase64String(base64Str);
            var obj = DeserializeObjectFromBytes<T>(buffer);
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
            using (var stream = new MemoryStream())
            {
                var b = new BinaryFormatter();
                b.Serialize(stream, obj);
                return stream.ToArray();
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
            using (var stream = new MemoryStream(data))
            {
                var b = new BinaryFormatter();
                obj = b.Deserialize(stream) as T;
            }
            return obj;
        }

        #endregion
    }
}
