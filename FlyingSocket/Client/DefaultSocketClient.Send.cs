using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FlyingSocket.Common;
using FlyingSocket.Utility;

namespace FlyingSocket.Client
{
    public partial class DefaultSocketClient
    {
        /// <summary>
        /// 协议组装器，用来组装往外发送的命令
        /// </summary>
        protected OutgoingDataAssembler OutgoingDataAssembler { get; private set; }
        /// <summary>
        /// 发送数据的缓存，统一写到内存中，调用一次发送
        /// </summary>
        protected DynamicBufferManager SendBuffer { get; private set; }
        //此Socket上的发送事件参数
        protected SocketAsyncEventArgs SendEventArgs { get; private set; }
        /// <summary>
        /// 数据发送重置信号
        /// </summary>
        private AutoResetEvent _sendingReset = new AutoResetEvent(true);

        /// <summary>
        /// 异步发送字符串内容
        /// </summary>
        /// <param name="content"></param>
        public void SendAsync(string content)
        {
            SendAsync(Encoding.UTF8.GetBytes(content));
        }

        /// <summary>
        /// 异步发送指定数据
        /// </summary>
        /// <param name="buffer"></param>
        public void SendAsync(byte[] buffer)
        {
            if (!this.IsConnected) { throw new InvalidCastException("还未建立连接。"); }

            OutgoingDataAssembler.Clear();
            OutgoingDataAssembler.AddRequest();
            OutgoingDataAssembler.BeginRequest(buffer.Length);
            Console.WriteLine("客户端发送消息头");

            WriteOutgoingData(SendBuffer);
            SendBufferData();

            var fileSize = 0;
            using (var stream = new MemoryStream(buffer))
            {
                stream.Position = fileSize;
                var readBuffer = new byte[SocketBufferSize];
                var i = 1;
                while (stream.Position < stream.Length)
                {
                    int count = stream.Read(readBuffer, 0, SocketBufferSize);
                    Console.WriteLine(string.Format("客户端第{0}次发送数据体：{1}", i++, count));
                    this.SendDataCommand(readBuffer, 0, count);
                }
                //发送EOF命令告诉服务端文件上传完成，此时服务端会关闭写入文件流。
                //todo:最好加上文件MD5校验
                Console.WriteLine("客户端发送结束消息。");
                this.EOF(stream.Length);
            }
            Array.Clear(buffer, 0, buffer.Length);
            MemoryUtility.FreeMemory();
        }

        /// <summary>
        /// 结束命令（待添加MD5数据校验）
        /// </summary>
        /// <param name="fileSize"></param>
        protected void EOF(Int64 fileSize)
        {
            OutgoingDataAssembler.Clear();
            OutgoingDataAssembler.AddRequest();
            OutgoingDataAssembler.AddCommand(CommandKeys.EOF);
            WriteOutgoingData(SendBuffer);
            SendBufferData();
        }

        /// <summary>
        /// 发送数据处理命令
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        private void SendDataCommand(byte[] buffer, int offset, int count)
        {
            OutgoingDataAssembler.Clear();
            OutgoingDataAssembler.AddRequest();
            OutgoingDataAssembler.AddCommand(CommandKeys.Data);
            WriteOutgoingData(SendBuffer, count);
            SendBuffer.WriteBuffer(buffer, offset, count); //写入二进制数据
            SendBufferData();
        }

        /// <summary>
        /// 写入待发送的数据到指定缓存
        /// </summary>
        /// <param name="buffer">缓存对象</param>
        /// <param name="additionalDataCount">Data命令中发送实际数据的长度</param>
        private void WriteOutgoingData(DynamicBufferManager buffer, int additionalDataCount = 0)
        {
            if (additionalDataCount < 0) { additionalDataCount = 0; }
            var protocolText = OutgoingDataAssembler.GetProtocolText();
            var commandData = Encoding.UTF8.GetBytes(protocolText);
            //获取总大小(int命令长度+命令内容的总长度+数据长度)，实际发送大小要在此基础加上sizeof(int)
            var totalLength = ProtocolCode.IntegerSize + commandData.Length + additionalDataCount;
            buffer.Clear();
            buffer.WriteInt(totalLength, false);                           //写入总大小    4byte
            buffer.WriteInt(commandData.Length, false);                    //写入命令大小  8byte
            buffer.WriteBuffer(commandData);                               //写入命令内容  8byte + data.Length
        }

        /// <summary>
        /// 将SendBuffer中的缓存数据发送出去
        /// </summary>
        private void SendBufferData()
        {
            var success = _sendingReset.WaitOne(_socketTimeout);
            if (!success)
            {
                Console.WriteLine("发送超时。");
                return;
            }
            this.SendEventArgs.SetBuffer(SendBuffer.Buffer, 0, SendBuffer.DataCount);
            var willRaiseEvent = _clientSocket.SendAsync(this.SendEventArgs);
            //true,异步，在IO_Complete中完成，false，同步完成，不触发IO_Complete
            if (!willRaiseEvent)
            {
                this.SendEventArgs.SetBuffer(null, 0, 0);
                _sendingReset.Set();

                success = _receivingReset.WaitOne(_socketTimeout);
                if (success)
                {
                    this.ReceiveEventArgs.SetBuffer(_receivedData, 0, _receivedData.Length);
                    _clientSocket.ReceiveAsync(this.ReceiveEventArgs);
                }
            }
        }
    }
}
