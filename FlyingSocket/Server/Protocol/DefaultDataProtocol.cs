using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlyingSocket.Core;

namespace FlyingSocket.Server.Protocol
{
    /// <summary>
    /// 默认数据通讯协议
    /// </summary>
    [ProtocolName(FlyingProtocolType.Default)]
    public class DefaultDataProtocol : BaseSocketProtocol
    {
        public DefaultDataProtocol(FlyingSocketServer socketServer, SocketUserToken userToken) : base("Default", socketServer, userToken) { }

        protected Stream InputStream { get; private set; }

        public override bool ProcessCommand(byte[] buffer, int offset, int count)
        {
            Console.WriteLine(string.Format("服务端写入数据长度：{0}", count));
            this.InputStream.Write(buffer, offset, count);
            OutgoingDataAssembler.AddSuccess();
            OutgoingDataAssembler.AddValue(ProtocolKey.FileSize, InputStream.Length);
            return SendResult();
            //return true;
            //var data = new byte[count];
            //Array.Copy(buffer, offset, data, 0, count);
            //base.FlyingSocketServer.ClientMessageReceived(data);
            //return true;
        }

        protected override bool ProcessEndRequest()
        {
            Console.WriteLine(string.Format("服务端结束写入。"));
            this.InputStream.Position = 0;
            using (InputStream)
            using (var reader = new StreamReader(InputStream, Encoding.UTF8))
            {
                var content = reader.ReadToEnd();
                Console.WriteLine(content);
            }
            InputStream = null;
            //return true;
            OutgoingDataAssembler.AddSuccess();
            OutgoingDataAssembler.AddValue(ProtocolKey.FileSize, InputStream.Length);
            return SendResult();
        }

        protected override bool ProcessBeginRequest()
        {
            Console.WriteLine(string.Format("服务端开始写入数据。"));
            var bodyLength = 0;
            var success = IncomingDataParser.GetValue(ProtocolKey.DataLengthKey, ref bodyLength);
            if (!success || bodyLength <= 0) { return false; }
            InputStream = new MemoryStream();
            OutgoingDataAssembler.AddSuccess();
            OutgoingDataAssembler.AddValue(ProtocolKey.FileSize, InputStream.Length);
            return SendResult();
        }

    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ProtocolNameAttribute : System.Attribute
    {
        public FlyingProtocolType ProtocolType { get; private set; }

        public ProtocolNameAttribute(FlyingProtocolType type) { this.ProtocolType = type; }

    }
}
