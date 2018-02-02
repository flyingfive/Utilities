using FlyingFive.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace FlyingFive.Data.Mapping
{
    /// <summary>
    /// 表示一个实体的映射配置
    /// </summary>
    /// <typeparam name="TEntity">配置的实体类型</typeparam>
    public abstract class EntityMappingConfiguration<TEntity> where TEntity : class, new()
    {
        private string _entityName = null;
        /// <summary>
        /// 实体映射
        /// </summary>
        private EntityMapping _entityMapping = null;

        public EntityMappingConfiguration()
        {
            _entityName = typeof(TEntity).FullName;
            _entityMapping = new EntityMapping() { EntityType = typeof(TEntity) };
        }

        protected void Table(string tableName)
        {
            _entityMapping.TableName = tableName;
        }

        protected void Table(string tableName, string schema)
        {
            _entityMapping.TableName = tableName;
            _entityMapping.Schema = schema;
        }

        /// <summary>
        /// 映射属性
        /// </summary>
        /// <typeparam name="TProperty">属性类型</typeparam>
        /// <param name="propertyExpression">要映射的属性表达式</param>
        /// <returns></returns>
        protected MemberMapping Property<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
        {
            var expression = propertyExpression.Body as System.Linq.Expressions.MemberExpression;
            if (propertyExpression == null || expression == null) { throw new ArgumentException("参数:propertyExpression不是有效的值!"); }
            var propName = expression.Member.Name;
            //var dbType = typeof(TProperty).ToSqlDbType();//expression.Type.ToSqlDbType();
            var propMapping = _entityMapping.PropertyMappings.Where(m => m.PropertyName.Equals(propName)).SingleOrDefault();
            if (propMapping != null)
            {
                throw new InvalidOperationException(string.Format("已经存在实体成员:{0}.{1}的映射,不能重复映射.", typeof(TEntity).FullName, propName));
            }
            propMapping = new MemberMapping() { PropertyName = propName, ColumnName = propName, EntityMapping = _entityMapping };
            _entityMapping.PropertyMappings.Add(propMapping);
            return propMapping;
        }

        /// <summary>
        /// 将配置添加到全局实体映射集合
        /// </summary>
        protected void AddToEntityMapping()
        {
            if (_entityMapping == null) { throw new InvalidOperationException(string.Format("还没有初始化:{0}的实体映射", _entityName)); }
            _entityMapping.FinalPass();
            EntityMapping currentMapping = null;
            if (EntityMappingTable.AllMappings.TryGetValue(_entityName, out currentMapping))
            {
                var flag = EntityMappingTable.AllMappings.TryRemove(_entityName, out currentMapping);
            }
            var added = EntityMappingTable.AllMappings.TryAdd(_entityName, _entityMapping);
            if (!added) { throw new InvalidOperationException("映射配置添加失败"); }
        }
    }

    /// <summary>
    /// 实体映射配置表
    /// </summary>
    public static class EntityMappingTable
    {
        private static object _locker = new object();

        private static readonly ConcurrentDictionary<string, EntityMapping> _allMappings = null;

        private static long _configFlag = 0;
        /// <summary>
        /// 是否有完成实体配置工作
        /// </summary>
        public static bool HasEntityConfigured { get { return System.Threading.Interlocked.Read(ref _configFlag) > 0; } }

        static EntityMappingTable()
        {
            _allMappings = new ConcurrentDictionary<string, EntityMapping>();
            if (!HasEntityConfigured)
            {
                ConfigureEntityMapping();
            }
        }

        /// <summary>
        /// 配置实体映射
        /// </summary>
        public static void ConfigureEntityMapping()
        {
            if (HasEntityConfigured) { return; }
            var typeFinder = new AppDomainTypeFinder();
            var mappingTypes = typeFinder.FindClassesOfType(typeof(EntityMappingConfiguration<>));
            foreach (var type in mappingTypes)
            {
                var instance = Activator.CreateInstance(type);
            }
            System.Threading.Interlocked.Increment(ref _configFlag);
        }

        /// <summary>
        /// 所有实体映射(通过类型完全限定名访问)
        /// </summary>
        public static new ConcurrentDictionary<string, EntityMapping> AllMappings
        {
            get
            {
                return _allMappings;
            }
        }
    }
}
