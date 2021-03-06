﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using FlyingSocket.Common;
using FlyingSocket.Utility;
using FlyingFive;

namespace FlyingSocket.Server.Protocol
{
    [ProtocolName(SocketProtocolType.Upload)]
    public class UploadSocketProtocol : BaseSocketProtocol
    {
        /// <summary>
        /// 上传文件名
        /// </summary>
        public string FileName { get; private set; }
        /// <summary>
        /// 写入服务端的文件流
        /// </summary>
        private FileStream _fileStream = null;

        public UploadSocketProtocol(FlyingSocketServer socketServer, SocketUserToken userToken)
            : base(socketServer, userToken)
        {
            _fileStream = null;
            FileName = "";
            lock (FlyingSocketServer.UploadInstances)
            {
                FlyingSocketServer.UploadInstances.Add(this);
            }
        }

        public override void Close()
        {
            FileName = "";
            if (_fileStream != null)
            {
                _fileStream.Close();
                _fileStream = null;
            }
            lock (FlyingSocketServer.UploadInstances)
            {
                FlyingSocketServer.UploadInstances.Remove(this);
            }
            base.Close();
        }

        public override bool ProcessCommand(byte[] buffer, int offset, int count) //处理分完包的数据，子类从这个方法继承
        {
            OutgoingDataAssembler.Clear();
            if (!Enum.GetNames(typeof(UploadProtocolCommand)).Contains(IncomingDataParser.Command))
            {
                OutgoingDataAssembler.AddFailure(CommandResult.UnSupportedCommand, "");
                return false;
            }
            var command = IncomingDataParser.Command.TryConvert<UploadProtocolCommand>();
            OutgoingDataAssembler.AddResponse();
            OutgoingDataAssembler.AddCommand(IncomingDataParser.Command);
            if (!CheckLogined(command)) //检测登录
            {
                OutgoingDataAssembler.AddFailure(CommandResult.Error, "用户未登录");
                return SendResult();
            }
            switch (command)
            {
                case UploadProtocolCommand.Active: return DoActive();
                case UploadProtocolCommand.Dir: return ListDirectories();
                case UploadProtocolCommand.CreateDir: return CreateDirectory();
                case UploadProtocolCommand.FileList: return ListFiles();
                case UploadProtocolCommand.Upload: return Upload();
                case UploadProtocolCommand.DeleteDir: return DeleteDirectory();
                case UploadProtocolCommand.DeleteFile: return DeleteFile();
                case UploadProtocolCommand.Data: return DoData(buffer, offset, count);
                case UploadProtocolCommand.EOF: return Eof();
                default:
                    OutgoingDataAssembler.AddFailure(CommandResult.UnSupportedCommand, "");
                    return false;
            }
        }

        public bool CheckLogined(UploadProtocolCommand command)
        {
            return true;
            //if ((command == UploadProtocolCommand.Login) | (command == UploadProtocolCommand.Active))
            //{
            //    return true;
            //}
            //else
            //{
            //    return base.Logined;
            //}
        }

        private bool ListDirectories()
        {
            var parentDir = "";
            if (IncomingDataParser.GetValue(CommandKeys.ParentDir, ref parentDir))
            {
                if (parentDir == "")
                {
                    parentDir = FileWorkingDirectory;
                }
                else
                {
                    parentDir = Path.Combine(FileWorkingDirectory, parentDir);
                }
                if (Directory.Exists(parentDir))
                {
                    var subDirectorys = Directory.GetDirectories(parentDir, "*", SearchOption.TopDirectoryOnly);
                    OutgoingDataAssembler.AddSuccess();
                    var directorySeparator = new char[1];
                    directorySeparator[0] = Path.DirectorySeparatorChar;
                    for (int i = 0; i < subDirectorys.Length; i++)
                    {
                        string[] directoryName = subDirectorys[i].Split(directorySeparator, StringSplitOptions.RemoveEmptyEntries);
                        OutgoingDataAssembler.AddValue(CommandKeys.Item, directoryName[directoryName.Length - 1]);
                    }
                }
                else
                {
                    OutgoingDataAssembler.AddFailure(CommandResult.Error, "目录不存在");
                }
            }
            else
            {
                OutgoingDataAssembler.AddFailure(CommandResult.ParameterError);
            }
            return SendResult();
        }

