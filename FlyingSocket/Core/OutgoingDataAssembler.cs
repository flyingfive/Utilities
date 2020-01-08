using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingSocket.Core
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
                    tmpStr = tmpStr + ProtocolKey.ReturnWrap + _protocolText[i];
                }
            }
            return tmpStr;
        }

        /// <summary>
        /// 添加请求协议命令文本:[Request]
        /// </summary>
        public void AddRequest()
        {
            _protocolText.Add(ProtocolKey.LeftBrackets + ProtocolKey.Request + ProtocolKey.RightBrackets);
        }

        /// <summary>
        /// 添加响应协议命令文本:[Response]
        /// </summary>
        public void AddResponse()
        {
            _protocolText.Add(ProtocolKey.LeftBrackets + ProtocolKey.Response + ProtocolKey.RightBrackets);
        }

        /// <summary>
        /// 添加命令协议命令文本:Command=commandText
        /// </summary>
        /// <param name="commandKey"></param>
        public void AddCommand(string commandKey)
        {
            _protocolText.Add(ProtocolKey.Command + ProtocolKey.EqualSign + commandKey);
        }

        /// <summary>
        /// 添加成功协议命令文本:Code=0
        /// </summary>
        public void AddSuccess()
        {
            _protocolText.Add(ProtocolKey.Code + ProtocolKey.EqualSign + ProtocolCode.Success.ToString());
        }

        /// <summary>
        /// 添加失败协议命令文本:Code=xMessage=xxx
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="message"></param>
        public void AddFailure(int errorCode, string message)
        {
            _protocolText.Add(ProtocolKey.Code + ProtocolKey.EqualSign + errorCode.ToString());
            _protocolText.Add(ProtocolKey.Message + ProtocolKey.EqualSign + message);
        }

        public void AddValue(string protocolKey, string value)
        {
            _protocolText.Add(protocolKey + ProtocolKey.EqualSign + value);
        }

        public void AddValue(string protocolKey, short value)
        {
            _protocolText.Add(protocolKey + ProtocolKey.EqualSign + value.ToString());
        }

        public void AddValue(string protocolKey, int value)
        {
            _protocolText.Add(protocolKey + ProtocolKey.EqualSign + value.ToString());
        }

        public void AddValue(string protocolKey, long value)
        {
            _protocolText.Add(protocolKey + ProtocolKey.EqualSign + value.ToString());
        }

        public void AddValue(string protocolKey, Single value)
        {
            _protocolText.Add(protocolKey + ProtocolKey.EqualSign + value.ToString());
        }

        public void AddValue(string protocolKey, double value)
        {
            _protocolText.Add(protocolKey + ProtocolKey.EqualSign + value.ToString());
        }
    }
}
