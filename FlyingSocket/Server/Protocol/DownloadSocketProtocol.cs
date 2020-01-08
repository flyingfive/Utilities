using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using FlyingSocket.Core;
using FlyingSocket.Utility;

namespace FlyingSocket.Server.Protocol
{
    /// <summary>
    /// 文件下载协议
    /// </summary>
    public class DownloadSocketProtocol : BaseSocketProtocol
    {
        /// <summary>
        /// 下载文件名
        /// </summary>
        public string FileName { get; private set; }
        private FileStream _fileStream = null;
        private bool _sendFile = false;
        private int _packetSize = 0;
        private byte[] _readBuffer = null;

        public DownloadSocketProtocol(FlyingSocketServer socketServer, SocketUserToken userToken)
            : base(socketServer, userToken)
        {
            ProtocolName = "Download";
            FileName = "";
            _fileStream = null;
            _sendFile = false;
            _packetSize = 64 * 1024;
            lock (FlyingSocketServer.DownloadSocketProtocolMgr)
            {
                FlyingSocketServer.DownloadSocketProtocolMgr.Add(this);
            }
        }

        public override void Close()
        {
            base.Close();
            FileName = "";
            _sendFile = false;
            if (_fileStream != null)
            {
                _fileStream.Close();
                _fileStream = null;
            }
            lock (FlyingSocketServer)
            {
                FlyingSocketServer.DownloadSocketProtocolMgr.Remove(this);
            }
        }

        public override bool ProcessCommand(byte[] buffer, int offset, int count) //处理分完包的数据，子类从这个方法继承
        {
            DownloadSocketCommand command = ParseCommand(IncomingDataParser.Command);
            OutgoingDataAssembler.Clear();
            OutgoingDataAssembler.AddResponse();
            OutgoingDataAssembler.AddCommand(IncomingDataParser.Command);
            if (!CheckLogined(command)) //检测登录
            {
                OutgoingDataAssembler.AddFailure(ProtocolCode.UserHasLogined, "");
                return SendResult();
            }
            if (command == DownloadSocketCommand.Login)
            {
                return DoLogin();
            }
            else if (command == DownloadSocketCommand.Active)
            {
                return DoActive();
            }
            else if (command == DownloadSocketCommand.Dir)
            {
                return DoDir();
            }
            else if (command == DownloadSocketCommand.FileList)
            {
                return DoFileList();
            }
            else if (command == DownloadSocketCommand.Download)
            {
                return DoDownload();
            }
            else
            {
                //Program.Logger.Error("Unknow command: " + m_incomingDataParser.Command);
                return false;
            }
        }

        public DownloadSocketCommand ParseCommand(string command)
        {
            if (command.Equals(ProtocolKey.Active, StringComparison.CurrentCultureIgnoreCase))
            {
                return DownloadSocketCommand.Active;
            }
            else if (command.Equals(ProtocolKey.Login, StringComparison.CurrentCultureIgnoreCase))
            {
                return DownloadSocketCommand.Login;
            }
            else if (command.Equals(ProtocolKey.Dir, StringComparison.CurrentCultureIgnoreCase))
            {
                return DownloadSocketCommand.Dir;
            }
            else if (command.Equals(ProtocolKey.FileList, StringComparison.CurrentCultureIgnoreCase))
            {
                return DownloadSocketCommand.FileList;
            }
            else if (command.Equals(ProtocolKey.Download, StringComparison.CurrentCultureIgnoreCase))
            {
                return DownloadSocketCommand.Download;
            }
            else
            {
                return DownloadSocketCommand.None;
            }
        }

        public bool CheckLogined(DownloadSocketCommand command)
        {
            if ((command == DownloadSocketCommand.Login) | (command == DownloadSocketCommand.Active))
            {
                return true;
            }
            else
            {
                return Logined;
            }
        }

