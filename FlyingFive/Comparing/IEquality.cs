using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace FlyingFive.Comparing
{
    /// <summary>
    /// 对象判等比较器
    /// </summary>
    public interface IEqualityComparer
    {
        /// <summary>
        /// 比较两个对象是否相等
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        bool AreEqual(object left, object right);
    }


    /// <summary>
    /// 比较器工厂(每种比较器都是静态单一实例，多线程或异步环境下要考虑冲突)
    /// </summary>
    public static class ComparerFactory
    {
        /// <summary>
        /// 支持比较的原生数据类型名称
        /// </summary>
        public static readonly string[] SupportedTypeName = Enum.GetNames(typeof(TypeCode)).Where(c => "Empty,Object,DBNull".IndexOf(c) < 0).Concat(new string[] { typeof(Guid).Name, typeof(BigInteger).Name }).ToArray();

        /// <summary>
        /// 创建类型比较器
        /// </summary>
        /// <param name="dataType">数据类型</param>
        /// <returns></returns>
        public static IEqualityComparer CreateObjectEquality(Type dataType)
        {
            if (dataType.IsArray && dataType.GetArrayRank() == 1)
            {
                return new ArrayEquality();
            }
            //泛型类型只支持泛型集合或可空类型
            if (dataType.IsGenericType && dataType.GetGenericArguments().Count() == 1)
            {
                if (dataType.IsGenericListType())
                {
                    return new ListEquality();
                }
                if (dataType.IsNullableType())
                {
                    return new ValueEquality();
                }
            }
            if (SupportedTypeName.Contains(dataType.Name))
            {
                return new ValueEquality();
            }
            else
            {
                var code = Type.GetTypeCode(dataType);
                if (Type.GetTypeCode(dataType) == TypeCode.Object)
                {
                    return new ValueEquality();
                }
            }
            throw new NotSupportedException("不支持的类型比较，支持的类型包括：内置数据类型、类、集合以及一维数组类型。");
        }
    }
}
