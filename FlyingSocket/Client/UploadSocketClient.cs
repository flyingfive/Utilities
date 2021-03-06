﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlyingSocket.Common;
using FlyingSocket.Server.Protocol;

namespace FlyingSocket.Client
{
    /// <summary>
    /// 数据上传协议的Socket客户端
    /// </summary>
    [ProtocolName(SocketProtocolType.Upload)]
    public class UploadSocketClient : BaseSocketClient
    {
        public UploadSocketClient()
            : base()
        {
        }

        public bool Upload(string dirName, string fileName, ref long fileSize)
        {
            if (!_tcpClient.Connected)
            {
                return false;
            }
            try
            {
                //step1:发送文件上传命令，DirName:文件上传目录，FileName：上传文件名
                OutgoingDataAssembler.Clear();
                OutgoingDataAssembler.AddRequest();
                OutgoingDataAssembler.AddCommand(UploadProtocolCommand.Upload.ToString());
                OutgoingDataAssembler.AddValue(CommandKeys.DirName, dirName);
                OutgoingDataAssembler.AddValue(CommandKeys.FileName, Path.GetFileName(fileName));
                SendCommand();
                var success = ReceiveCommand();
                if (success)
                {
                    success = CheckErrorCode();
                    if (success)
                    {
                        success = IncomingDataParser.GetValue(CommandKeys.FileSize, ref fileSize);
                    }
                    if (!success) { return false; }
                    //成功后开始分包上传数据,此时服务端会创建指定的空文件并打开链接到此文件的流，准备开始数据写入
                    int PacketSize = 32 * 1024;         //分包大小：32KB
                    using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                    {
                        fileStream.Position = fileSize;
                        byte[] readBuffer = new byte[PacketSize];
                        while (fileStream.Position < fileStream.Length)
                        {
                            int count = fileStream.Read(readBuffer, 0, PacketSize);
                            if (!this.DoData(readBuffer, 0, count))
                            {
                                throw new Exception(this.ErrorString);
                            }
                        }
                        //发送EOF命令告诉服务端文件上传完成，此时服务端会关闭写入文件流。
                        //todo:最好加上文件MD5校验
                        if (!this.DoEof(fileStream.Length))
                        {
                            throw new Exception(this.ErrorString);
                        }
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
                OutgoingDataAssembler.AddCommand(CommandKeys.Data);
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
                OutgoingDataAssembler.AddCommand(CommandKeys.EOF);
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
