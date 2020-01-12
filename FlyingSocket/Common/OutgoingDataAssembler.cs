using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingSocket.Common
{
    /// <summary>
    /// 数据发送组装器
    /// </summary>
    public class OutgoingDataAssembler
    {
        private List<string> _protocolText = null;

        public OutgoingDataAssembler()
        {
            _protocolText = new List<string>();
        }

        public void Clear()
        {
            _protocolText.Clear();
        }

        public string GetProtocolText()
        {
            var tmpStr = "";
            if (_protocolText.Count > 0)
            {
                tmpStr = _protocolText[0];
                for (int i = 1; i < _protocolText.Count; i++)
                {
                    tmpStr = tmpStr + CommandKeys.ReturnWrap + _protocolText[i];
                }
            }
            return tmpStr;
        }

        /// <summary>
        /// 添加请求协议命令文本:[Request]
        /// </summary>
        public void AddRequest()
        {
            _protocolText.Add(CommandKeys.LeftBrackets + CommandKeys.Request + CommandKeys.RightBrackets);
        }

        /// <summary>
        /// 添加响应协议命令文本:[Response]
        /// </summary>
        public void AddResponse()
        {
            _protocolText.Add(CommandKeys.LeftBrackets + CommandKeys.Response + CommandKeys.RightBrackets);
        }

        /// <summary>
        /// 添加命令协议命令文本:Command=commandText
        /// </summary>
        /// <param name="commandKey"></param>
        public void AddCommand(string commandKey)
        {
            _protocolText.Add(CommandKeys.Command + CommandKeys.EqualSign + commandKey);
        }

        /// <summary>
        /// 开始请求
        /// </summary>
        public void BeginRequest(int requestLength)
        {
            _protocolText.Add(string.Format("{0}{1}{2}", CommandKeys.Command, CommandKeys.EqualSign, CommandKeys.Begin));
            _protocolText.Add(string.Format("{0}{1}{2}", CommandKeys.DataLength, CommandKeys.EqualSign, requestLength));
        }

        /// <summary>
        /// 添加成功协议命令文本:Code=0
        /// </summary>
        public void AddSuccess()
        {
            _protocolText.Add(CommandKeys.Code + CommandKeys.EqualSign + ((int)CommandResult.Success).ToString());
        }


        /// <summary>
        /// 添加失败协议命令文本:Code=xMessage=xxx
        /// </summary>
        /// <param name="status"></param>
        /// <param name="message"></param>
        public void AddFailure(CommandResult status, string message = "")
        {
            _protocolText.Add(CommandKeys.Code + CommandKeys.EqualSign + Convert.ToInt32(status).ToString());
            if (string.IsNullOrEmpty(message))
            {
                message = ProtocolCode.GetErrorString(status);
            }
            if (string.IsNullOrEmpty(message)) { throw new InvalidOperationException("没有为客户端返回错误信息"); }

            _protocolText.Add(CommandKeys.Message + CommandKeys.EqualSign + message);
        }

        public void AddValue(string protocolKey, string value)
        {
            _protocolText.Add(protocolKey + CommandKeys.EqualSign + value);
        }

        public void AddValue(string protocolKey, short value)
        {
            _protocolText.Add(protocolKey + CommandKeys.EqualSign + value.ToString());
        }

        public void AddValue(string protocolKey, int value)
        {
            _protocolText.Add(protocolKey + CommandKeys.EqualSign + value.ToString());
        }

        public void AddValue(string protocolKey, long value)
        {
            _protocolText.Add(protocolKey + CommandKeys.EqualSign + value.ToString());
        }

        public void AddValue(string protocolKey, Single value)
        {
            _protocolText.Add(protocolKey + CommandKeys.EqualSign + value.ToString());
        }

        public void AddValue(string protocolKey, double value)
        {
            _protocolText.Add(protocolKey + CommandKeys.EqualSign + value.ToString());
        }
    }
}
