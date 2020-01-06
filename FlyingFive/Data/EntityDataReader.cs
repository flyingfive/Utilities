using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace FlyingFive.Data
{
    /// <summary>
    /// 泛型集合的数据读取器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class EntityDataReader<T> : DbDataReader, IDataReader where T : class
    {
        private T _current = null;
        private bool _closed = false;
        private readonly IEnumerator<T> enumerator = null;
        private IEnumerable<T> _innerCollection = null;
        private IList<Extensions.MappingData> _mappings = null;

        private static readonly HashSet<Type> _scalarTypes = new HashSet<Type>()
        { 
            //引用类型
            typeof(String),
            typeof(Byte[]),
            //值类型
            typeof(Byte),
            typeof(Int16),
            typeof(Int32),
            typeof(Int64),
            typeof(Single),
            typeof(Double),
            typeof(Decimal),
            typeof(DateTime),
            typeof(Guid),
            typeof(Boolean),
            typeof(TimeSpan),
            //可空的值类型
            typeof(Byte?),
            typeof(Int16?),
            typeof(Int32?),
            typeof(Int64?),
            typeof(Single?),
            typeof(Double?),
            typeof(Decimal?),
            typeof(DateTime?),
            typeof(Guid?),
            typeof(Boolean?),
            typeof(TimeSpan?)
        };


        public EntityDataReader(IEnumerable<T> collection)
        {
            if (collection == null) { throw new ArgumentNullException("参数collection不能为null"); }
            _mappings = new List<Extensions.MappingData>();
            this.enumerator = collection.GetEnumerator();
            this._innerCollection = collection;
            var index = 0;
            var properties = typeof(T).GetProperties().Where(p => p.CanWrite && p.CanRead && IsScalarType(p.PropertyType));
            foreach (var property in properties)
            {
                _mappings.Add(new Extensions.MappingData() { Property = property, ValueAccessor = Emit.DelegateGenerator.CreateValueGetter(property), Ordinal = index });
                index++;
            }
        }

        private static bool IsScalarType(Type t)
        {
            return _scalarTypes.Contains(t);
        }

        private static Type StripNullableType(Type t)
        {
            return t.GetGenericArguments()[0];
        }

        private TField GetValue<TField>(int i)
        {
            var mapping = _mappings.FirstOrDefault(m => m.Ordinal == i);
            if (mapping == null) { throw new InvalidOperationException("索引不在范围内。"); }
            var val = (TField)mapping.ValueAccessor(_current);
            return val;
        }

        #region GetSchemaTable


        const string shemaTableSchema = @"<?xml version=""1.0"" standalone=""yes""?>
<xs:schema id=""NewDataSet"" xmlns="""" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
  <xs:element name=""NewDataSet"" msdata:IsDataSet=""true"" msdata:MainDataTable=""SchemaTable"" msdata:Locale="""">
    <xs:complexType>
      <xs:choice minOccurs=""0"" maxOccurs=""unbounded"">
        <xs:element name=""SchemaTable"" msdata:Locale="""" msdata:MinimumCapacity=""1"">
          <xs:complexType>
            <xs:sequence>
              <xs:element name=""ColumnName"" msdata:ReadOnly=""true"" type=""xs:string"" minOccurs=""0"" />
              <xs:element name=""ColumnOrdinal"" msdata:ReadOnly=""true"" type=""xs:int"" default=""0"" minOccurs=""0"" />
              <xs:element name=""ColumnSize"" msdata:ReadOnly=""true"" type=""xs:int"" minOccurs=""0"" />
              <xs:element name=""NumericPrecision"" msdata:ReadOnly=""true"" type=""xs:short"" minOccurs=""0"" />
              <xs:element name=""NumericScale"" msdata:ReadOnly=""true"" type=""xs:short"" minOccurs=""0"" />
              <xs:element name=""IsUnique"" msdata:ReadOnly=""true"" type=""xs:boolean"" minOccurs=""0"" />
              <xs:element name=""IsKey"" msdata:ReadOnly=""true"" type=""xs:boolean"" minOccurs=""0"" />
              <xs:element name=""BaseServerName"" msdata:ReadOnly=""true"" type=""xs:string"" minOccurs=""0"" />
              <xs:element name=""BaseCatalogName"" msdata:ReadOnly=""true"" type=""xs:string"" minOccurs=""0"" />
              <xs:element name=""BaseColumnName"" msdata:ReadOnly=""true"" type=""xs:string"" minOccurs=""0"" />
              <xs:element name=""BaseSchemaName"" msdata:ReadOnly=""true"" type=""xs:string"" minOccurs=""0"" />
              <xs:element name=""BaseTableName"" msdata:ReadOnly=""true"" type=""xs:string"" minOccurs=""0"" />
              <xs:element name=""DataType"" msdata:DataType=""System.Type, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"" msdata:ReadOnly=""true"" type=""xs:string"" minOccurs=""0"" />
              <xs:element name=""AllowDBNull"" msdata:ReadOnly=""true"" type=""xs:boolean"" minOccurs=""0"" />
              <xs:element name=""ProviderType"" msdata:ReadOnly=""true"" type=""xs:int"" minOccurs=""0"" />
              <xs:element name=""IsAliased"" msdata:ReadOnly=""true"" type=""xs:boolean"" minOccurs=""0"" />
              <xs:element name=""IsExpression"" msdata:ReadOnly=""true"" type=""xs:boolean"" minOccurs=""0"" />
              <xs:element name=""IsIdentity"" msdata:ReadOnly=""true"" type=""xs:boolean"" minOccurs=""0"" />
              <xs:element name=""IsAutoIncrement"" msdata:ReadOnly=""true"" type=""xs:boolean"" minOccurs=""0"" />
              <xs:element name=""IsRowVersion"" msdata:ReadOnly=""true"" type=""xs:boolean"" minOccurs=""0"" />
              <xs:element name=""IsHidden"" msdata:ReadOnly=""true"" type=""xs:boolean"" minOccurs=""0"" />
              <xs:element name=""IsLong"" msdata:ReadOnly=""true"" type=""xs:boolean"" default=""false"" minOccurs=""0"" />
              <xs:element name=""IsReadOnly"" msdata:ReadOnly=""true"" type=""xs:boolean"" minOccurs=""0"" />
              <xs:element name=""ProviderSpecificDataType"" msdata:DataType=""System.Type, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"" msdata:ReadOnly=""true"" type=""xs:string"" minOccurs=""0"" />
              <xs:element name=""DataTypeName"" msdata:ReadOnly=""true"" type=""xs:string"" minOccurs=""0"" />
              <xs:element name=""XmlSchemaCollectionDatabase"" msdata:ReadOnly=""true"" type=""xs:string"" minOccurs=""0"" />
              <xs:element name=""XmlSchemaCollectionOwningSchema"" msdata:ReadOnly=""true"" type=""xs:string"" minOccurs=""0"" />
              <xs:element name=""XmlSchemaCollectionName"" msdata:ReadOnly=""true"" type=""xs:string"" minOccurs=""0"" />
              <xs:element name=""UdtAssemblyQualifiedName"" msdata:ReadOnly=""true"" type=""xs:string"" minOccurs=""0"" />
              <xs:element name=""NonVersionedProviderType"" msdata:ReadOnly=""true"" type=""xs:int"" minOccurs=""0"" />
            </xs:sequence>
          </xs:complexType>
        </xs:element>
      </xs:choice>
    </xs:complexType>
  </xs:element>
</xs:schema>";
        public override DataTable GetSchemaTable()
        {
            DataSet s = new DataSet();
            s.Locale = System.Globalization.CultureInfo.CurrentCulture;
            s.ReadXmlSchema(new System.IO.StringReader(shemaTableSchema));
            DataTable t = s.Tables[0];
            for (int i = 0; i < this.FieldCount; i++)
            {
                DataRow row = t.NewRow();
                row["ColumnName"] = this.GetName(i);
                row["ColumnOrdinal"] = i;

                Type type = this.GetFieldType(i);
                if (type.IsGenericType
                  && type.GetGenericTypeDefinition() == typeof(System.Nullable<int>).GetGenericTypeDefinition())
                {
                    type = type.GetGenericArguments()[0];
                }
                row["DataType"] = this.GetFieldType(i);
                row["DataTypeName"] = this.GetDataTypeName(i);
                row["ColumnSize"] = -1;
                t.Rows.Add(row);
            }
            return t;

        }
        #endregion

        #region IDataReader Members

        public override void Close()
        {
            _closed = true;
        }

        public override int Depth
        {
            get { return 1; }
        }


        public override bool IsClosed
        {
            get { return _closed; }
        }

        public override bool NextResult()
        {
            return false;
        }

        private int _entitiesRead = 0;
        public override bool Read()
        {
            bool rv = enumerator.MoveNext();
            if (rv)
            {
                _current = enumerator.Current;
                _entitiesRead += 1;
            }
            return rv;
        }

        public override int RecordsAffected
        {
            get { return -1; }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            Close();
            base.Dispose(disposing);
        }
        
        #region IDataRecord Members

        public override int FieldCount
        {
            get
            {
                return _mappings.Count;
            }
        }

        public override bool GetBoolean(int i)
        {
            return GetValue<bool>(i);
        }

        public override byte GetByte(int i)
        {
            return GetValue<byte>(i);
        }

        public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            var buf = GetValue<byte[]>(i);
            int bytes = Math.Min(length, buf.Length - (int)fieldOffset);
            Buffer.BlockCopy(buf, (int)fieldOffset, buffer, bufferoffset, bytes);
            return bytes;
        }

        public override char GetChar(int i)
        {
            return GetValue<char>(i);
        }

        public override long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            string s = GetValue<string>(i);
            int chars = Math.Min(length, s.Length - (int)fieldoffset);
            s.CopyTo((int)fieldoffset, buffer, bufferoffset, chars);
            return chars;
        }

        public override string GetDataTypeName(int i)
        {
            var type = GetFieldType(i);
            return type.Name;
        }

        public override DateTime GetDateTime(int i)
        {
            return GetValue<DateTime>(i);
        }

        public override decimal GetDecimal(int i)
        {
            return GetValue<decimal>(i);
        }

        public override double GetDouble(int i)
        {
            return GetValue<double>(i);
        }

        public override Type GetFieldType(int i)
        {
            var mapping = _mappings.SingleOrDefault(m => m.Ordinal == i);
            if (mapping == null) { throw new InvalidOperationException("索引不在范围内。"); }
            var dataType = mapping.Property.PropertyType;
            if (dataType.IsNullableType())
            {
                return dataType.GetUnderlyingType();
            }
            return dataType;
        }

        public override float GetFloat(int i)
        {
            return GetValue<float>(i);
        }

        public override Guid GetGuid(int i)
        {
            return GetValue<Guid>(i);
        }

        public override short GetInt16(int i)
        {
            return GetValue<short>(i);
        }

        public override int GetInt32(int i)
        {
            return GetValue<int>(i);
        }

        public override long GetInt64(int i)
        {
            return GetValue<long>(i);
        }

        public override string GetName(int i)
        {
            var mapping = _mappings.SingleOrDefault(m => m.Ordinal == i);
            if (mapping != null)
            {
                return mapping.Property.Name;
            }
            return string.Empty;
        }

        public override int GetOrdinal(string name)
        {
            var mapping = _mappings.SingleOrDefault(m => m.Property.Name.Equals(name));
            if (mapping != null)
            {
                return mapping.Ordinal;
            }
            return -1;
        }

        public override string GetString(int i)
        {
            return GetValue<string>(i);
        }

        public override int GetValues(object[] values)
        {
            for (int i = 0; i < _mappings.Count; i++)
            {
                values[i] = GetValue(i);
            }
            return _mappings.Count;
        }

        public override object GetValue(int i)
        {
            object o = GetValue<object>(i);
            return o;
        }

        public override bool IsDBNull(int i)
        {
            object o = GetValue<object>(i);
            return (o == null);
        }

        public override object this[string name]
        {
            get { return GetValue(GetOrdinal(name)); }
        }

        public override object this[int i]
        {
            get { return GetValue(i); }
        }

        #endregion

        #region DbDataReader Members

        public override System.Collections.IEnumerator GetEnumerator()
        {
            return this.enumerator;
        }

        public override bool HasRows
        {
            get { return _innerCollection != null && _innerCollection.Count() > 0; }
        }

        #endregion
    }
}
