using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlyingSocket.Core;

namespace FlyingSocket.Client
{
    /// <summary>
    /// 数据上传Socket客户端
    /// </summary>
    public class UploadSocketClient : SocketClientBase
    {
        public UploadSocketClient()
            : base()
        {
            _protocolFlag = ProtocolFlag.Upload;
        }

        public bool Upload(string dirName, string fileName, ref long fileSize)
        {
            bool bConnect = ReconnectAndLogin(); //检测连接是否还在，如果断开则重连并登录
            if (!bConnect)
            {
                return bConnect;
            }
            try
            {
                OutgoingDataAssembler.Clear();
                OutgoingDataAssembler.AddRequest();
                OutgoingDataAssembler.AddCommand(ProtocolKey.Upload);
                OutgoingDataAssembler.AddValue(ProtocolKey.DirName, dirName);
                OutgoingDataAssembler.AddValue(ProtocolKey.FileName, Path.GetFileName(fileName));
                SendCommand();
                var success = ReceiveCommand();
                if (success)
                {
                    success = CheckErrorCode();
                    if (success)
                    {
                        success = IncomingDataParser.GetValue(ProtocolKey.FileSize, ref fileSize);
                    }
                    //return success;
                    if (!success) { return false; }
                    int PacketSize = 32 * 1024;         //32KB
                    using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                    {
                        fileStream.Position = fileSize;
                        byte[] readBuffer = new byte[PacketSize];
                        while (fileStream.Position < fileStream.Length)
                        {
                            int count = fileStream.Read(readBuffer, 0, PacketSize);
                            if (!this.DoData(readBuffer, 0, count))
                                throw new Exception(this.ErrorString);
                        }
                        if (!this.DoEof(fileStream.Length))
                            throw new Exception(this.ErrorString);
                        return true;
                    }

                }
                else
                {
                    return false;
                }
            }
            catch (Exception E)
            {
                //记录日志
                ErrorString = E.Message;
                return false;
            }
        }

        public bool DoData(byte[] buffer, int offset, int count)
        {
            try
            {
                OutgoingDataAssembler.Clear();
                OutgoingDataAssembler.AddRequest();
                OutgoingDataAssembler.AddCommand(ProtocolKey.Data);
                SendCommand(buffer, offset, count);
                return true;
            }
            catch (Exception E)
            {
                //记录日志
                ErrorString = E.Message;
                return false;
            }
        }

        public bool DoEof(Int64 fileSize)
        {
            try
            {
                OutgoingDataAssembler.Clear();
                OutgoingDataAssembler.AddRequest();
                OutgoingDataAssembler.AddCommand(ProtocolKey.Eof);
                SendCommand();
                bool bSuccess = ReceiveCommand();
                if (bSuccess)
                {
                    return CheckErrorCode();
                }
                else
                {
                    return false;
                }
            }
            catch (Exception E)
            {
                //记录日志
                ErrorString = E.Message;
                return false;
            }
        }
    }
}
