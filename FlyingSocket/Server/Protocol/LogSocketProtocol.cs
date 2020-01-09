using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Windows.Forms;

namespace FlyingSocket.Server.Protocol
{
    public class LogOutputSocketProtocol : BaseSocketProtocol
    {
        public LogFixedBuffer LogFixedBuffer { get; private set; }

        public LogOutputSocketProtocol(FlyingSocketServer socketServer, SocketUserToken userToken)
            : base(socketServer, userToken)
        {
            ProtocolName = "LogOutput";
            LogFixedBuffer = new LogFixedBuffer();
            lock (base.FlyingSocketServer.LogOutputSocketProtocolMgr)
            {
                base.FlyingSocketServer.LogOutputSocketProtocolMgr.Add(this);
            }
            SendResponse();
        }

        public override void Close()
        {
            lock (base.FlyingSocketServer.LogOutputSocketProtocolMgr)
            {
                base.FlyingSocketServer.LogOutputSocketProtocolMgr.Remove(this);
            }
        }

        public override bool ProcessReceive(byte[] buffer, int offset, int count)
        {
            ActiveTime = DateTime.UtcNow;
            if (count == 1)
            {
                if (buffer[0] == (byte)Keys.Escape)
                {
                    return false;
                }
                else
                {
                    return SendResponse();
                }
            }
            else
            {
                return SendResponse();
            }
        }

        public override bool SendCallback()
        {
            bool result = base.SendCallback();
            if (LogFixedBuffer.DataCount > 0)
            {
                result = SendBuffer(LogFixedBuffer.FixedBuffer, 0, LogFixedBuffer.DataCount);
                LogFixedBuffer.Clear();
            }
            return result;
        }

        /// <summary>
        /// 主动发送，如果没有回调的时候，需要主动下发，否则等待回调
        /// </summary>
        /// <returns></returns>
        public bool InitiativeSend()
        {
            if (!IsAsyncSending)
            {
                return SendCallback();
            }
            return true;
        }

        public bool SendResponse()
        {
            LogFixedBuffer.WriteString("\r\nNET IOCP Demo Server, SQLDebug_Fan fansheng_hx@163.com, http://blog.csdn.net/SQLDebug_Fan\r\n");
            LogFixedBuffer.WriteString("Press ESC to exit\r\n");
            return InitiativeSend();
        }
    }

    public class LogOutputSocketProtocolMgr 
    {
        private List<LogOutputSocketProtocol> _innerList = null;

        public LogOutputSocketProtocolMgr()
        {
            _innerList = new List<LogOutputSocketProtocol>();
        }

        public int Count()
        {
            return _innerList.Count;
        }

        public LogOutputSocketProtocol ElementAt(int index)
        {
            return _innerList.ElementAt(index);
        }

        public void Add(LogOutputSocketProtocol value)
        {
            _innerList.Add(value);
        }

        public void Remove(LogOutputSocketProtocol value)
        {
            _innerList.Remove(value);
        }
    }

    public class LogFixedBuffer 
    {
        public byte[] FixedBuffer { get; private set; }
        public int DataCount { get; private set; }

        public LogFixedBuffer()
        {
            FixedBuffer = new byte[1024 * 16]; //申请大小为16K
            DataCount = 0;
        }

        public void WriteBuffer(byte[] buffer, int offset, int count)
        {
            if ((FixedBuffer.Length - DataCount) >= count) //如果长度够，则复制内存
            {
                Array.Copy(buffer, offset, FixedBuffer, DataCount, count);
                DataCount = DataCount + count;
            }
        }

        public void WriteBuffer(byte[] buffer)
        {
            WriteBuffer(buffer, 0, buffer.Length);
        }

        public void WriteString(string value)
        {
            byte[] tmpBuffer = Encoding.UTF8.GetBytes(value);
            WriteBuffer(tmpBuffer);
        }

        public void Clear()
        {
            DataCount = 0;
        }
    }

    ////扩展log4net的日志输出
    //class LogSocketAppender : log4net.Appender.AppenderSkeleton
    //{
    //    public LogSocketAppender()
    //    {
    //        Name = "LogSocketAppender";
    //    }

    //    protected override void Append(LoggingEvent loggingEvent)
    //    {
    //        string strLoggingMessage = RenderLoggingEvent(loggingEvent);
    //        byte[] tmpBuffer = Encoding.Default.GetBytes(strLoggingMessage);
    //        lock (Program.AsyncSocketSvr.LogOutputSocketProtocolMgr)
    //        {
    //            for (int i = 0; i < Program.AsyncSocketSvr.LogOutputSocketProtocolMgr.Count(); i++)
    //            {
    //                Program.AsyncSocketSvr.LogOutputSocketProtocolMgr.ElementAt(i).LogFixedBuffer.WriteBuffer(tmpBuffer);
    //                Program.AsyncSocketSvr.LogOutputSocketProtocolMgr.ElementAt(i).InitiativeSend();
    //            }
    //        }
    //    }
    //}
}
