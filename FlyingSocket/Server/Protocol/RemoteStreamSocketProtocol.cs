using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using FlyingSocket.Core;

namespace FlyingSocket.Server.Protocol
{
    public class RemoteStreamSocketProtocol : BaseSocketProtocol
    {
        private FileStream _fileStream = null;
        private byte[] _readBuffer = null;

        public RemoteStreamSocketProtocol(FlyingSocketServer socketServer, SocketUserToken userToken)
            : base(socketServer, userToken)
        {
            ProtocolName = "RemoteStream";
            _fileStream = null;
        }

        public override void Close()
        {
            base.Close();
            if (_fileStream != null)
            {
                _fileStream.Close();
            }
            _fileStream = null;
        }

        public override bool ProcessCommand(byte[] buffer, int offset, int count) //处理分完包的数据，子类从这个方法继承
        {
            var command = ParseCommand(IncomingDataParser.Command);
            OutgoingDataAssembler.Clear();
            OutgoingDataAssembler.AddResponse();
            OutgoingDataAssembler.AddCommand(IncomingDataParser.Command);
            if (command == RemoteStreamSocketCommand.FileExists)
            {
                return CheckFileExists();
            }
            else if (command == RemoteStreamSocketCommand.OpenFile)
            {
                return OpenFile();
            }
            else if (command == RemoteStreamSocketCommand.SetSize)
            {
                return SetFileSize();
            }
            else if (command == RemoteStreamSocketCommand.GetSize)
            {
                return GetFileSize();
            }
            else if (command == RemoteStreamSocketCommand.SetPosition)
            {
                return SetPosition();
            }
            else if (command == RemoteStreamSocketCommand.GetPosition)
            {
                return DoGetPosition();
            }
            else if (command == RemoteStreamSocketCommand.Read)
            {
                return Read();
            }
            else if (command == RemoteStreamSocketCommand.Write)
            {
                return Write(buffer, offset, count);
            }
            else if (command == RemoteStreamSocketCommand.Seek)
            {
                return Seek();
            }
            else if (command == RemoteStreamSocketCommand.CloseFile)
            {
                return CloseFile();
            }
            else
            {
                //Program.Logger.Error("Unknow command: " + m_incomingDataParser.Command);
                return false;
            }
        }

        public RemoteStreamSocketCommand ParseCommand(string command)
        {
            if (command.Equals(ProtocolKey.FileExists, StringComparison.CurrentCultureIgnoreCase))
            {
                return RemoteStreamSocketCommand.FileExists;
            }
            else if (command.Equals(ProtocolKey.OpenFile, StringComparison.CurrentCultureIgnoreCase))
            {
                return RemoteStreamSocketCommand.OpenFile;
            }
            else if (command.Equals(ProtocolKey.SetSize, StringComparison.CurrentCultureIgnoreCase))
            {
                return RemoteStreamSocketCommand.SetSize;
            }
            else if (command.Equals(ProtocolKey.GetSize, StringComparison.CurrentCultureIgnoreCase))
            {
                return RemoteStreamSocketCommand.GetSize;
            }
            else if (command.Equals(ProtocolKey.SetPosition, StringComparison.CurrentCultureIgnoreCase))
            {
                return RemoteStreamSocketCommand.SetPosition;
            }
            else if (command.Equals(ProtocolKey.GetPosition, StringComparison.CurrentCultureIgnoreCase))
            {
                return RemoteStreamSocketCommand.GetPosition;
            }
            else if (command.Equals(ProtocolKey.Read, StringComparison.CurrentCultureIgnoreCase))
            {
                return RemoteStreamSocketCommand.Read;
            }
            else if (command.Equals(ProtocolKey.Write, StringComparison.CurrentCultureIgnoreCase))
            {
                return RemoteStreamSocketCommand.Write;
            }
            else if (command.Equals(ProtocolKey.Seek, StringComparison.CurrentCultureIgnoreCase))
            {
                return RemoteStreamSocketCommand.Seek;
            }
            else if (command.Equals(ProtocolKey.CloseFile, StringComparison.CurrentCultureIgnoreCase))
            {
                return RemoteStreamSocketCommand.CloseFile;
            }
            else
            {
                return RemoteStreamSocketCommand.None;
            }
        }

        public bool CheckFileExists()
        {
            var filename = "";
            if (IncomingDataParser.GetValue(ProtocolKey.FileName, ref filename))
            {
                if (File.Exists(filename))
                {
                    OutgoingDataAssembler.AddSuccess();
                }
                else
                {
                    OutgoingDataAssembler.AddFailure(ProtocolCode.FileNotExist, "file not exists");
                }
            }
            else
            {
                OutgoingDataAssembler.AddFailure(ProtocolCode.ParameterError, "");
            }
            return SendResult();
        }

