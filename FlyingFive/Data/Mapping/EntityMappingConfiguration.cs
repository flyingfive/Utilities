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
    /// 表示实体映射的父类
    /// </summary>
    /// <typeparam name="TEntity">映射的实体类型</typeparam>
    public abstract class EntityMappingConfiguration<TEntity> where TEntity : class,new()
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

        /// <summary>
        /// 映射到表
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="schema">构架名</param>
        protected void Table(string tableName, string schema = "")
        {
            _entityMapping.TableName = tableName;
            if (!string.IsNullOrWhiteSpace(schema)) { _entityMapping.Schema = schema; }
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
            var propMapping = _entityMapping.MemberMappings.Where(m => m.PropertyName.Equals(propName)).SingleOrDefault();
            if (propMapping != null)
            {
                throw new InvalidOperationException(string.Format("存在重复的成员映射：{0}.{1}", typeof(TEntity).FullName, propMapping.PropertyName));
            }
            propMapping = new MemberMapping() { PropertyName = propName, ColumnName = propName, MappingExpression = expression/*, MappingType = dbType*/, EntityMapping = _entityMapping };
            _entityMapping.MemberMappings.Add(propMapping);
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
            var hasConfigured = EntityMappingTable.AllConfigurations.TryGetValue(_entityName, out currentMapping);
            if (hasConfigured)
            {
                EntityMappingTable.AllConfigurations.TryRemove(_entityName, out currentMapping);
            }
            var added = EntityMappingTable.AllConfigurations.TryAdd(_entityName, _entityMapping);
            if (!added) { throw new InvalidOperationException("实体映射添加到配置列表失败!"); }
        }
    }

    /// <summary>
    /// 实体映射列表
    /// </summary>
    public static class EntityMappingTable
    {
        private static object _locker = new object();

        private static readonly ConcurrentDictionary<string, EntityMapping> _allMappings = null;

        private static long _configFlag = 0;
        /// <summary>
        /// 是否配置了实体映射
        /// </summary>
        public static bool HasConfigured { get { return System.Threading.Interlocked.Read(ref _configFlag) > 0; } }

        static EntityMappingTable()
        {
            _allMappings = new ConcurrentDictionary<string, EntityMapping>();
        }

        /// <summary>
        /// 配置实体映射
        /// </summary>
        public static void ConfigureEntityMapping()
        {
            if (HasConfigured) { return; }
            var typeFinder = new AppDomainTypeFinder();
            var mappingTypes = typeFinder.FindClassesOfType(typeof(EntityMappingConfiguration<>));
            foreach (var type in mappingTypes)
            {
                var instance = Activator.CreateInstance(type);
            }
            if (_allMappings.Count > 0)
            {
                System.Threading.Interlocked.Increment(ref _configFlag);
            }
        }

        /// <summary>
        /// 所有实体映射配置(通过类型完全限定名访问)
        /// </summary>
        public static ConcurrentDictionary<string, EntityMapping> AllConfigurations
        {
            get
            {
                return _allMappings;
            }
        }

    }
}
