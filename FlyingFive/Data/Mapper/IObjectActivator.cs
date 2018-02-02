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
                throw new DataAccessException(UtilExceptions.AppendErrorMsg(reader, this._readOrdinal, ex), ex);
            }
        }
    }

    /// <summary>
    /// 实体对象激活器
    /// </summary>
    public class ObjectActivator : IObjectActivator
    {
        private int? _checkNullOrdinal = null;
        private Func<IDataReader, DataReaderOrdinalEnumerator, ObjectActivatorEnumerator, object> _instanceCreator = null;
        private List<int> _readerOrdinals = null;
        private List<IObjectActivator> _objectActivators = null;
        private List<IValueSetter> _memberSetters = null;

        private DataReaderOrdinalEnumerator _readerOrdinalEnumerator = null;
        private ObjectActivatorEnumerator _objectActivatorEnumerator = null;

        public ObjectActivator(Func<IDataReader, DataReaderOrdinalEnumerator, ObjectActivatorEnumerator, object> instanceCreator, List<int> readerOrdinals
            , List<IObjectActivator> objectActivators, List<IValueSetter> memberSetters, int? checkNullOrdinal)
        {
            this._instanceCreator = instanceCreator;
            this._readerOrdinals = readerOrdinals;
            this._objectActivators = objectActivators;
            this._memberSetters = memberSetters;
            this._checkNullOrdinal = checkNullOrdinal;
            this._readerOrdinalEnumerator = new DataReaderOrdinalEnumerator(this._readerOrdinals);
            this._objectActivatorEnumerator = new ObjectActivatorEnumerator(this._objectActivators);
        }

        public object CreateInstance(IDataReader reader)
        {
            if (this._checkNullOrdinal.HasValue)
            {
                if (reader.IsDBNull(this._checkNullOrdinal.Value)) { return null; }
            }
            this._objectActivatorEnumerator.Reset();
            this._readerOrdinalEnumerator.Reset();
            object instance = this._instanceCreator(reader, this._readerOrdinalEnumerator, this._objectActivatorEnumerator);
            foreach (var setter in this._memberSetters)
            {
                setter.SetValue(instance, reader);
            }
            return instance;
        }
    }
}
