using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Mapper
{
    /// <summary>
    /// 表示对象创建工具
    /// </summary>
    public interface IObjectActivator
    {
        /// <summary>
        /// 从IDataReader创建一个(映射的)数据对象
        /// </summary>
        /// <param name="reader">IDataReader对象</param>
        /// <returns></returns>
        object CreateInstance(IDataReader reader);
    }


    /// <summary>
    /// 映射字段激活器
    /// </summary>
    public class MappingFieldActivator : IObjectActivator
    {
        private Func<IDataReader, int, object> _creator = null;
        private int _readOrdinal = -1;

        public MappingFieldActivator(Func<IDataReader, int, object> fn, int readOrdinal)
        {
            this._creator = fn;
            this._readOrdinal = readOrdinal;
        }

        public object CreateInstance(IDataReader reader)
        {
            try
            {
                return _creator(reader, _readOrdinal);
            }
            catch (Exception ex)
            {
                return ex;
                //throw new SISSException(ObjectActivator.AppendErrorMsg(reader, this._readOrdinal, ex), ex);
            }
        }
    }

    /// <summary>
    /// 实体对象激活器
    /// </summary>
    public class ObjectActivator : IObjectActivator
    {

        public object CreateInstance(IDataReader reader)
        {
            throw new NotImplementedException();
        }
    }
}
