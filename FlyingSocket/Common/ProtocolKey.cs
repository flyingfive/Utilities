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
    public enum SocketProtocolType : int
    {
        /// <summary>
        /// 未定义协议
        /// </summary>
        Undefine = 0x0,
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

    /// <summary>
    /// 命令标识的编码字符
    /// </summary>
    public class CommandKeys
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


        //public static string Login = "Login";
        //public static string Active = "Active";

        //public static string Dir = "Dir";
        //public static string CreateDir = "CreateDir";
        //public static string DeleteDir = "DeleteDir";
        //public static string FileList = "FileList";
        //public static string DeleteFile = "DeleteFile";

        /// <summary>
        /// 客户端登录认证命令
        /// </summary>
        public static readonly string Login = "Login";
        public static readonly string Protocol = "Protocol";
        public static readonly string SessionID = "SessionID";
        public static readonly string Token = "TokenId";

        /// <summary>
        /// 指定命令数据的字符
        /// </summary>
        public static readonly string Data = UploadProtocolCommand.Data.ToString();
        /// <summary>
        /// 指定命令操作结束的编码字符
        /// </summary>
        public static readonly string EOF = UploadProtocolCommand.EOF.ToString();
        /// <summary>
        /// 
        /// </summary>
        public static string SendFile = "SendFile";

        /// <summary>
        /// 指定请求开始的编码字符
        /// </summary>
        public static readonly string Begin = "BEGIN";
        /// <summary>
        /// 指定请求数据长度的编码字符
        /// </summary>
        public static readonly string DataLength = "LENGTH";
    }

    /// <summary>
    /// 命令的响应结果
    /// </summary>
    public enum CommandResult : int
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
        /// <summary>
        /// 整形值占用字节数
        /// </summary>
        public static readonly int IntegerSize = sizeof(int);
        public static string GetErrorString(CommandResult status)
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
        Active = 2,
        Dir = 3,
        CreateDir = 4,
        DeleteDir = 5,
        FileList = 6,
        DeleteFile = 7,
        Upload = 8,
        Data = 9,
        EOF = 10,
    }

    /// <summary>
    /// 文件下载协议中支持的命令
    /// </summary>
    public enum DownloadProtocolCommand
    {
        Active = 2,
        Dir = 3,
        FileList = 4,
        Download = 5,
    }
}
