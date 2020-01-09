using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using FlyingSocket.Core;

namespace FlyingSocket.Server
{
    /// <summary>
    /// 异步Socket调用节点
    /// </summary>
    public abstract class SocketInvokeElement
    {
        /// <summary>
        /// 当前Socket调用节点位于的Socket服务对象
        /// </summary>
        protected FlyingSocketServer FlyingSocketServer { get; private set; }
        /// <summary>
        /// 当前Socket调用节点的用户对象
        /// </summary>
        protected SocketUserToken SocketUserToken { get; private set; }

        /// <summary>
        /// 长度是否使用网络字节顺序
        /// </summary>
        public bool NetByteOrder { get; set; }
        /// <summary>
        /// 协议解析器，用来解析客户端接收到的命令
        /// </summary>
        protected IncomingDataParser IncomingDataParser { get; private set; }
        /// <summary>
        /// 协议组装器，用来组织服务端返回的命令
        /// </summary>
        protected OutgoingDataAssembler OutgoingDataAssembler { get; private set; }
        /// <summary>
        /// 标识是否有发送异步事件
        /// </summary>
        protected bool IsAsyncSending { get; private set; }

        public DateTime ConnectTime { get; protected set; }
        public DateTime ActiveTime { get; protected set; }

        public SocketInvokeElement(FlyingSocketServer socketServer, SocketUserToken userToken)
        {
            FlyingSocketServer = socketServer;
            SocketUserToken = userToken;
            NetByteOrder = false;
            IsAsyncSending = false;
            IncomingDataParser = new IncomingDataParser();
            OutgoingDataAssembler = new OutgoingDataAssembler();
            ConnectTime = DateTime.UtcNow;
            ActiveTime = DateTime.UtcNow;
        }

        public virtual void Close()
        {
        }

        /// <summary>
        /// 接收异步事件返回的数据，用于对数据进行缓存和分包
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public virtual bool ProcessReceive(byte[] buffer, int offset, int count)
        {
            ActiveTime = DateTime.UtcNow;
            var receiveBuffer = SocketUserToken.ReceiveBuffer;

            receiveBuffer.WriteBuffer(buffer, offset, count);
            var result = true;
            while (receiveBuffer.DataCount > sizeof(int))
            {
                //按照长度分包
                var packetLength = BitConverter.ToInt32(receiveBuffer.Buffer, 0); //获取包长度
                if (NetByteOrder)
                {
                    packetLength = System.Net.IPAddress.NetworkToHostOrder(packetLength); //把网络字节顺序转为本地字节顺序
                }

                if ((packetLength > 10 * 1024 * 1024) | (receiveBuffer.DataCount > 10 * 1024 * 1024)) //最大Buffer异常保护
                {
                    return false;
                }
                if ((receiveBuffer.DataCount - sizeof(int)) >= packetLength) //收到的数据达到包长度
                {
                    result = ProcessPacket(receiveBuffer.Buffer, sizeof(int), packetLength);
                    if (result)
                        receiveBuffer.Clear(packetLength + sizeof(int)); //从缓存中清理
                    else
                        return result;
                }
                else
                {
                    return true;
                }
            }
            return true;
        }

        /// <summary>
        /// 处理分完包后的数据，把命令和数据分开，并对命令进行解析
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public virtual bool ProcessPacket(byte[] buffer, int offset, int count)
        {
            if (count < sizeof(int))
            {
                return false;
            }
            int commandLen = BitConverter.ToInt32(buffer, offset); //取出命令长度
            string tmpStr = Encoding.UTF8.GetString(buffer, offset + sizeof(int), commandLen);
            if (!IncomingDataParser.DecodeProtocolText(tmpStr)) //解析命令
                return false;

            return ProcessCommand(buffer, offset + sizeof(int) + commandLen, count - sizeof(int) - commandLen); //处理命令
        }

        /// <summary>
        /// 处理具体命令，子类从这个方法继承，buffer是收到的数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public virtual bool ProcessCommand(byte[] buffer, int offset, int count)
        {
            return true;
        }

