using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Comparing
{
    /// <summary>
    /// 数组对象判等（对比数组每个元素）
    /// </summary>
    public class ArrayEquality : IEqualityComparer
    {
        public bool AreEqual(object left, object right)
        {
            if (left == null || right == null)
            {
                if (left == null && right == null) { return true; }     //null不参与比较，除非二者都为null
                return false;                                           //二者其一为null，另一个非null
            }
            if (left.GetType() != right.GetType()) { return false; }
            if (!left.GetType().IsArray || !right.GetType().IsArray)    //二者其一不是数组
            {
                return false;
            }
            var arr1 = left as Array;
            var arr2 = right as Array;
            if (arr1.Length != arr2.Length) { return false; }
            var leftElementType = left.GetType().GetElementType();      //数组元素类型不等
            var rightElementType = right.GetType().GetElementType();
            if (leftElementType != rightElementType) { return false; }
            var comparer = ComparerFactory.CreateObjectEquality(leftElementType);
            for (int i = 0; i < arr1.Length; i++)
            {
                var obj1 = arr1.GetValue(i);
                var obj2 = arr2.GetValue(i);
                if (!comparer.AreEqual(obj1, obj2)) { return false; }
            }
            return true;
        }
    }
}
