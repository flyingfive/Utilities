using FlyingFive.Data.DbExpressions;
using FlyingFive.Data.Descriptors;
using FlyingFive.Data.Infrastructure;
using FlyingFive.Data.Kernel;
using FlyingFive.Data.Query;
using FlyingFive.Data.Query.Internals;
using FlyingFive.Data.Schema;
using FlyingFive.Data.Visitors;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace FlyingFive.Data
{
    /// <summary>
    /// 表示DB上下文
    /// </summary>
    public abstract partial class DbContext : IDbContext
    {
        private bool _disposed = false;
        public IDbSession Session { get; private set; }
        private CommonAdoSession _commonSession = null;
        internal Dictionary<Type, TrackEntityCollection> TrackingEntityContainer
        {
            get;
            private set;
        }
        /// <summary>
        /// 上下文环境供应者(提供上下文所需要的DB连接以及表达式翻译等功能)
        /// </summary>
        public abstract IDbContextServiceProvider DbContextServiceProvider { get; }

        /// <summary>
        /// 通用会话
        /// </summary>
        public CommonAdoSession CommonSession
        {
            get
            {
                this.CheckDisposed();
                if (this._commonSession == null)
                    this._commonSession = new CommonAdoSession(this.DbContextServiceProvider.CreateConnection());
                return this._commonSession;
            }
        }

        protected DbContext()
        {
            this.Session = new DbSession(this);
            this.TrackingEntityContainer = new Dictionary<Type, TrackEntityCollection>();
        }

        public virtual IQuery<TEntity> Query<TEntity>()
        {
            return new Query<TEntity>(this, "");
        }

        public TEntity QueryByKey<TEntity>(object key, bool tracking = false)
        {
            Expression<Func<TEntity, bool>> predicate = BuildPredicate<TEntity>(key);
            var q = this.Query<TEntity>().Where(predicate);

            if (tracking)
                q = q.AsTracking();

            return q.FirstOrDefault();
        }

        public IEnumerable<T> SqlQuery<T>(string plainSql, CommandType cmdType = CommandType.Text, params FakeParameter[] parameters)
        {
            if (string.IsNullOrEmpty(plainSql)) { throw new ArgumentException("必需提供参数：plainSql"); }
            return new InternalSqlQuery<T>(this, plainSql, cmdType, parameters);
        }

        public TEntity Insert<TEntity>(TEntity entity)
        {
            //Utils.CheckNull(entity);

            var typeDescriptor = EntityTypeDescriptor.GetEntityTypeDescriptor(entity.GetType());

            Dictionary<MappingMemberDescriptor, object> keyValueMap = CreateKeyValueMap(typeDescriptor);
            MappingMemberDescriptor identityMemberDescriptor = typeDescriptor.AutoIncrement;

            Dictionary<MappingMemberDescriptor, DbExpression> insertColumns = new Dictionary<MappingMemberDescriptor, DbExpression>();
            foreach (var kv in typeDescriptor.MappingMemberDescriptors)
            {
                MappingMemberDescriptor memberDescriptor = kv.Value;

                if (memberDescriptor.Column.IsIdentity) { continue; }
                //if (memberDescriptor == autoIncrementMemberDescriptor)
                //    continue;

                object val = memberDescriptor.GetValue(entity);

                if (keyValueMap.ContainsKey(memberDescriptor))
                {
                    keyValueMap[memberDescriptor] = val;
                }
                DbExpression valExp = DbExpression.Parameter(val, memberDescriptor.MemberInfoType);
                insertColumns.Add(memberDescriptor, valExp);
            }

            var nullValueKey = keyValueMap.Where(a => a.Value == null && a.Key.Column.IsIdentity).Select(a => a.Key).FirstOrDefault();
            if (nullValueKey != null)
            {
                throw new DataAccessException(string.Format("主键 '{0}' 不能为NULL.", nullValueKey.MemberInfo.Name));
            }

            DbTable dbTable = typeDescriptor.Table;//table == null ? typeDescriptor.Table : new DbTable(table, typeDescriptor.Table.Schema);
            DbInsertExpression e = new DbInsertExpression(dbTable);

            foreach (var kv in insertColumns)
            {
                e.InsertColumns.Add(kv.Key.Column, kv.Value);
            }

            if (identityMemberDescriptor == null)
            {
                this.ExecuteSqlCommand(e);
                return entity;
            }

            IDbExpressionTranslator translator = this.DbContextServiceProvider.CreateDbExpressionTranslator();
            List<FakeParameter> parameters = null;
            string sql = translator.Translate(e, out parameters);

            sql = string.Concat(sql, ";", this.GetSelectLastIdentityIdClause());

            object identityVal = this.Session.ExecuteScalar(sql, CommandType.Text, parameters.ToArray());

            if (identityVal == null || identityVal == DBNull.Value)
            {
                throw new DataAccessException("获取数据库标识ID失败!");
            }

            identityVal = identityVal.TryConvert(identityMemberDescriptor.MemberInfoType);
            identityMemberDescriptor.SetValue(entity, identityVal);
            return entity;
        }

        public int Update<TEntity>(TEntity entity)
        {
            UtilExceptions.CheckNull(entity);

            EntityTypeDescriptor typeDescriptor = EntityTypeDescriptor.GetEntityTypeDescriptor(entity.GetType());
            EnsureEntityHasPrimaryKey(typeDescriptor);

            Dictionary<MappingMemberDescriptor, object> keyValueMap = CreateKeyValueMap(typeDescriptor);

            IEntityState entityState = this.TryGetTrackedEntityState(entity);
            Dictionary<MappingMemberDescriptor, DbExpression> updateColumns = new Dictionary<MappingMemberDescriptor, DbExpression>();
            foreach (var kv in typeDescriptor.MappingMemberDescriptors)
            {
                MemberInfo member = kv.Key;
                MappingMemberDescriptor memberDescriptor = kv.Value;

                if (keyValueMap.ContainsKey(memberDescriptor))
                {
                    keyValueMap[memberDescriptor] = memberDescriptor.GetValue(entity);
                    continue;
                }

                if (memberDescriptor.Column.IsIdentity) { continue; }

                object val = memberDescriptor.GetValue(entity);

                if (entityState != null && !entityState.HasChanged(memberDescriptor, val)) { continue; }

                DbExpression valExp = DbExpression.Parameter(val, memberDescriptor.MemberInfoType);
                updateColumns.Add(memberDescriptor, valExp);
            }

            if (updateColumns.Count == 0)
                return 0;

            DbTable dbTable = typeDescriptor.Table;//table == null ? typeDescriptor.Table : new DbTable(table, typeDescriptor.Table.Schema);
            DbExpression conditionExp = MakeCondition(keyValueMap, dbTable);
            DbUpdateExpression e = new DbUpdateExpression(dbTable, conditionExp);

            foreach (var item in updateColumns)
            {
                e.UpdateColumns.Add(item.Key.Column, item.Value);
            }

            int ret = this.ExecuteSqlCommand(e);
            if (entityState != null)
                entityState.Refresh();
            return ret;
        }

        public int Update<TEntity>(Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, TEntity>> content)
        {
            UtilExceptions.CheckNull(condition);
            UtilExceptions.CheckNull(content);

            EntityTypeDescriptor typeDescriptor = EntityTypeDescriptor.GetEntityTypeDescriptor(typeof(TEntity));

            Dictionary<MemberInfo, Expression> updateColumns = InitMemberExtractor.Extract(content);

            DbTable explicitDbTable = null;
            DefaultExpressionParser expressionParser = typeDescriptor.GetExpressionParser(explicitDbTable);

            DbExpression conditionExp = expressionParser.ParseFilterPredicate(condition);

            DbUpdateExpression e = new DbUpdateExpression(explicitDbTable ?? typeDescriptor.Table, conditionExp);

            foreach (var kv in updateColumns)
            {
                MemberInfo key = kv.Key;
                MappingMemberDescriptor memberDescriptor = typeDescriptor.TryGetMappingMemberDescriptor(key);

                if (memberDescriptor == null)
                    throw new DataAccessException(string.Format("The member '{0}' does not map any column.", key.Name));

                if (memberDescriptor.Column.IsPrimaryKey)
                    throw new DataAccessException(string.Format("Could not update the primary key '{0}'.", memberDescriptor.Column.Name));

                if (memberDescriptor.Column.IsIdentity)
                    throw new DataAccessException(string.Format("Could not update the identity column '{0}'.", memberDescriptor.Column.Name));

                e.UpdateColumns.Add(memberDescriptor.Column, expressionParser.Parse(kv.Value));
            }

            if (e.UpdateColumns.Count == 0)
                return 0;

            return this.ExecuteSqlCommand(e);
        }

        public int Delete<TEntity>(TEntity entity)
        {
            UtilExceptions.CheckNull(entity);

            var typeDescriptor = EntityTypeDescriptor.GetEntityTypeDescriptor(entity.GetType());
            EnsureEntityHasPrimaryKey(typeDescriptor);

            Dictionary<MappingMemberDescriptor, object> keyValueMap = new Dictionary<MappingMemberDescriptor, object>();

            foreach (MappingMemberDescriptor keyMemberDescriptor in typeDescriptor.PrimaryKeys)
            {
                object keyVal = keyMemberDescriptor.GetValue(entity);
                keyValueMap.Add(keyMemberDescriptor, keyVal);
            }

            DbTable dbTable = typeDescriptor.Table;//table == null ? typeDescriptor.Table : new DbTable(table, typeDescriptor.Table.Schema);
            DbExpression conditionExp = MakeCondition(keyValueMap, dbTable);
            DbDeleteExpression e = new DbDeleteExpression(dbTable, conditionExp);
            return this.ExecuteSqlCommand(e);
        }

        public int Delete<TEntity>(Expression<Func<TEntity, bool>> condition)
        {
            UtilExceptions.CheckNull(condition);

            var typeDescriptor = EntityTypeDescriptor.GetEntityTypeDescriptor(typeof(TEntity));

            DefaultExpressionParser expressionParser = typeDescriptor.GetExpressionParser(typeDescriptor.Table);
            DbExpression conditionExp = expressionParser.ParseFilterPredicate(condition);

            DbDeleteExpression e = new DbDeleteExpression(typeDescriptor.Table, conditionExp);

            return this.ExecuteSqlCommand(e);
        }

        public int DeleteByKey<TEntity>(object key)
        {
            Expression<Func<TEntity, bool>> predicate = BuildPredicate<TEntity>(key);
            return this.Delete<TEntity>(predicate);
        }

        public void TrackEntity(object entity)
        {
            UtilExceptions.CheckNull(entity);
            Type entityType = entity.GetType();
            if (entityType.IsAnonymousType()) { return; }
            var entityContainer = this.TrackingEntityContainer;
            TrackEntityCollection collection;
            if (!entityContainer.TryGetValue(entityType, out collection))
            {
                var typeDescriptor = EntityTypeDescriptor.GetEntityTypeDescriptor(entityType);

                if (!typeDescriptor.HasPrimaryKeys) { return; }
                collection = new TrackEntityCollection(typeDescriptor);
                entityContainer.Add(entityType, collection);
            }
            collection.TryAddEntity(entity);
        }

        public void Dispose()
        {
            if (this._disposed)
                return;

            if (this._commonSession != null)
                this._commonSession.Dispose();
            this.Dispose(true);
            this._disposed = true;
        }

        protected virtual void Dispose(bool disposing)
        {

        }

        protected void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(string.Format("无法访问已释放的对象:{0}", this.GetType().FullName));
            }
        }


        private int ExecuteSqlCommand(DbExpression e)
        {
            var translator = this.DbContextServiceProvider.CreateDbExpressionTranslator();
            List<FakeParameter> parameters = null;
            string cmdText = translator.Translate(e, out parameters);
            int r = this.CommonSession.ExecuteNonQuery(cmdText, CommandType.Text, parameters.ToArray());
            return r;
        }


        /// <summary>
        /// 获取选择最后插入的标识ID语句
        /// </summary>
        /// <returns></returns>
        public abstract string GetSelectLastIdentityIdClause();

        protected virtual IEntityState TryGetTrackedEntityState(object entity)
        {
            UtilExceptions.CheckNull(entity);
            Type entityType = entity.GetType();
            Dictionary<Type, TrackEntityCollection> entityContainer = this.TrackingEntityContainer;

            if (entityContainer == null)
                return null;

            TrackEntityCollection collection;
            if (!entityContainer.TryGetValue(entityType, out collection))
            {
                return null;
            }

            IEntityState ret = collection.TryGetEntityState(entity);
            return ret;
        }


        internal class TrackEntityCollection
        {
            public TrackEntityCollection(EntityTypeDescriptor typeDescriptor)
            {
                this.TypeDescriptor = typeDescriptor;
                this.Entities = new Dictionary<object, IEntityState>(1);
            }
            public EntityTypeDescriptor TypeDescriptor { get; private set; }
            public Dictionary<object, IEntityState> Entities { get; private set; }
            public bool TryAddEntity(object entity)
            {
                if (this.Entities.ContainsKey(entity))
                {
                    return false;
                }

                IEntityState entityState = new EntityState(this.TypeDescriptor, entity);
                this.Entities.Add(entity, entityState);

                return true;
            }
            public IEntityState TryGetEntityState(object entity)
            {
                IEntityState ret;
                if (!this.Entities.TryGetValue(entity, out ret))
                    ret = null;

                return ret;
            }
        }
    }
}
