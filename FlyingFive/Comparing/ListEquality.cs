using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Comparing
{
    /// <summary>
    /// 泛型集合判等（对比集合每个元素）
    /// </summary>
    public class ListEquality : IEqualityComparer
    {
        public bool AreEqual(object left, object right)
        {
            if (left == null || right == null)
            {
                if (left == null && right == null) { return true; }     //null不参与比较，除非二者都为null
                return false;                                           //二者其一为null，另一个非null
            }
            if (left.GetType() != right.GetType()) { return false; }
            if (!left.GetType().IsListType() || !right.GetType().IsListType()) { return false; }
            var leftDataType = left.GetType().GetGenericArguments().First();
            var rightDataType = right.GetType().GetGenericArguments().First();
            if (leftDataType != rightDataType) { return false; }
            var list1 = left as System.Collections.IList;
            var list2 = right as System.Collections.IList;
            if (list1 == null || list2 == null)
            {
                return false;
            }
            if (list1.Count != list2.Count) { return false; }
            var comparer = ComparerFactory.CreateObjectEquality(leftDataType);
            for (int i = 0; i < list1.Count; i++)
            {
                var obj1 = list1[i];
                var obj2 = list2[i];
                if (!comparer.AreEqual(obj1, obj2)) { return false; }
            }
            return true;
        }
    }
}
