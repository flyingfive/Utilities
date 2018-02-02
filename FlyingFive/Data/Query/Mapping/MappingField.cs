using FlyingFive.Data.Mapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Query.Mapping
{
    /// <summary>
    /// 查询中的映射字段
    /// </summary>
    public class MappingField : IObjectActivatorCreator
    {
        private Type _type = null;
        public MappingField(Type type, int readerOrdinal)
        {
            this._type = type;
            this.ReaderOrdinal = readerOrdinal;
        }

        /// <summary>
        /// 读取器顺序位置
        /// </summary>
        public int ReaderOrdinal { get; private set; }
        public int? CheckNullOrdinal { get; set; }

        public IObjectActivator CreateObjectActivator()
        {
            var activator = this.CreateObjectActivator(null);
            return activator;
        }
        public IObjectActivator CreateObjectActivator(IDbContext dbContext)
        {
            Func<IDataReader, int, object> fn = MappingTypeConstructor.GetInstance(this._type).DataCreator;
            var activator = new MappingFieldActivator(fn, this.ReaderOrdinal);
            return activator;
        }
    }
}