        public virtual bool SendCompleted()
        {
            ActiveTime = DateTime.UtcNow;
            IsAsyncSending = false;
            var sendBufferManager = SocketUserToken.SendBuffer;
            sendBufferManager.ClearFirstPacket(); //
            int offset = 0;
            int count = 0;
            if (sendBufferManager.GetFirstPacket(ref offset, ref count))
            {
                IsAsyncSending = true;
                var success = FlyingSocketServer.SendAsyncEvent(SocketUserToken.ConnectSocket, SocketUserToken.SendEventArgs, sendBufferManager.DynamicBufferManager.Buffer, offset, count);
                return success;
            }
            else
            {
                return SendCallback();
            }
        }

        /// <summary>
        /// 发送回调函数，用于连续下发数据
        /// </summary>
        /// <returns></returns>
        public virtual bool SendCallback()
        {
            return true;
        }

        public bool SendResult()
        {
            var commandText = OutgoingDataAssembler.GetProtocolText();
            var buffer = Encoding.UTF8.GetBytes(commandText);
            int totalLength = sizeof(int) + buffer.Length; //获取总大小
            var sendBufferManager = SocketUserToken.SendBuffer;
            sendBufferManager.StartPacket();
            sendBufferManager.DynamicBufferManager.WriteInt(totalLength, false); //写入总大小
            sendBufferManager.DynamicBufferManager.WriteInt(buffer.Length, false); //写入命令大小
            sendBufferManager.DynamicBufferManager.WriteBuffer(buffer); //写入命令内容
            sendBufferManager.EndPacket();

            bool result = true;
            if (!IsAsyncSending)
            {
                int packetOffset = 0;
                int packetCount = 0;
                if (sendBufferManager.GetFirstPacket(ref packetOffset, ref packetCount))
                {
                    IsAsyncSending = true;
                    result = FlyingSocketServer.SendAsyncEvent(SocketUserToken.ConnectSocket, SocketUserToken.SendEventArgs, sendBufferManager.DynamicBufferManager.Buffer, packetOffset, packetCount);
                }
            }
            return result;
        }

        public bool SendResult(byte[] buffer, int offset, int count)
        {
            string commandText = OutgoingDataAssembler.GetProtocolText();
            byte[] bufferUTF8 = Encoding.UTF8.GetBytes(commandText);
            int totalLength = sizeof(int) + bufferUTF8.Length + count; //获取总大小
            var sendBufferManager = SocketUserToken.SendBuffer;
            sendBufferManager.StartPacket();
            sendBufferManager.DynamicBufferManager.WriteInt(totalLength, false); //写入总大小
            sendBufferManager.DynamicBufferManager.WriteInt(bufferUTF8.Length, false); //写入命令大小
            sendBufferManager.DynamicBufferManager.WriteBuffer(bufferUTF8); //写入命令内容
            sendBufferManager.DynamicBufferManager.WriteBuffer(buffer, offset, count); //写入二进制数据
            sendBufferManager.EndPacket();

            bool result = true;
            if (!IsAsyncSending)
            {
                int packetOffset = 0;
                int packetCount = 0;
                if (sendBufferManager.GetFirstPacket(ref packetOffset, ref packetCount))
                {
                    IsAsyncSending = true;
                    result = FlyingSocketServer.SendAsyncEvent(SocketUserToken.ConnectSocket, SocketUserToken.SendEventArgs, sendBufferManager.DynamicBufferManager.Buffer, packetOffset, packetCount);
                }
            }
            return result;
        }

        /// <summary>
        /// 不是按包格式下发一个内存块，用于日志这类下发协议
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public bool SendBuffer(byte[] buffer, int offset, int count)
        {
            var sendBufferManager = SocketUserToken.SendBuffer;
            sendBufferManager.StartPacket();
            sendBufferManager.DynamicBufferManager.WriteBuffer(buffer, offset, count);
            sendBufferManager.EndPacket();

            bool result = true;
            if (!IsAsyncSending)
            {
                int packetOffset = 0;
                int packetCount = 0;
                if (sendBufferManager.GetFirstPacket(ref packetOffset, ref packetCount))
                {
                    IsAsyncSending = true;
                    result = FlyingSocketServer.SendAsyncEvent(SocketUserToken.ConnectSocket, SocketUserToken.SendEventArgs, sendBufferManager.DynamicBufferManager.Buffer, packetOffset, packetCount);
                }
            }
            return result;
        }
    }
}