        public bool DoDir()
        {
            var parentDir = "";
            if (IncomingDataParser.GetValue(ProtocolKey.ParentDir, ref parentDir))
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
                    string[] subDirectorys = Directory.GetDirectories(parentDir, "*", SearchOption.TopDirectoryOnly);
                    OutgoingDataAssembler.AddSuccess();
                    char[] directorySeparator = new char[1];
                    directorySeparator[0] = Path.DirectorySeparatorChar;
                    for (int i = 0; i < subDirectorys.Length; i++)
                    {
                        string[] directoryName = subDirectorys[i].Split(directorySeparator, StringSplitOptions.RemoveEmptyEntries);
                        OutgoingDataAssembler.AddValue(ProtocolKey.Item, directoryName[directoryName.Length - 1]);
                    }
                }
                else
                {
                    OutgoingDataAssembler.AddFailure(ProtocolCode.DirNotExist, "");
                }
            }
            else
            {
                OutgoingDataAssembler.AddFailure(ProtocolCode.ParameterError, "");
            }
            return SendResult();
        }

        public bool DoFileList()
        {
            var dirName = "";
            if (IncomingDataParser.GetValue(ProtocolKey.DirName, ref dirName))
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
                    string[] files = Directory.GetFiles(dirName);
                    OutgoingDataAssembler.AddSuccess();
                    Int64 fileSize = 0;
                    for (int i = 0; i < files.Length; i++)
                    {
                        var fileInfo = new FileInfo(files[i]);
                        fileSize = fileInfo.Length;
                        OutgoingDataAssembler.AddValue(ProtocolKey.Item, fileInfo.Name + ProtocolKey.TextSeperator + fileSize.ToString());
                    }
                }
                else
                {
                    OutgoingDataAssembler.AddFailure(ProtocolCode.DirNotExist, "");
                }
            }
            else
            {
                OutgoingDataAssembler.AddFailure(ProtocolCode.ParameterError, "");
            }
            return SendResult();
        }

        public bool DoDownload()
        {
            string dirName = "";
            string fileName = "";
            Int64 fileSize = 0;
            int packetSize = 0;
            if (IncomingDataParser.GetValue(ProtocolKey.DirName, ref dirName) & IncomingDataParser.GetValue(ProtocolKey.FileName, ref fileName)
                & IncomingDataParser.GetValue(ProtocolKey.FileSize, ref fileSize) & IncomingDataParser.GetValue(ProtocolKey.PacketSize, ref packetSize))
            {
                if (dirName == "")
                {
                    dirName = FileWorkingDirectory;
                }
                else
                {
                    dirName = Path.Combine(FileWorkingDirectory, dirName);
                }
                fileName = Path.Combine(dirName, fileName);
                //Program.Logger.Info("Start download file: " + fileName);
                if (_fileStream != null) //关闭上次传输的文件
                {
                    _fileStream.Close();
                    _fileStream = null;
                    FileName = "";
                    _sendFile = false;
                }
                if (File.Exists(fileName))
                {
                    if (!CheckFileInUse(fileName)) //检测文件是否正在使用中
                    {
                        FileName = fileName;
                        _fileStream = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite);
                        _fileStream.Position = fileSize; //文件移到上次下载位置
                        OutgoingDataAssembler.AddSuccess();
                        _sendFile = true;
                        _packetSize = packetSize;
                    }
                    else
                    {
                        OutgoingDataAssembler.AddFailure(ProtocolCode.FileIsInUse, "");
                        //Program.Logger.Error("Start download file error, file is in use: " + fileName);
                    }
                }
                else
                {
                    OutgoingDataAssembler.AddFailure(ProtocolCode.FileNotExist, "");
                }
            }
            else
            {
                OutgoingDataAssembler.AddFailure(ProtocolCode.ParameterError, "");
            }
            return SendResult();
        }

        //检测文件是否正在使用中，如果正在使用中则检测是否被上传协议占用，如果占用则关闭,真表示正在使用中，并没有关闭
        public bool CheckFileInUse(string fileName)
        {
            if (FileUtility.IsFileInUse(fileName))
            {
                bool result = true;
                lock (FlyingSocketServer.DownloadSocketProtocolMgr)
                {
                    DownloadSocketProtocol downloadSocketProtocol = null;
                    for (int i = 0; i < FlyingSocketServer.DownloadSocketProtocolMgr.Count(); i++)
                    {
                        downloadSocketProtocol = FlyingSocketServer.DownloadSocketProtocolMgr.ElementAt(i);
                        if (fileName.Equals(downloadSocketProtocol.FileName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            lock (downloadSocketProtocol.SocketUserToken) //AsyncSocketUserToken有多个线程访问
                            {
                                FlyingSocketServer.CloseClientConnection(downloadSocketProtocol.SocketUserToken);
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

        public override bool SendCallback()
        {
            var result = base.SendCallback();
            if (_fileStream != null)
            {
                if (_sendFile) //发送文件头
                {
                    OutgoingDataAssembler.Clear();
                    OutgoingDataAssembler.AddResponse();
                    OutgoingDataAssembler.AddCommand(ProtocolKey.SendFile);
                    OutgoingDataAssembler.AddSuccess();
                    OutgoingDataAssembler.AddValue(ProtocolKey.FileSize, _fileStream.Length - _fileStream.Position);
                    result = SendResult();
                    _sendFile = false;
                }
                else
                {
                    if (_fileStream.Position < _fileStream.Length) //发送具体数据
                    {
                        OutgoingDataAssembler.Clear();
                        OutgoingDataAssembler.AddResponse();
                        OutgoingDataAssembler.AddCommand(ProtocolKey.Data);
                        OutgoingDataAssembler.AddSuccess();
                        if (_readBuffer == null)
                        {
                            _readBuffer = new byte[_packetSize];
                        }
                        else if (_readBuffer.Length < _packetSize) //避免多次申请内存
                        {
                            _readBuffer = new byte[_packetSize];
                        }
                        int count = _fileStream.Read(_readBuffer, 0, _packetSize);
                        result = SendResult(_readBuffer, 0, count);
                    }
                    else //发送完成
                    {
                        //Program.Logger.Info("End download file: " + m_fileName);
                        _fileStream.Close();
                        _fileStream = null;
                        FileName = "";
                        _sendFile = false;
                        result = true;
                    }
                }
            }
            return result;
        }
    }

    public class DownloadSocketProtocolMgr
    {
        private List<DownloadSocketProtocol> _innerList = null;

        public DownloadSocketProtocolMgr()
        {
            _innerList = new List<DownloadSocketProtocol>();
        }

        public int Count()
        {
            return _innerList.Count;
        }

        public DownloadSocketProtocol ElementAt(int index)
        {
            return _innerList.ElementAt(index);
        }

        public void Add(DownloadSocketProtocol value)
        {
            _innerList.Add(value);
        }

        public void Remove(DownloadSocketProtocol value)
        {
            _innerList.Remove(value);
        }
    }
}