        private bool OpenFile()
        {
            var filename = "";
            short mode = 0;
            if (IncomingDataParser.GetValue(ProtocolKey.FileName, ref filename) & IncomingDataParser.GetValue(ProtocolKey.Mode, ref mode))
            {
                RemoteStreamMode readWriteMode = (RemoteStreamMode)mode;
                if (File.Exists(filename))
                {
                    if (readWriteMode == RemoteStreamMode.Read)
                    { _fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read); }
                    else
                    {
                        _fileStream = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite);
                    }
                }
                else
                {
                    _fileStream = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite);
                }
                OutgoingDataAssembler.AddSuccess();
            }
            else
            {
                OutgoingDataAssembler.AddFailure(ProtocolCode.ParameterError, "");
            }
            return SendResult();
        }

        private bool SetFileSize()
        {
            long fileSize = 0;
            if (IncomingDataParser.GetValue(ProtocolKey.Size, ref fileSize))
            {
                if (_fileStream == null)
                {
                    OutgoingDataAssembler.AddFailure(ProtocolCode.NotOpenFile, "");
                }
                else
                {
                    _fileStream.SetLength(fileSize);
                    OutgoingDataAssembler.AddSuccess();
                }
            }
            else
            {
                OutgoingDataAssembler.AddFailure(ProtocolCode.ParameterError, "");
            }
            return SendResult();
        }

        private bool GetFileSize()
        {
            if (_fileStream == null)
            {
                OutgoingDataAssembler.AddFailure(ProtocolCode.NotOpenFile, "");
            }
            else
            {
                OutgoingDataAssembler.AddSuccess();
                OutgoingDataAssembler.AddValue(ProtocolKey.Size, _fileStream.Length);
            }
            return SendResult();
        }

        private bool SetPosition()
        {
            long position = 0;
            if (IncomingDataParser.GetValue(ProtocolKey.Position, ref position))
            {
                if (_fileStream == null)
                {
                    OutgoingDataAssembler.AddFailure(ProtocolCode.NotOpenFile, "");
                }
                else
                {
                    _fileStream.Position = position;
                    OutgoingDataAssembler.AddSuccess();
                }
            }
            else
            {
                OutgoingDataAssembler.AddFailure(ProtocolCode.ParameterError, "");
            }
            return SendResult();
        }

        private bool DoGetPosition()
        {
            if (_fileStream == null)
            {
                OutgoingDataAssembler.AddFailure(ProtocolCode.NotOpenFile, "");
            }
            else
            {
                OutgoingDataAssembler.AddSuccess();
                OutgoingDataAssembler.AddValue(ProtocolKey.Position, _fileStream.Position);
            }
            return SendResult();
        }

        private bool Read()
        {
            int count = 0;
            if (IncomingDataParser.GetValue(ProtocolKey.Count, ref count))
            {
                if (_fileStream == null)
                {
                    OutgoingDataAssembler.AddFailure(ProtocolCode.NotOpenFile, "");
                }
                else
                {
                    if (_readBuffer == null)
                    {
                        _readBuffer = new byte[count];
                    }
                    else if (_readBuffer.Length < count) //避免多次申请内存
                    {
                        _readBuffer = new byte[count];
                    }
                    count = _fileStream.Read(_readBuffer, 0, count);
                    OutgoingDataAssembler.AddSuccess();
                    OutgoingDataAssembler.AddValue(ProtocolKey.Count, count); //返回读取个数
                }
            }
            else
            {
                OutgoingDataAssembler.AddFailure(ProtocolCode.ParameterError, "");
            }
            return SendResult(_readBuffer, 0, count);
        }

        private bool Write(byte[] buffer, int offset, int count)
        {
            if (_fileStream == null)
            {
                OutgoingDataAssembler.AddFailure(ProtocolCode.NotOpenFile, "");
            }
            else
            {
                _fileStream.Write(buffer, offset, count);
                OutgoingDataAssembler.AddSuccess();
                OutgoingDataAssembler.AddValue(ProtocolKey.Count, count); //返回写入个数
            }
            return SendResult();
        }

        private bool Seek()
        {
            long offset = 0;
            int seekOrign = 0;
            if (IncomingDataParser.GetValue(ProtocolKey.Offset, ref offset) & IncomingDataParser.GetValue(ProtocolKey.SeekOrigin, ref seekOrign))
            {
                if (_fileStream == null)
                {
                    OutgoingDataAssembler.AddFailure(ProtocolCode.NotOpenFile, "");
                }
                else
                {
                    offset = _fileStream.Seek(offset, (SeekOrigin)seekOrign);
                    OutgoingDataAssembler.AddSuccess();
                    OutgoingDataAssembler.AddValue(ProtocolKey.Offset, offset);
                }
            }
            else
            {
                OutgoingDataAssembler.AddFailure(ProtocolCode.ParameterError, "");
            }
            return SendResult();
        }

        private bool CloseFile()
        {
            if (_fileStream == null)
            {
                OutgoingDataAssembler.AddFailure(ProtocolCode.NotOpenFile, "");
            }
            else
            {
                _fileStream.Close();
                _fileStream = null;
                OutgoingDataAssembler.AddSuccess();
            }
            return SendResult();
        }
    }
}
