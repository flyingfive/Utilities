using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlyingSocket.Common;
using FlyingSocket.Utility;

namespace FlyingSocket.Server.Protocol
{
    /// <summary>
    /// 默认数据通讯协议
    /// </summary>
    [ProtocolName(SocketProtocolType.Default)]
    public class DefaultDataProtocol : BaseSocketProtocol
    {
        public DefaultDataProtocol(FlyingSocketServer socketServer, SocketUserToken userToken) : base(socketServer, userToken)
        {
            lock (FlyingSocketServer.DefaultInstances)
            {
                FlyingSocketServer.DefaultInstances.Add(this);
            }
        }
        /// <summary>
        /// 临时数据流
        /// </summary>
        protected Stream InputStream { get; private set; }

        public override bool ProcessCommand(byte[] buffer, int offset, int count)
        {
            if (string.Equals(IncomingDataParser.Command, CommandKeys.Begin))
            {
                return ProcessBeginRequest();
            }
            if (string.Equals(IncomingDataParser.Command, CommandKeys.EOF))
            {
                return ProcessEndRequest();
            }
            Console.WriteLine(string.Format("服务端写入数据长度：{0}", count));
            this.InputStream.Write(buffer, offset, count);
            //Array.Clear(buffer, 0, buffer.Length);
            OutgoingDataAssembler.AddSuccess();
            OutgoingDataAssembler.AddValue(CommandKeys.FileSize, InputStream.Length);
            return SendResult();
            //return true;
            //var data = new byte[count];
            //Array.Copy(buffer, offset, data, 0, count);
            //base.FlyingSocketServer.ClientMessageReceived(data);
            //return true;
        }

        protected bool ProcessEndRequest()
        {
            Console.WriteLine(string.Format("服务端结束写入。"));
            this.InputStream.Position = 0;
            var length = 0L;
            using (InputStream)
            using (var reader = new StreamReader(InputStream, Encoding.UTF8))
            {
                var content = reader.ReadToEnd();
                //Console.WriteLine("服务端接收到的数据：" + content);
                var file = Path.Combine(FileWorkingDirectory, string.Format("{0}.txt", Guid.NewGuid().ToString()));
                System.IO.File.WriteAllText(file, content, Encoding.UTF8);
                length = InputStream.Length;
            }
            InputStream = null;

            //return true;
            OutgoingDataAssembler.AddSuccess();
            OutgoingDataAssembler.AddValue(CommandKeys.FileSize, length);
            return SendResult();
        }

        protected bool ProcessBeginRequest()
        {
            Console.WriteLine(string.Format("服务端开始写入数据。"));
            var bodyLength = 0;
            var success = IncomingDataParser.GetValue(CommandKeys.DataLength, ref bodyLength);
            if (!success || bodyLength <= 0) { return false; }
            InputStream = new MemoryStream();
            OutgoingDataAssembler.AddSuccess();
            OutgoingDataAssembler.AddValue(CommandKeys.FileSize, InputStream.Length);
            return SendResult();
        }

        public override void Close()
        {
            if (InputStream != null)
            {
                InputStream.Close();
                InputStream = null;
            }
            lock (FlyingSocketServer.UploadInstances)
            {
                FlyingSocketServer.DefaultInstances.Remove(this);
            }
            base.Close();
            MemoryUtility.FreeMemory();
        }

    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ProtocolNameAttribute : System.Attribute
    {
        public SocketProtocolType ProtocolType { get; private set; }

        public ProtocolNameAttribute(SocketProtocolType type) { this.ProtocolType = type; }

    }
}
