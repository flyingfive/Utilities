using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FlyingSocket.Common;

namespace FlyingSocket.Client
{
    public partial class DefaultSocketClient
    {
        /// <summary>
        /// 收到数据的解析器，用于解析返回的内容
        /// </summary>
        protected IncomingDataParser IncomingDataParser { get; private set; }
        /// <summary>
        /// 接收数据的缓存(还没有像服务端一样对大数据做粘包处理)
        /// </summary>
        protected DynamicBufferManager ReceiveBuffer { get; private set; }
        /// <summary>
        /// 此Socket上的接收事件参数
        /// </summary>
        protected SocketAsyncEventArgs ReceiveEventArgs { get; private set; }
        /// <summary>
        /// 临时接收数据
        /// </summary>
        private byte[] _receivedData = null;
        /// <summary>
        /// 数据接收重置信号
        /// </summary>
        private AutoResetEvent _receivingReset = new AutoResetEvent(true);

        private void AsyncReceiveData(SocketAsyncEventArgs e)
        {
            _receivingReset.Set();
            var receiveCount = e.BytesTransferred;
            if (receiveCount == 0)            //服务端请求关闭连接
            {
                _clientSocket.DisconnectAsync(DisconnectEventArgs);
            }
            else
            {
                var success = ProcesReceive(e);
                if (!success)       //如果处理数据返回失败，则断开连接
                {
                    //CloseClientConnection(userToken);
                }
                else
                {
                    //否则投递下次接受数据请求
                    //var willRaiseEvent = userToken.ConnectSocket.ReceiveAsync(userToken.ReceiveEventArgs); //投递接收请求

                    this.ReceiveEventArgs.SetBuffer(_receivedData, 0, _receivedData.Length);
                    var willRaiseEvent = _clientSocket.ReceiveAsync(this.ReceiveEventArgs);
                    if (!willRaiseEvent)
                    {
                        AsyncReceiveData(ReceiveEventArgs);
                    }
                }
            }
        }

        private bool ProcesReceive(SocketAsyncEventArgs e)
        {
            var offset = e.Offset;
            //会将服务端的多次数据响应全部集中一起返回。这里应解析字节大小外再重新调用ReceiveAsync方法循环处理。
            Console.WriteLine("客户端数据接收完成" + e.BytesTransferred);
            //var text = Encoding.UTF8.GetString(e.Buffer, 8, e.BytesTransferred - 8);
            //Debug.WriteLine(text);
            var receiveCount = e.BytesTransferred;
            ReceiveBuffer.WriteBuffer(e.Buffer, offset, receiveCount);
            var result = true;
            while (ReceiveBuffer.DataCount > ProtocolCode.IntegerSize)
            {
                //按照长度分包
                var packetLength = BitConverter.ToInt32(ReceiveBuffer.Buffer, 0); //获取包长度
                if (NetByteOrder)
                {
                    packetLength = System.Net.IPAddress.NetworkToHostOrder(packetLength); //把网络字节顺序转为本地字节顺序
                }

                if ((packetLength > 10 * 1024 * 1024) | (ReceiveBuffer.DataCount > 10 * 1024 * 1024)) //最大Buffer异常保护
                {
                    return false;
                }
                if ((ReceiveBuffer.DataCount - ProtocolCode.IntegerSize) >= packetLength) //收到的数据达到包长度
                {
                    result = ProcessPacket(ReceiveBuffer.Buffer, ProtocolCode.IntegerSize, packetLength);
                    if (result)
                    {
                        ReceiveBuffer.Clear(packetLength + ProtocolCode.IntegerSize); //从缓存中清理
                    }
                    else
                    {
                        return result;
                    }
                }
                else
                {
                    return true;
                }
            }
            return true;
        }

        protected virtual bool ProcessPacket(byte[] buffer, int offset, int count)
        {
            if (count < ProtocolCode.IntegerSize)
            {
                return false;
            }
            var commandLength = BitConverter.ToInt32(buffer, offset); //取出命令长度
            var protocolText = Encoding.UTF8.GetString(buffer, offset + ProtocolCode.IntegerSize, commandLength);
            if (!IncomingDataParser.DecodeProtocolText(protocolText)) //解析命令
            {
                return false;
            }
            if (string.Equals(IncomingDataParser.Command, CommandKeys.Login))
            {
                CheckLoginResult();//(IncomingDataParser);
                return true;
            }
            //var code = -1;
            //if (!IncomingDataParser.GetValue(CommandKeys.Code, ref code))
            //{
            //    return false;
            //}
            //if (code != 0)
            //{
            //    return false;
            //}
            //var sessionId = string.Empty;
            //var token = string.Empty;
            //if (!IncomingDataParser.GetValue(CommandKeys.SessionID, ref sessionId))
            //{
            //    return false;
            //}
            //if (!IncomingDataParser.GetValue(CommandKeys.Token, ref token))
            //{
            //    return false;
            //}
            //this.SessionID = sessionId;
            //this.Token = token;
            //实际数据体在buffer中的起止位置
            var dataOffset = offset + ProtocolCode.IntegerSize + commandLength;
            var dataLength = count - ProtocolCode.IntegerSize - commandLength;
            if (dataLength <= 0) { return true; }                   //数据已处理完。
            return ProcessCommand(buffer, dataOffset, dataLength);  //处理其它命令
        }

        public event EventHandler<EventArgs> Logined;
        private void CheckLoginResult()
        {
            var code = -1;
            //登录失败。
            if (!IncomingDataParser.GetValue(CommandKeys.Code, ref code))
            {
                return;
            }
            if (code != 0)
            {
                return;
            }
            var sessionId = string.Empty;
            var token = string.Empty;
            if (!IncomingDataParser.GetValue(CommandKeys.SessionID, ref sessionId))
            {
                return;
            }
            if (!IncomingDataParser.GetValue(CommandKeys.Token, ref token))
            {
                return;
            }
            this.SessionID = sessionId;
            this.Token = token;
            Logined?.Invoke(this, EventArgs.Empty);
        }

        public  bool ProcessCommand(byte[] buffer, int offset, int count)
        {
            var text = Encoding.UTF8.GetString(buffer, offset, count);
            Console.WriteLine("接收到数据：" + text);
            return true;
        }

    }
}
