using FlyingFive.Data.Emit;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FlyingFive.Data.Mapper
{
    /// <summary>
    /// 表示成员映射器
    /// </summary>
    public interface IMemberMapper
    {
        /// <summary>
        /// 根据映射从DataReader赋值给实体对象的属性
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="reader"></param>
        /// <param name="ordinal"></param>
        void Map(object instance, IDataReader reader, int ordinal);
    }

    public static class MemberMapperHelper
    {
        /// <summary>
        /// 创建成员映射器对象
        /// </summary>
        /// <param name="member">成员对象</param>
        /// <returns></returns>
        public static IMemberMapper CreateMemberMapper(MemberInfo member)
        {
            Type type = DynamicClassGenerator.CreateMemberMapperClass(member);
            //IMRM obj = (IMRM)type.GetConstructor(Type.EmptyTypes).Invoke(null);
            var mrm = Activator.CreateInstance(type) as IMemberMapper;
            return mrm;
            //return obj;
        }

    }
}
