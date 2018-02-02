using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Infrastructure
{
    /// <summary>
    /// 输出显示查询信息
    /// </summary>
    public class DisplayQueryInfo
    {

        /// <summary>
        /// 获取执行命令的显示信息
        /// </summary>
        /// <param name="cmdText">执行的命令语句</param>
        /// <param name="parameters">命令中的参数列表</param>
        /// <returns></returns>
        public static string GetCommandDisplayInfo(string cmdText, FakeParameter[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(cmdText);
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    if (param == null)
                    {
                        continue;
                    }
                    string typeName = null;
                    object value = null;
                    Type parameterType;
                    if (param.Value == null || param.Value == DBNull.Value)
                    {
                        parameterType = param.DataType;
                        value = "NULL";
                    }
                    else
                    {
                        value = param.Value;
                        parameterType = param.Value.GetType();
                        if (parameterType == typeof(string) || parameterType == typeof(DateTime))
                        {
                            value = string.Format("'{0}'", value);
                        }
                    }
                    if (parameterType != null) { typeName = GetTypeDisplayName(parameterType); }
                    sb.AppendFormat("{0} {1} = {2};", typeName, param.Name, value);
                    //sb.AppendLine();
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 获取类型的显示名称
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetTypeDisplayName(Type type)
        {
            Type underlyingType = null;
            if (type.IsNullable(out underlyingType))
            {
                return string.Format("Nullable<{0}>", GetTypeDisplayName(underlyingType));
            }
            else
            {
                return type.Name;
            }
        }

    }
}