        private bool CreateDirectory()
        {
            var parentDir = "";
            var dirName = "";
            if (IncomingDataParser.GetValue(CommandKeys.ParentDir, ref parentDir) & IncomingDataParser.GetValue(CommandKeys.DirName, ref dirName))
            {
                if (parentDir == "")
                {
                    parentDir = FileWorkingDirectory;
                }
                else
                {
                    parentDir = Path.Combine(FileWorkingDirectory, parentDir);
                }
                if (Directory.Exists(parentDir))
                {
                    try
                    {
                        parentDir = Path.Combine(parentDir, dirName);
                        Directory.CreateDirectory(parentDir);
                        OutgoingDataAssembler.AddSuccess();
                    }
                    catch (Exception E)
                    {
                        OutgoingDataAssembler.AddFailure(CommandResult.Error, "目录创建错误");
                    }
                }
                else
                {
                    OutgoingDataAssembler.AddFailure(CommandResult.Error, "目录不存在");
                }
            }
            else
            {
                OutgoingDataAssembler.AddFailure(CommandResult.ParameterError);
            }
            return SendResult();
        }

        private bool DeleteDirectory()
        {
            var parentDir = "";
            var dirName = "";
            if (IncomingDataParser.GetValue(CommandKeys.ParentDir, ref parentDir) & IncomingDataParser.GetValue(CommandKeys.DirName, ref dirName))
            {
                if (parentDir == "")
                {
                    parentDir = FileWorkingDirectory;
                }
                else
                {
                    parentDir = Path.Combine(FileWorkingDirectory, parentDir);
                }
                if (Directory.Exists(parentDir))
                {
                    try
                    {
                        parentDir = Path.Combine(parentDir, dirName);
                        Directory.Delete(parentDir, true);
                        OutgoingDataAssembler.AddSuccess();
                    }
                    catch (Exception E)
                    {
                        OutgoingDataAssembler.AddFailure(CommandResult.Error, E.Message);
                    }
                }
                else
                {
                    OutgoingDataAssembler.AddFailure(CommandResult.Error, "目录不存在");
                }
            }
            else
            {
                OutgoingDataAssembler.AddFailure(CommandResult.ParameterError);
            }
            return SendResult();
        }

        private bool ListFiles()
        {
            var dirName = "";
            if (IncomingDataParser.GetValue(CommandKeys.DirName, ref dirName))
            {
                if (dirName == "")
                {
                    dirName = FileWorkingDirectory;
                }
                else
                {
                    dirName = Path.Combine(FileWorkingDirectory, dirName);
                }
                if (Directory.Exists(dirName))
                {
                    var files = Directory.GetFiles(dirName);
                    OutgoingDataAssembler.AddSuccess();
                    Int64 fileSize = 0;
                    for (int i = 0; i < files.Length; i++)
                    {
                        FileInfo fileInfo = new FileInfo(files[i]);
                        fileSize = fileInfo.Length;
                        OutgoingDataAssembler.AddValue(CommandKeys.Item, fileInfo.Name + CommandKeys.TextSeperator + fileSize.ToString());
                    }
                }
                else
                {
                    OutgoingDataAssembler.AddFailure(CommandResult.Error, "目录不存在");
                }
            }
            else
            {
                OutgoingDataAssembler.AddFailure(CommandResult.ParameterError);
            }
            return SendResult();
        }

        private bool DeleteFile()
        {
            var dirName = "";
            if (IncomingDataParser.GetValue(CommandKeys.DirName, ref dirName))
            {
                if (dirName == "")
                {
                    dirName = FileWorkingDirectory;
                }
                else
                {
                    dirName = Path.Combine(FileWorkingDirectory, dirName);
                }
                var fileName = "";
                if (Directory.Exists(dirName))
                {
                    try
                    {
                        List<string> files = IncomingDataParser.GetValue(CommandKeys.Item);
                        for (int i = 0; i < files.Count; i++)
                        {
                            fileName = Path.Combine(dirName, files[i]);
                            File.Delete(fileName);
                        }
                        OutgoingDataAssembler.AddSuccess();
                    }
                    catch (Exception E)
                    {
                        OutgoingDataAssembler.AddFailure(CommandResult.Error, E.Message);
                    }
                }
                else
                {
                    OutgoingDataAssembler.AddFailure(CommandResult.Error, "目录不存在");
                }
            }
            else
            {
                OutgoingDataAssembler.AddFailure(CommandResult.ParameterError);
            }
            return SendResult();
        }

