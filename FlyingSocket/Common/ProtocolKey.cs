using FlyingFive;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace FlyingSocket.Common
{
    /// <summary>
    /// 支持的协议类型
    /// </summary>
    public enum SocketProtocolType : byte
    {
        /// <summary>
        /// 默认数据协议
        /// </summary>
        Default = 0xf,
        /// <summary>
        /// 上传协议
        /// </summary>
        Upload = 0x1,
        /// <summary>
        /// 下载协议
        /// </summary>
        Download = 0x2,
        /// <summary>
        /// SQL协议
        /// </summary>
        SQL = 0x3,
    }

    public class ProtocolKey
    {
        /// <summary>
        /// 指定socket请求消息的编码字符
        /// </summary>
        public static readonly string Request = "Request";
        /// <summary>
        /// 指定socket响应消息的编码字符
        /// </summary>
        public static readonly string Response = "Response";
        /// <summary>
        /// 字符[
        /// </summary>
        public static readonly string LeftBrackets = "[";
        /// <summary>
        /// 字符]
        /// </summary>
        public static readonly string RightBrackets = "]";
        /// <summary>
        /// 换行的编码字符
        /// </summary>
        public static readonly string ReturnWrap = "\r\n";
        /// <summary>
        /// 字符=
        /// </summary>
        public static readonly string EqualSign = "=";
        /// <summary>
        /// 指定命令名称的字符
        /// </summary>
        public static readonly string Command = "Command";
        /// <summary>
        /// 指定命令响应编码的字符
        /// </summary>
        public static readonly string Code = "Code";
        /// <summary>
        /// 指定命令响应消息的字符
        /// </summary>
        public static readonly string Message = "Message";
        public static string UserName = "UserName";
        public static string Password = "Password";
        public static string FileName = "FileName";
        public static string Item = "Item";
        public static string ParentDir = "ParentDir";
        public static string DirName = "DirName";
        public static char TextSeperator = (char)1;
        public static string FileSize = "FileSize";
        public static string PacketSize = "PacketSize";


        public static string Login = "Login";
        public static string Active = "Active";

        public static string Dir = "Dir";
        public static string CreateDir = "CreateDir";
        public static string DeleteDir = "DeleteDir";
        public static string FileList = "FileList";
        public static string DeleteFile = "DeleteFile";
        public static string Upload = "Upload";
        public static string Data = "Data";
        /// <summary>
        /// 指定操作结束的编码字符
        /// </summary>
        public static string Eof = "EOF";
        /// <summary>
        /// 
        /// </summary>
        //public static string Download = "Download";
        public static string SendFile = "SendFile";


        public static readonly string BeginCommandKey = "BEGIN";
        public static readonly string DataLengthKey = "LENGTH";
    }

    public enum ProtocolStatus : int
    {
        [Description("成功")]
        Success = 0x00000000,
        [Description("不支持的命令")]
        UnSupportedCommand = 0x00000001,
        [Description("数据包长度错误")]
        PacketLengthError = 0x00000002,
        [Description("数据包格式错误")]
        PacketFormatError = 0x00000003,
        [Description("命令未完成")]
        CommandNotCompleted = 0x00000004,
        [Description("参数错误")]
        ParameterError = 0x00000005,
        Error = 0x00000006,
    }

    public class ProtocolCode
    {
        public static string GetErrorString(ProtocolStatus status)
        {
            var desc = status.GetCustomAttribute<DescriptionAttribute>();
            return desc == null ? "" : desc.Description;
        }
    }


    /// <summary>
    /// 文件上传协议中支持的命令
    /// </summary>
    public enum UploadProtocolCommand
    {
        Login = 0,
        Active = 2,
        Dir = 3,
        CreateDir = 4,
        DeleteDir = 5,
        FileList = 6,
        DeleteFile = 7,
        Upload = 8,
        Data = 9,
        Eof = 10,
    }

    /// <summary>
    /// 文件下载协议中支持的命令
    /// </summary>
    public enum DownloadProtocolCommand
    {
        //None = 0,
        Login = 0,
        Active = 2,
        Dir = 3,
        FileList = 4,
        Download = 5,
    }
}
