using FlyingFive.Data.Kernel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Fakes
{
    /// <summary>
    /// 伪装的DataReader数据读取器
    /// </summary>
    public class FakeDataReader : IDataReader, IDataRecord, IDisposable
    {
        private CommonAdoSession _session = null;
        private IDataReader _dataReader = null;
        private IDbCommand _command = null;
        private IList<OutputParameter> _outputParameters = null;
        private bool _disposed = false;


        public FakeDataReader(CommonAdoSession commonSession, IDataReader reader, IDbCommand command, List<OutputParameter> outputParameters)
        {
            if (commonSession == null) { throw new ArgumentNullException("参数: commonSession不能为null"); }
            if (reader == null) { throw new ArgumentNullException("参数: reader不能为null"); }
            if (command == null) { throw new ArgumentNullException("参数: command不能为null"); }

            this._session = commonSession;
            this._dataReader = reader;
            this._command = command;
            this._outputParameters = outputParameters;
        }


        #region IDataReader
        /// <summary>
        /// 指示读取器当前行的嵌套深度
        /// </summary>
        public int Depth { get { return this._dataReader.Depth; } }
        /// <summary>
        /// 读取器是否关闭
        /// </summary>
        public bool IsClosed { get { return this._dataReader.IsClosed; } }
        /// <summary>
        /// 影响行数
        /// </summary>
        public int RecordsAffected { get { return this._dataReader.RecordsAffected; } }

        /// <summary>
        /// 关闭结果集读取器
        /// </summary>
        public void Close()
        {
            if (!this._dataReader.IsClosed)
            {
                try
                {
                    this._dataReader.Close();
                    this._dataReader.Dispose();
                    OutputParameter.CallMapValue(this._outputParameters);
                }
                finally
                {
                    this._session.Complete();
                }
            }
        }

        /// <summary>
        /// 获取读取器结果集的元数据构架表
        /// </summary>
        /// <returns></returns>
        public DataTable GetSchemaTable()
        {
            return this._dataReader.GetSchemaTable();
        }

        public bool NextResult()
        {
            return this._dataReader.NextResult();
        }

        public bool Read()
        {
            return this._dataReader.Read();
        }

        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }
            this.Close();
            this._command.Dispose();
            this._disposed = true;
        }
        #endregion

        #region IDataRecord
        public int FieldCount { get { return this._dataReader.FieldCount; } }

        public object this[int i] { get { return this._dataReader[i]; } }
        public object this[string name] { get { return this._dataReader[name]; } }

        public bool GetBoolean(int i)
        {
            return this._dataReader.GetBoolean(i);
        }
        public byte GetByte(int i)
        {
            return this._dataReader.GetByte(i);
        }
        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            return this._dataReader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
        }
        public char GetChar(int i)
        {
            return this._dataReader.GetChar(i);
        }
        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            return this._dataReader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
        }
        public IDataReader GetData(int i)
        {
            return this._dataReader.GetData(i);
        }
        public string GetDataTypeName(int i)
        {
            return this._dataReader.GetDataTypeName(i);
        }
        public DateTime GetDateTime(int i)
        {
            return this._dataReader.GetDateTime(i);
        }
        public decimal GetDecimal(int i)
        {
            return this._dataReader.GetDecimal(i);
        }
        public double GetDouble(int i)
        {
            return this._dataReader.GetDouble(i);
        }
        public Type GetFieldType(int i)
        {
            return this._dataReader.GetFieldType(i);
        }
        public float GetFloat(int i)
        {
            return this._dataReader.GetFloat(i);
        }
        public Guid GetGuid(int i)
        {
            return this._dataReader.GetGuid(i);
        }
        public short GetInt16(int i)
        {
            return this._dataReader.GetInt16(i);
        }
        public int GetInt32(int i)
        {
            return this._dataReader.GetInt32(i);
        }
        public long GetInt64(int i)
        {
            return this._dataReader.GetInt64(i);
        }
        public string GetName(int i)
        {
            return this._dataReader.GetName(i);
        }
        public int GetOrdinal(string name)
        {
            return this._dataReader.GetOrdinal(name);
        }
        public string GetString(int i)
        {
            return this._dataReader.GetString(i);
        }
        public object GetValue(int i)
        {
            return this._dataReader.GetValue(i);
        }
        public int GetValues(object[] values)
        {
            return this._dataReader.GetValues(values);
        }
        public bool IsDBNull(int i)
        {
            return this._dataReader.IsDBNull(i);
        }
        #endregion

    }
}