        /// <summary>
        /// step1:开始文件上传。创建文件，并打开文件流准备数据写入。
        /// </summary>
        /// <returns></returns>
        private bool Upload()
        {
            var dirName = "";
            var fileName = "";
            if (IncomingDataParser.GetValue(CommandKeys.DirName, ref dirName) & IncomingDataParser.GetValue(CommandKeys.FileName, ref fileName))
            {
                if (dirName == "")
                {
                    dirName = FileWorkingDirectory;
                }
                else
                {
                    dirName = Path.Combine(FileWorkingDirectory, dirName);
                }
                if (!Directory.Exists(dirName)) { Directory.CreateDirectory(dirName); }
                fileName = Path.Combine(dirName, fileName);
                //Program.Logger.Info("Start upload file: " + fileName);
                if (_fileStream != null) //关闭上次传输的文件
                {
                    _fileStream.Close();
                    _fileStream = null;
                    FileName = "";
                }
                if (File.Exists(fileName))
                {
                    if (!CheckFileInUse(fileName)) //检测文件是否正在使用中
                    {
                        FileName = fileName;
                        _fileStream = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite);
                        _fileStream.Position = _fileStream.Length; //文件移到末尾
                        OutgoingDataAssembler.AddSuccess();
                        OutgoingDataAssembler.AddValue(CommandKeys.FileSize, _fileStream.Length);
                    }
                    else
                    {
                        OutgoingDataAssembler.AddFailure(CommandResult.Error, "文件正在使用中");
                        //Program.Logger.Error("Start upload file error, file is in use: " + fileName);
                    }
                }
                else
                {
                    FileName = fileName;
                    _fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    _fileStream.Position = _fileStream.Length; //文件移到末尾
                    OutgoingDataAssembler.AddSuccess();
                    OutgoingDataAssembler.AddValue(CommandKeys.FileSize, _fileStream.Length);
                }
            }
            else
            {
                OutgoingDataAssembler.AddFailure(CommandResult.ParameterError);
            }
            return SendResult();
        }

        //检测文件是否正在使用中，如果正在使用中则检测是否被上传协议占用，如果占用则关闭,真表示正在使用中，并没有关闭
        private bool CheckFileInUse(string fileName)
        {
            if (FileUtility.CheckFileInUse(fileName))
            {
                bool result = true;
                lock (FlyingSocketServer.UploadInstances)
                {
                    UploadSocketProtocol uploadSocketProtocol = null;
                    for (int i = 0; i < FlyingSocketServer.UploadInstances.Count(); i++)
                    {
                        uploadSocketProtocol = FlyingSocketServer.UploadInstances.ElementAt(i);
                        if (fileName.Equals(uploadSocketProtocol.FileName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            lock (uploadSocketProtocol.SocketUserToken) //SocketUserToken有多个线程访问
                            {
                                FlyingSocketServer.CloseClientConnection(uploadSocketProtocol.SocketUserToken);
                            }
                            result = false;
                        }
                    }
                }
                return result;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// step1:写入文件数据到打开的文件流
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private bool DoData(byte[] buffer, int offset, int count)
        {
            if (_fileStream == null)
            {
                OutgoingDataAssembler.AddFailure(CommandResult.Error, "文件未打开");
                return false;
            }
            else
            {
                _fileStream.Write(buffer, offset, count);
                return true;
                //m_outgoingDataAssembler.AddSuccess();
                //m_outgoingDataAssembler.AddValue(ProtocolKey.Count, count); //返回读取个数
            }
            //return DoSendResult(); //接收数据不发回响应
        }

        /// <summary>
        /// step3:收到该指令表示文件数据上传完成，服务端关闭文件流。
        /// todo：应加上MD5校验文件完整性。
        /// </summary>
        /// <returns></returns>
        private bool Eof()
        {
            if (_fileStream == null)
            {
                OutgoingDataAssembler.AddFailure(CommandResult.Error, "文件未打开");
            }
            else
            {
                //Program.Logger.Info("End upload file: " + m_fileName);
                _fileStream.Close();
                _fileStream = null;
                FileName = "";
                OutgoingDataAssembler.AddSuccess();
            }
            return SendResult();
        }
    }

}
