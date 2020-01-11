﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlyingFive;

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
        ///// <summary>
        ///// 名称
        ///// </summary>
        //public List<string> Names { get; private set; }
        ///// <summary>
        ///// 值
        ///// </summary>
        //public List<string> Values { get; private set; }

        public IDictionary<String, string> Data { get; private set; }

        public IncomingDataParser()
        {
            //Names = new List<string>();
            //Values = new List<string>();
            Data = new Dictionary<string, string>();
        }

        public bool DecodeProtocolText(string protocolText)
        {
            Header = "";
            Data.Clear();
            //Names.Clear();
            //Values.Clear();
            var index = protocolText.IndexOf(ProtocolKey.ReturnWrap);
            if (index < 0)
            {
                return false;
            }
            else
            {
                var keyValues = protocolText.Split(new string[] { ProtocolKey.ReturnWrap }, StringSplitOptions.RemoveEmptyEntries);
                if (keyValues.Length < 2) //每次命令至少包括两行
                {
                    return false;
                }
                foreach (var item in keyValues)
                {
                    var config = item.Split(new string[] { ProtocolKey.EqualSign }, StringSplitOptions.None);
                    if (config.Length < 1 || config.Length > 2) { return false; }
                    //if (config.Length == 1)
                    //{
                    //    Command = config.First();
                    //}
                    //if (config.Length != 2) { return false; }
                    if (config.First().Equals(ProtocolKey.Command, StringComparison.CurrentCultureIgnoreCase))
                    {
                        Command = config[1];
                    }
                    else
                    {
                        if (config.Length == 2) { Data.Add(config.First(), config.Last()); }
                        //Names.Add(tmpStr[0].ToLower());
                        //Values.Add(tmpStr[1]);
                    }
                }
                return true;
            }
        }

        public bool GetValue(string protocolKey, ref string value)
        {
            if (Data.ContainsKey(protocolKey))
            {
                value = Data[protocolKey];
                return true;
            }
            return false;
        }

        public List<string> GetValue(string protocolKey)
        {
            List<string> result = new List<string>();
            //for (int i = 0; i < Names.Count; i++)
            //{
            //    if (protocolKey.Equals(Names[i], StringComparison.CurrentCultureIgnoreCase))
            //    {
            //        result.Add(Values[i]);
            //    }
            //}
            return result;
        }

        public bool GetValue(string protocolKey, ref short value)
        {
            if (Data.ContainsKey(protocolKey))
            {
                value = Data[protocolKey].TryConvert<short>();
                return true;
            }
            return false;
        }

        public bool GetValue(string protocolKey, ref int value)
        {
            if (Data.ContainsKey(protocolKey))
            {
                value = Data[protocolKey].TryConvert<int>();
                return true;
            }
            return false;
        }

        public bool GetValue(string protocolKey, ref long value)
        {
            if (Data.ContainsKey(protocolKey))
            {
                value = Data[protocolKey].TryConvert<long>();
                return true;
            }
            return false;
        }
    }
}