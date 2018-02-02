using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FlyingFive.Data.Mapper
{
    /// <summary>
    /// 表示从DataReader为对象设置值的赋值器
    /// </summary>
    public interface IValueSetter
    {
        /// <summary>
        /// 从DataReader为对象赋值
        /// </summary>
        /// <param name="obj">要赋值的对象</param>
        /// <param name="reader">一个DataReader对象</param>
        void SetValue(object obj, IDataReader reader);
    }

    /// <summary>
    /// 映射成员绑定器
    /// </summary>
    public class MappingMemberBinder : IValueSetter
    {
        private IMemberMapper _memberMapper = null;
        /// <summary>
        /// 读取DataReader的顺序
        /// </summary>
        public int Ordinal { get; private set; }
        /// <summary>
        /// 映射的成员
        /// </summary>
        public MemberInfo Member { get; private set; }

        public MappingMemberBinder(MemberInfo member, IMemberMapper memberMapper, int ordinal)
        {
            this.Member = member;
            this._memberMapper = memberMapper;
            this.Ordinal = ordinal;
        }


        public void SetValue(object obj, IDataReader reader)
        {
            try
            {
                this._memberMapper.Map(obj, reader, this.Ordinal);
            }
            catch (DataAccessException ex)
            {
                throw new DataAccessException(string.Format("成员{0}+{1}值绑定失败: {2}", Member.DeclaringType.FullName, Member.Name, ex.Message), ex);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    /// <summary>
    /// 复杂成员绑定
    /// </summary>
    public class ComplexMemberBinder : IValueSetter
    {
        private Action<object, object> _setter;
        private IObjectActivator _activtor;
        public ComplexMemberBinder(Action<object, object> setter, IObjectActivator activtor)
        {
            this._setter = setter;
            this._activtor = activtor;
        }
        public void SetValue(object obj, IDataReader reader)
        {
            object val = this._activtor.CreateInstance(reader);
            this._setter(obj, val);
        }
    }
}
