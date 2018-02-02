using FlyingFive.Data.DbExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Drivers.SqlServer
{
    public class MsSqlGenerator_OffsetFetch : MsSqlGenerator
    {
        protected override void BuildLimitSql(DbExpressions.DbSqlQueryExpression exp)
        {
            this.SqlBuilder.Append("SELECT ");

            var columns = exp.ColumnSegments;
            for (int i = 0; i < columns.Count; i++)
            {
                DbColumnSegment column = columns[i];
                if (i > 0)
                    this.SqlBuilder.Append(",");

                this.AppendColumnSegment(column);
            }

            this.SqlBuilder.Append(" FROM ");
            exp.Table.Accept(this);
            this.BuildWhereState(exp.Condition);
            this.BuildGroupState(exp);

            List<DbOrdering> orderings = exp.Orderings;
            if (orderings.Count == 0)
            {
                DbExpression orderingExp = DbExpression.Add(UtilConstants.DbParameter_1, DbConstantExpression.Zero, DbConstantExpression.Zero.Type, null);
                DbOrdering ordering = new DbOrdering(orderingExp, DbOrderType.Asc);
                orderings = new List<DbOrdering>(1);
                orderings.Add(ordering);
            }

            this.BuildOrderState(orderings);

            this.SqlBuilder.Append(" OFFSET ", exp.SkipCount.Value.ToString(), " ROWS");
            if (exp.TakeCount != null)
            {
                this.SqlBuilder.Append(" FETCH NEXT ", exp.TakeCount.Value.ToString(), " ROWS ONLY");
            }
        }
    }
}
