using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace FlyingFive.Data.Emit
{
    /// <summary>
    /// MSIL指令助手
    /// </summary>
    public static class EmitHelper
    {
        /// <summary>
        /// 使用MSIL指令将当前堆栈上的值赋给成员对象
        /// </summary>
        /// <param name="il">MSIL指令</param>
        /// <param name="member">成员对象(属性或字段)</param>
        public static void SetValueIL(ILGenerator il, MemberInfo member)
        {
            MemberTypes memberType = member.MemberType;
            if (memberType == MemberTypes.Property)
            {
                MethodInfo setter = ((PropertyInfo)member).GetSetMethod();
                il.EmitCall(OpCodes.Callvirt, setter, null);//给属性赋值
            }
            else if (memberType == MemberTypes.Field)
            {
                il.Emit(OpCodes.Stfld, ((FieldInfo)member));//给字段赋值
            }
            else
            {
                throw new NotSupportedException("不支持字段或属性外的其它成员操作");
            }
        }
    }
}
