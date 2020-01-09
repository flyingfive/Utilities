using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingSocket.Core
{
    /// <summary>
    /// 接收数据解析器
    /// </summary>
    public class IncomingDataParser
    {
        /// <summary>
        /// 消息头部
        /// </summary>
        public string Header { get; private set; }
        /// <summary>
        /// 命令
        /// </summary>
        public string Command { get; private set; }
        /// <summary>
        /// 名称
        /// </summary>
        public List<string> Names { get; private set; }
        /// <summary>
        /// 值
        /// </summary>
        public List<string> Values { get; private set; }

        public IncomingDataParser()
        {
            Names = new List<string>();
            Values = new List<string>();
        }

        public bool DecodeProtocolText(string protocolText)
        {
            Header = "";
            Names.Clear();
            Values.Clear();
            int speIndex = protocolText.IndexOf(ProtocolKey.ReturnWrap);
            if (speIndex < 0)
            {
                return false;
            }
            else
            {
                string[] tmpNameValues = protocolText.Split(new string[] { ProtocolKey.ReturnWrap }, StringSplitOptions.RemoveEmptyEntries);
                if (tmpNameValues.Length < 2) //每次命令至少包括两行
                {
                    return false;
                }
                for (int i = 0; i < tmpNameValues.Length; i++)
                {
                    string[] tmpStr = tmpNameValues[i].Split(new string[] { ProtocolKey.EqualSign }, StringSplitOptions.None);
                    if (tmpStr.Length > 1) //存在等号
                    {
                        if (tmpStr.Length > 2) //超过两个等号，返回失败
                            return false;
                        if (tmpStr[0].Equals(ProtocolKey.Command, StringComparison.CurrentCultureIgnoreCase))
                        {
                            Command = tmpStr[1];
                        }
                        else
                        {
                            Names.Add(tmpStr[0].ToLower());
                            Values.Add(tmpStr[1]);
                        }
                    }
                }
                return true;
            }
        }

        public bool GetValue(string protocolKey, ref string value)
        {
            int index = Names.IndexOf(protocolKey.ToLower());
            if (index > -1)
            {
                value = Values[index];
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<string> GetValue(string protocolKey)
        {
            List<string> result = new List<string>();
            for (int i = 0; i < Names.Count; i++)
            {
                if (protocolKey.Equals(Names[i], StringComparison.CurrentCultureIgnoreCase))
                {
                    result.Add(Values[i]);
                }
            }
            return result;
        }

        public bool GetValue(string protocolKey, ref short value)
        {
            int index = Names.IndexOf(protocolKey.ToLower());
            if (index > -1)
            {
                return short.TryParse(Values[index], out value);
            }
            else
            {
                return false;
            }
        }

        public bool GetValue(string protocolKey, ref int value)
        {
            int index = Names.IndexOf(protocolKey.ToLower());
            if (index > -1)
            {
                return int.TryParse(Values[index], out value);
            }
            else
            {
                return false;
            }
        }

        public bool GetValue(string protocolKey, ref long value)
        {
            int index = Names.IndexOf(protocolKey.ToLower());
            if (index > -1)
            {
                return long.TryParse(Values[index], out value);
            }
            else
            {
                return false;
            }
        }

        public bool GetValue(string protocolKey, ref Single value)
        {
            int index = Names.IndexOf(protocolKey.ToLower());
            if (index > -1)
            {
                return Single.TryParse(Values[index], out value);
            }
            else
            {
                return false;
            }
        }

        public bool GetValue(string protocolKey, ref Double value)
        {
            int index = Names.IndexOf(protocolKey.ToLower());
            if (index > -1)
            {
                return Double.TryParse(Values[index], out value);
            }
            else
            {
                return false;
            }
        }
    }
}