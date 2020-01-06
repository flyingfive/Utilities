using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace FlyingFive.Comparing
{
    /// <summary>
    /// C#原生值判等
    /// </summary>
    public class ValueEquality : IEqualityComparer
    {
        public bool AreEqual(object left, object right)
        {
            //The primitive types: Boolean, Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, IntPtr, UIntPtr, Char, Double, and Single.
            if (left == null || right == null)
            {
                if (left == null && right == null) { return true; }     //null不参与比较，除非二者都为null
                return false;
            }
            var type1 = left.GetType();
            var type2 = right.GetType();
            if (type1 != type2) { return false; }
            if (type1.IsNullableType())
            {
                return CompareNullableValue(left, right);
            }
            var typeCode = Type.GetTypeCode(type1);
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return (Boolean)Convert.ChangeType(left, typeof(Boolean)) == (Boolean)Convert.ChangeType(right, typeof(Boolean));
                case TypeCode.Byte:
                    return (Byte)Convert.ChangeType(left, typeof(Byte)) == (Byte)Convert.ChangeType(right, typeof(Byte));
                case TypeCode.Char:
                    return (Char)Convert.ChangeType(left, typeof(Char)) == (Char)Convert.ChangeType(right, typeof(Char));
                case TypeCode.DateTime:
                    return (DateTime)Convert.ChangeType(left, typeof(DateTime)) == (DateTime)Convert.ChangeType(right, typeof(DateTime));
                case TypeCode.Decimal:
                    return (Decimal)Convert.ChangeType(left, typeof(Decimal)) == (Decimal)Convert.ChangeType(right, typeof(Decimal));
                case TypeCode.Double:
                    return (Double)Convert.ChangeType(left, typeof(Double)) == (Double)Convert.ChangeType(right, typeof(Double));
                case TypeCode.Int16:
                    return (Int16)Convert.ChangeType(left, typeof(Int16)) == (Int16)Convert.ChangeType(right, typeof(Int16));
                case TypeCode.Int32:
                    return (Int32)Convert.ChangeType(left, typeof(Int32)) == (Int32)Convert.ChangeType(right, typeof(Int32));
                case TypeCode.Int64:
                    return (Int64)Convert.ChangeType(left, typeof(Int64)) == (Int64)Convert.ChangeType(right, typeof(Int64));
                case TypeCode.SByte:
                    return (SByte)Convert.ChangeType(left, typeof(SByte)) == (SByte)Convert.ChangeType(right, typeof(SByte));
                case TypeCode.Single:
                    return (Single)Convert.ChangeType(left, typeof(Single)) == (Single)Convert.ChangeType(right, typeof(Single));
                case TypeCode.UInt16:
                    return (UInt16)Convert.ChangeType(left, typeof(UInt16)) == (UInt16)Convert.ChangeType(right, typeof(UInt16));
                case TypeCode.UInt32:
                    return (UInt32)Convert.ChangeType(left, typeof(UInt32)) == (UInt32)Convert.ChangeType(right, typeof(UInt32));
                case TypeCode.UInt64:
                    return (UInt64)Convert.ChangeType(left, typeof(UInt64)) == (UInt64)Convert.ChangeType(right, typeof(UInt64));
                case TypeCode.String:
                    {
                        var s1 = (String)Convert.ChangeType(left, typeof(String));
                        var s2 = (String)Convert.ChangeType(right, typeof(String));
                        return string.Equals(s1, s2);
                    }
                case TypeCode.DBNull:
                    {
                        if (DBNull.Value.Equals(left) && DBNull.Value.Equals(right)) { return true; }
                        return false;
                    }
                case TypeCode.Object:
                    if (type1 == typeof(Guid))
                    {
                        var id1 = (Guid)Convert.ChangeType(left, typeof(Guid));
                        var id2 = (Guid)Convert.ChangeType(right, typeof(Guid));
                        return id1 == id2;
                    }
                    if (type1 == typeof(System.Numerics.BigInteger))
                    {
                        System.Numerics.BigInteger id1 = 0;
                        System.Numerics.BigInteger id2 = 0;
                        if (!System.Numerics.BigInteger.TryParse(left.ToString(), out id1)) { return false; }
                        if (!System.Numerics.BigInteger.TryParse(right.ToString(), out id2)) { return false; }
                        return id1 == id2;
                    }
                    throw new NotSupportedException(string.Format("不支持的类型：{0}", type1.FullName));
            }
            return false;
        }

        protected bool CompareNullableValue(object left, object right)
        {
            var type = left.GetType().GetUnderlyingType();
            var converter = new NullableConverter(type);
            var typeCode = Type.GetTypeCode(type);
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    {
                        var val1 = (Boolean?)converter.ConvertTo(left, type);
                        var val2 = (Boolean?)converter.ConvertTo(right, type);
                        return val1 == val2;
                    }
                case TypeCode.Byte:
                    {
                        var val1 = (Byte?)converter.ConvertTo(left, type);
                        var val2 = (Byte?)converter.ConvertTo(right, type);
                        return val1 == val2;
                    }
                case TypeCode.Char:
                    {
                        var val1 = (Char?)converter.ConvertTo(left, type);
                        var val2 = (Char?)converter.ConvertTo(right, type);
                        return val1 == val2;
                    }
                case TypeCode.DateTime:
                    {
                        var val1 = (DateTime?)converter.ConvertTo(left, type);
                        var val2 = (DateTime?)converter.ConvertTo(right, type);
                        return val1 == val2;
                    }
                case TypeCode.Decimal:
                    {
                        var val1 = (Decimal?)converter.ConvertTo(left, type);
                        var val2 = (Decimal?)converter.ConvertTo(right, type);
                        return val1 == val2;
                    }
                case TypeCode.Double:
                    {
                        var val1 = (Double?)converter.ConvertTo(left, type);
                        var val2 = (Double?)converter.ConvertTo(right, type);
                        return val1 == val2;
                    }
                case TypeCode.Int16:
                    {
                        var val1 = (Int16?)converter.ConvertTo(left, type);
                        var val2 = (Int16?)converter.ConvertTo(right, type);
                        return val1 == val2;
                    }
                case TypeCode.Int32:
                    {
                        var val1 = (Int32?)converter.ConvertTo(left, type);
                        var val2 = (Int32?)converter.ConvertTo(right, type);
                        return val1 == val2;
                    }
                case TypeCode.Int64:
                    {
                        var val1 = (Int64?)converter.ConvertTo(left, type);
                        var val2 = (Int64?)converter.ConvertTo(right, type);
                        return val1 == val2;
                    }
                case TypeCode.SByte:
                    {
                        var val1 = (SByte?)converter.ConvertTo(left, type);
                        var val2 = (SByte?)converter.ConvertTo(right, type);
                        return val1 == val2;
                    }
                case TypeCode.Single:
                    {
                        var val1 = (Single?)converter.ConvertTo(left, type);
                        var val2 = (Single?)converter.ConvertTo(right, type);
                        return val1 == val2;
                    }
                case TypeCode.UInt16:
                    {
                        var val1 = (UInt16?)converter.ConvertTo(left, type);
                        var val2 = (UInt16?)converter.ConvertTo(right, type);
                        return val1 == val2;
                    }
                case TypeCode.UInt32:
                    {
                        var val1 = (UInt32?)converter.ConvertTo(left, type);
                        var val2 = (UInt32?)converter.ConvertTo(right, type);
                        return val1 == val2;
                    }
                case TypeCode.UInt64:
                    {
                        var val1 = (UInt64?)converter.ConvertTo(left, type);
                        var val2 = (UInt64?)converter.ConvertTo(right, type);
                        return val1 == val2;
                    }
                case TypeCode.Object:
                    if (type == typeof(Guid))
                    {
                        var val1 = (Guid?)converter.ConvertTo(left, type);
                        var val2 = (Guid?)converter.ConvertTo(right, type);
                        return val1 == val2;
                    }
                    if (type == typeof(System.Numerics.BigInteger))
                    {
                        var val1 = (System.Numerics.BigInteger?)converter.ConvertTo(left, type);
                        var val2 = (System.Numerics.BigInteger?)converter.ConvertTo(right, type);
                        return val1 == val2;
                    }
                    throw new NotSupportedException(string.Format("不支持的类型：{0}", type.FullName));
            }
            return false;
        }
    }
}
