using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlyingFive.Data.Emit;

namespace FlyingFive.Comparing
{
    /// <summary>
    /// Object对象判等（对比所有可读属性）
    /// </summary>
    public class ClassEquality : IEqualityComparer
    {
        public bool AreEqual(object left, object right)
        {
            if (left == null || right == null)
            {
                if (left == null && right == null) { return true; }     //null不参与比较，除非二者都为null
                return false;
            }
            if (left.GetType() != right.GetType()) { return false; }
            var properties = left.GetType().GetProperties().Where(p => p.CanRead).Select(p => new { prop = p, getter = DelegateGenerator.CreateValueGetter(p) });
            foreach (var item in properties)
            {
                var val1 = item.getter(left);
                var val2 = item.getter(right);
                var comparer = ComparerFactory.CreateObjectEquality(item.prop.PropertyType);
                if (!comparer.AreEqual(val1, val2)) { return false; }
            }
            return true;
        }
    }
}
