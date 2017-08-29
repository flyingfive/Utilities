using FlyingFive.Data.Descriptors;
using FlyingFive.Data.Infrastructure;
using FlyingFive.Data.Mapper;
using FlyingFive.Data.Mapping;
using FlyingFive.Data.Query.Mapping;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace FlyingFive.Data.Query.Internals
{
    /// <summary>
    /// 内部纯SQL查询
    /// </summary>
    internal class InternalSqlQuery<T> : IEnumerable<T>, IEnumerable
    {
        private string _sql = null;
        private DbContext _dbContext = null;
        private CommandType _cmdType = CommandType.Text;
        private FakeParameter[] _parameters = null;

        public InternalSqlQuery(DbContext dbContext, string sql, CommandType cmdType, FakeParameter[] parameters)
        {
            this._dbContext = dbContext;
            this._sql = sql;
            this._cmdType = cmdType;
            this._parameters = parameters;
        }


        public IEnumerator<T> GetEnumerator()
        {
            return new QueryEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }


        struct QueryEnumerator : IEnumerator<T>
        {
            private IDataReader _reader;
            private InternalSqlQuery<T> _internalSqlQuery;
            private IObjectActivator _objectActivator;
            private T _current;
            private bool _hasFinished;
            private bool _disposed;

            public QueryEnumerator(InternalSqlQuery<T> internalSqlQuery)
            {
                this._internalSqlQuery = internalSqlQuery;
                this._reader = null;
                this._objectActivator = null;

                this._current = default(T);
                this._hasFinished = false;
                this._disposed = false;
            }

            public T Current { get { return this._current; } }

            object IEnumerator.Current { get { return this._current; } }

            public bool MoveNext()
            {
                if (this._hasFinished || this._disposed)
                    return false;

                if (this._reader == null)
                {
                    this.Prepare();
                }

                if (this._reader.Read())
                {
                    this._current = (T)this._objectActivator.CreateInstance(this._reader);
                    return true;
                }
                else
                {
                    this._reader.Close();
                    this._current = default(T);
                    this._hasFinished = true;
                    return false;
                }
            }

            public void Dispose()
            {
                if (this._disposed)
                    return;

                if (this._reader != null)
                {
                    if (!this._reader.IsClosed)
                        this._reader.Close();
                    this._reader.Dispose();
                    this._reader = null;
                }

                if (!this._hasFinished)
                {
                    this._hasFinished = true;
                }

                this._current = default(T);
                this._disposed = true;
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            void Prepare()
            {
                Type type = typeof(T);
                if (SupportedMappingTypes.IsMappingType(type))
                {
                    var field = new MappingField(type, 0);
                    this._objectActivator = field.CreateObjectActivator();
                    this._reader = this.ExecuteReader();
                    return;
                }

                this._reader = this.ExecuteReader();
                this._objectActivator = GetObjectActivator(type, this._reader);
            }

            private IDataReader ExecuteReader()
            {
                IDataReader reader = this._internalSqlQuery._dbContext.CommonSession.ExecuteReader(this._internalSqlQuery._sql, this._internalSqlQuery._cmdType, this._internalSqlQuery._parameters);
                return reader;
            }

            private static ObjectActivator GetObjectActivator(Type type, IDataReader reader)
            {
                List<CacheInfo> caches;
                if (!ObjectActivatorCache.TryGetValue(type, out caches))
                {
                    if (!Monitor.TryEnter(type))
                    {
                        return CreateObjectActivator(type, reader);
                    }

                    try
                    {
                        caches = ObjectActivatorCache.GetOrAdd(type, new List<CacheInfo>(1));
                    }
                    finally
                    {
                        Monitor.Exit(type);
                    }
                }

                CacheInfo cache = TryGetCacheInfoFromList(caches, reader);

                if (cache == null)
                {
                    lock (caches)
                    {
                        cache = TryGetCacheInfoFromList(caches, reader);
                        if (cache == null)
                        {
                            ObjectActivator activator = CreateObjectActivator(type, reader);
                            cache = new CacheInfo(activator, reader);
                            caches.Add(cache);
                        }
                    }
                }

                return cache.ObjectActivator;
            }

            static ObjectActivator CreateObjectActivator(Type type, IDataReader reader)
            {
                ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
                if (constructor == null)
                    throw new ArgumentException(string.Format("The type of '{0}' does't define a none parameter constructor.", type.FullName));

                EntityConstructorDescriptor constructorDescriptor = EntityConstructorDescriptor.GetConstructorDescriptor(constructor);
                EntityMemberMapper mapper = constructorDescriptor.GetEntityMemberMapper();
                Func<IDataReader, DataReaderOrdinalEnumerator, ObjectActivatorEnumerator, object> instanceCreator = constructorDescriptor.GetInstanceCreator();
                List<IValueSetter> memberSetters = PrepareValueSetters(type, reader, mapper);
                return new ObjectActivator(instanceCreator, null, null, memberSetters, null);
            }

            static List<IValueSetter> PrepareValueSetters(Type type, IDataReader reader, EntityMemberMapper mapper)
            {
                List<IValueSetter> memberSetters = new List<IValueSetter>(reader.FieldCount);

                MemberInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
                MemberInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetField);
                List<MemberInfo> members = new List<MemberInfo>(properties.Length + fields.Length);
                members.AddRange(properties);
                members.AddRange(fields);

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string name = reader.GetName(i);
                    var member = members.Where(a => a.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                    if (member == null)
                    {
                        member = members.Where(a => string.Equals(a.Name, name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                        if (member == null)
                        {
                            var entityMapping = EntityMappingCollection.Mappings[type.FullName];
                            var propMapping = entityMapping.PropertyMappings.Where(p => p.ColumnName.Equals(name, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                            if (propMapping != null)
                            {
                                member = members.Where(a => a.Name.Equals(propMapping.PropertyName)).FirstOrDefault();
                                if (member == null) { continue; }
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }

                    var memberMapper = mapper.TryGetMappingMemberMapper(member);
                    if (memberMapper == null)
                    {
                        continue;
                    }
                    var memberBinder = new MappingMemberBinder(member, memberMapper, i);
                    memberSetters.Add(memberBinder);
                }

                return memberSetters;
            }
            static CacheInfo TryGetCacheInfoFromList(List<CacheInfo> caches, IDataReader reader)
            {
                CacheInfo cache = null;
                for (int i = 0; i < caches.Count; i++)
                {
                    var item = caches[i];
                    if (item.IsTheSameFields(reader))
                    {
                        cache = item;
                        break;
                    }
                }

                return cache;
            }

            static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, List<CacheInfo>> ObjectActivatorCache = new System.Collections.Concurrent.ConcurrentDictionary<Type, List<CacheInfo>>();
        }

        public class CacheInfo
        {
            private ReaderFieldInfo[] _readerFields = null;

            public CacheInfo(ObjectActivator activator, IDataReader reader)
            {
                int fieldCount = reader.FieldCount;
                var readerFields = new ReaderFieldInfo[fieldCount];

                for (int i = 0; i < fieldCount; i++)
                {
                    readerFields[i] = new ReaderFieldInfo(reader.GetName(i), reader.GetFieldType(i));
                }

                this._readerFields = readerFields;
                this.ObjectActivator = activator;
            }

            public ObjectActivator ObjectActivator { get; private set; }

            public bool IsTheSameFields(IDataReader reader)
            {
                ReaderFieldInfo[] readerFields = this._readerFields;
                int fieldCount = reader.FieldCount;

                if (fieldCount != readerFields.Length)
                    return false;

                for (int i = 0; i < fieldCount; i++)
                {
                    ReaderFieldInfo readerField = readerFields[i];
                    if (reader.GetFieldType(i) != readerField.Type || reader.GetName(i) != readerField.Name)
                    {
                        return false;
                    }
                }

                return true;
            }

            /// <summary>
            /// DataReader字段信息
            /// </summary>
            internal class ReaderFieldInfo
            {
                public ReaderFieldInfo(string name, Type type)
                {
                    this.Name = name;
                    this.Type = type;
                }

                /// <summary>
                /// 字段名称
                /// </summary>
                public string Name { get; private set; }
                /// <summary>
                /// 字段数据类型
                /// </summary>
                public Type Type { get; private set; }
            }
        }
    }
}
