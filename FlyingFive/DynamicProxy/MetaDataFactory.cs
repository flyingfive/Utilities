﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FlyingFive.DynamicProxy
{
    /// <summary>
    /// 代理过的接口类型元数据
    /// </summary>
    public class MetaDataFactory
    {
        private static Hashtable typeMap = new Hashtable();

        private MetaDataFactory()
        {
        }

        /// <summary>
        /// 添加代理了的接口信息
        /// </summary>
        /// <param name="interfaceType"></param>
        public static void Add(Type interfaceType)
        {
            if (interfaceType != null)
            {
                lock (typeMap.SyncRoot)
                {
                    if (!typeMap.ContainsKey(interfaceType.FullName))
                    {
                        typeMap.Add(interfaceType.FullName, interfaceType);
                    }
                }
            }
        }

        /// <summary>
        /// 获取接口方法信息的方法
        /// </summary>
        public static readonly MethodInfo GetInterfaceMethod = typeof(MetaDataFactory).GetMethod("GetMethod", new Type[] { typeof(string), typeof(int) });

        /// <summary>
        /// 根据索引位置返回接口方法信息
        /// </summary>
        /// <param name="interfaceName">接口全名</param>
        /// <param name="index">方法位置索引</param>
        /// <returns></returns>
        public static MethodInfo GetMethod(string interfaceName, int index)
        {
            Type type = null;
            lock (typeMap.SyncRoot)
            {
                if (!typeMap.ContainsKey(interfaceName))
                {
                    throw new NotSupportedException(string.Format("不支持的接口：{0}", interfaceName));
                }
                type = (Type)typeMap[interfaceName];
            }
            var methods = type.GetMethods();
            if (index < methods.Length)
            {
                return methods[index];
            }
            throw new IndexOutOfRangeException(string.Format("指定接口{0}的位置{1}中不存在方法信息。", interfaceName, index));
        }
    }
}
