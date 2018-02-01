using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Fakes
{
    /// <summary>
    /// 表示输出参数
    /// </summary>
    public class OutputParameter
    {
        private FakeParameter _fakeParameter = null;
        private IDbDataParameter _dbParameter = null;

        public OutputParameter(FakeParameter param, IDbDataParameter parameter)
        {
            this._fakeParameter = param;
            this._dbParameter = parameter;
        }

        /// <summary>
        /// 执行完DB操作后从实际的DB参数中将值映射到伪装参数
        /// </summary>
        public void MapValue()
        {
            object val = this._dbParameter.Value;
            if (val == DBNull.Value)
                this._fakeParameter.Value = null;
            else
                this._fakeParameter.Value = val;
        }

        /// <summary>
        /// DB操作完成后批量映射输出参数的实际值到伪装参数上
        /// </summary>
        /// <param name="outputParameters">输出参数列表</param>
        public static void CallMapValue(IList<OutputParameter> outputParameters)
        {
            if (outputParameters != null)
            {
                for (int i = 0; i < outputParameters.Count; i++)
                {
                    outputParameters[i].MapValue();
                }
            }
        }
    }
}
