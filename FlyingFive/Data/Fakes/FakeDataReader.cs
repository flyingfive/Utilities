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
        private IDataReader _innerReader = null;
        private IDbCommand _innerCommand = null;
        private IList<OutputParameter> _outputParameters = null;
        private bool _disposed = false;

        public FakeDataReader(IDataReader reader)
        {
            if (reader == null) { throw new ArgumentNullException("参数: reader不能为null"); }
            this._innerReader = reader;
        }

        public FakeDataReader(CommonAdoSession commonSession, IDataReader reader, IDbCommand command, List<OutputParameter> outputParameters)
        {
            if (commonSession == null) { throw new ArgumentNullException("参数: commonSession不能为null"); }
            if (reader == null) { throw new ArgumentNullException("参数: reader不能为null"); }
            if (command == null) { throw new ArgumentNullException("参数: command不能为null"); }

            this._session = commonSession;
            this._innerReader = reader;
            this._innerCommand = command;
            this._outputParameters = outputParameters;
        }


        #region IDataReader
        /// <summary>
        /// 指示读取器当前行的嵌套深度
        /// </summary>
        public int Depth { get { return this._innerReader.Depth; } }
        /// <summary>
        /// 读取器是否关闭
        /// </summary>
        public bool IsClosed { get { return this._innerReader.IsClosed; } }
        /// <summary>
        /// 影响行数
        /// </summary>
        public int RecordsAffected { get { return this._innerReader.RecordsAffected; } }

        /// <summary>
        /// 关闭结果集读取器
        /// </summary>
        public void Close()
        {
            if (!this._innerReader.IsClosed)
            {
                try
                {
                    this._innerReader.Close();
                    this._innerReader.Dispose();
                    if (_outputParameters != null) { OutputParameter.CallMapValue(this._outputParameters); }
                }
                finally
                {
                    if (this._session != null) { this._session.Complete(); }
                }
            }
        }

        /// <summary>
        /// 获取读取器结果集的元数据构架表
        /// </summary>
        /// <returns></returns>
        public DataTable GetSchemaTable()
        {
            return this._innerReader.GetSchemaTable();
        }

        public bool NextResult()
        {
            return this._innerReader.NextResult();
        }

        public bool Read()
        {
            return this._innerReader.Read();
        }

        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }
            this.Close();
            if (this._innerCommand != null)
            {
                this._innerCommand.Dispose();
            }
            this._disposed = true;
        }
        #endregion

        #region IDataRecord
        public int FieldCount { get { return this._innerReader.FieldCount; } }

        public object this[int i] { get { return this._innerReader[i]; } }
        public object this[string name] { get { return this._innerReader[name]; } }

        public bool GetBoolean(int i)
        {
            return this._innerReader.GetBoolean(i);
        }
        public byte GetByte(int i)
        {
            return this._innerReader.GetByte(i);
        }
        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            return this._innerReader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
        }
        public char GetChar(int i)
        {
            return this._innerReader.GetChar(i);
        }
        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            return this._innerReader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
        }
        public IDataReader GetData(int i)
        {
            return this._innerReader.GetData(i);
        }
        public string GetDataTypeName(int i)
        {
            return this._innerReader.GetDataTypeName(i);
        }
        public DateTime GetDateTime(int i)
        {
            return this._innerReader.GetDateTime(i);
        }
        public decimal GetDecimal(int i)
        {
            return this._innerReader.GetDecimal(i);
        }
        public double GetDouble(int i)
        {
            return this._innerReader.GetDouble(i);
        }
        public Type GetFieldType(int i)
        {
            return this._innerReader.GetFieldType(i);
        }
        public float GetFloat(int i)
        {
            return this._innerReader.GetFloat(i);
        }
        public Guid GetGuid(int i)
        {
            return this._innerReader.GetGuid(i);
        }
        public short GetInt16(int i)
        {
            return this._innerReader.GetInt16(i);
        }
        public int GetInt32(int i)
        {
            return this._innerReader.GetInt32(i);
        }
        public long GetInt64(int i)
        {
            return this._innerReader.GetInt64(i);
        }
        public string GetName(int i)
        {
            return this._innerReader.GetName(i);
        }
        public int GetOrdinal(string name)
        {
            return this._innerReader.GetOrdinal(name);
        }
        public string GetString(int i)
        {
            return this._innerReader.GetString(i);
        }
        public object GetValue(int i)
        {
            return this._innerReader.GetValue(i);
        }
        public int GetValues(object[] values)
        {
            return this._innerReader.GetValues(values);
        }
        public bool IsDBNull(int i)
        {
            return this._innerReader.IsDBNull(i);
        }
        #endregion

    }
}
