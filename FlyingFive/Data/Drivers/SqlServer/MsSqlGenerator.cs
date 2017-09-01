using FlyingFive.Data.DbExpressions;
using FlyingFive.Data.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FlyingFive.Data.Drivers.SqlServer
{
    /// <summary>
    /// MsSql规范的SQL代码生成器
    /// </summary>
    public partial class MsSqlGenerator : DbExpressionVisitor<DbExpression>
    {
        public const string ParameterPrefix = "@P";

        public ISqlBuilder SqlBuilder { get; internal set; }
        public List<FakeParameter> Parameters { get; private set; }

        private DbValueExpressionVisitor _valueExpressionVisitor;
        DbValueExpressionVisitor ValueExpressionVisitor
        {
            get
            {
                if (this._valueExpressionVisitor == null)
                    this._valueExpressionVisitor = new DbValueExpressionVisitor(this);

                return this._valueExpressionVisitor;
            }
        }

        public MsSqlGenerator()
        {
            this.SqlBuilder = new SqlBuilder();
            this.Parameters = new List<FakeParameter>();
        }


        public static MsSqlGenerator CreateInstance()
        {
            return new MsSqlGenerator();
        }


        public override DbExpression Visit(DbColumnAccessExpression exp)
        {
            this.QuoteName(exp.Table.Name);
            this.SqlBuilder.Append(".");
            this.QuoteName(exp.Column.Name);
            return exp;
        }

        public override DbExpression Visit(DbInsertExpression exp)
        {
            this.SqlBuilder.Append("INSERT INTO ");
            this.QuoteName(exp.Table.Name);
            this.SqlBuilder.Append("(");

            bool first = true;
            foreach (var item in exp.InsertColumns)
            {
                if (first)
                    first = false;
                else
                {
                    this.SqlBuilder.Append(",");
                }

                this.QuoteName(item.Key.Name);
            }

            this.SqlBuilder.Append(")");

            this.SqlBuilder.Append(" VALUES(");
            first = true;
            foreach (var item in exp.InsertColumns)
            {
                if (first)
                    first = false;
                else
                {
                    this.SqlBuilder.Append(",");
                }

                DbExpression valExp = item.Value.OptimizeDbExpression();
                AmendDbInfo(item.Key, valExp);
                valExp.Accept(this.ValueExpressionVisitor);
            }

            this.SqlBuilder.Append(")");

            return exp;
        }

        public override DbExpression Visit(DbParameterExpression exp)
        {
            object paramValue = exp.Value;
            Type paramType = exp.Type;

            if (paramType.IsEnum)
            {
                paramType = Enum.GetUnderlyingType(paramType);
                if (paramValue != null) { paramValue = Convert.ChangeType(paramValue, paramType); }
            }
            if (paramValue == null)
            {
                paramValue = DBNull.Value;
            }

            FakeParameter p = null;
            //if (paramValue == DBNull.Value)
            //{
            //    p = this.Parameters.Where(a => AreEqual(a.Value, paramValue) && a.DataType == paramType).FirstOrDefault();
            //}
            //else
            //{
            //    p = this.Parameters.Where(a => a.DbType == exp.DbType && AreEqual(a.Value, paramValue)).FirstOrDefault();
            //}
            //if (p != null)
            //{
            //    this.SqlBuilder.Append(p.Name);
            //    return exp;
            //}

            var paramName = CreateParameterName(this.Parameters.Count);
            p = FakeParameter.Create(paramName, paramValue, paramType);

            if (paramValue.GetType() == typeof(String))
            {
                if (exp.DbType == DbType.AnsiStringFixedLength || exp.DbType == DbType.StringFixedLength)
                {
                    p.Size = ((string)paramValue).Length;
                }
                if (paramValue.TryConvert<String>().Length <= 4000)
                {
                    p.Size = 4000;
                }
            }
            if (exp.DbType != null)
            {
                p.DbType = exp.DbType;
            }
            this.Parameters.Add(p);
            this.SqlBuilder.Append(paramName);
            return exp;
        }

        public override DbExpression Visit(DbConvertExpression exp)
        {
            var stripedExp = exp.StripInvalidConvert();

            if (stripedExp.NodeType != DbExpressionType.Convert)
            {
                EnsureDbExpressionReturnCSharpBoolean(stripedExp).Accept(this);
                return exp;
            }

            exp = (DbConvertExpression)stripedExp;

            string dbTypeString;
            if (TryGetCastTargetDbTypeString(exp.Operand.Type, exp.Type, out dbTypeString))
            {
                this.BuildCastState(EnsureDbExpressionReturnCSharpBoolean(exp.Operand), dbTypeString);
            }
            else
                EnsureDbExpressionReturnCSharpBoolean(exp.Operand).Accept(this);

            return exp;
        }
        public override DbExpression Visit(DbMemberExpression exp)
        {
            MemberInfo member = exp.Member;

            if (member.DeclaringType == typeof(DateTime))
            {
                if (member == UtilConstants.PropertyInfo_DateTime_Now)
                {
                    this.SqlBuilder.Append("GETDATE()");
                    return exp;
                }
                if (member == UtilConstants.PropertyInfo_DateTime_UtcNow)
                {
                    this.SqlBuilder.Append("GETUTCDATE()");
                    return exp;
                }
                if (member == UtilConstants.PropertyInfo_DateTime_Today)
                {
                    this.BuildCastState("GETDATE()", "DATE");
                    return exp;
                }
                if (member == UtilConstants.PropertyInfo_DateTime_Date)
                {
                    this.BuildCastState(exp.Expression, "DATE");
                    return exp;
                }
                if (this.IsDatePart(exp))
                {
                    return exp;
                }
            }

            DbParameterExpression newExp;
            if (exp.TryConvertToParameterExpression(out newExp))
            {
                return newExp.Accept(this);
            }
            if (member.Name == "Length" && member.DeclaringType == UtilConstants.TypeOfString)
            {
                this.SqlBuilder.Append("LEN(");
                exp.Expression.Accept(this);
                this.SqlBuilder.Append(")");
                return exp;
            }
            else if (member.Name == "Value" && exp.Expression.Type.IsNullable())
            {
                exp.Expression.Accept(this);
                return exp;
            }
            throw new NotSupportedException(string.Format("'{0}.{1}' is not supported.", member.DeclaringType.FullName, member.Name));
        }

        public override DbExpression Visit(DbCaseWhenExpression exp)
        {
            this.SqlBuilder.Append("CASE");
            foreach (var whenThen in exp.WhenThenPairs)
            {
                // then 部分得判断是否是诸如 a>1,a=b,in,like 等等的情况，如果是则将其构建成一个 case when 
                this.SqlBuilder.Append(" WHEN ");
                whenThen.When.Accept(this);
                this.SqlBuilder.Append(" THEN ");
                EnsureDbExpressionReturnCSharpBoolean(whenThen.Then).Accept(this);
            }

            this.SqlBuilder.Append(" ELSE ");
            EnsureDbExpressionReturnCSharpBoolean(exp.Else).Accept(this);
            this.SqlBuilder.Append(" END");
            return exp;
        }

        public override DbExpression Visit(DbMethodCallExpression exp)
        {
            Action<DbMethodCallExpression, MsSqlGenerator> methodHandler;
            if (!MethodHandlers.TryGetValue(exp.Method.Name, out methodHandler))
            {
                throw UtilExceptions.NotSupportedMethod(exp.Method);
            }
            methodHandler(exp, this);
            return exp;
        }

        public override DbExpression Visit(DbEqualExpression exp)
        {
            DbExpression left = exp.Left;
            DbExpression right = exp.Right;
            left = left.OptimizeDbExpression();
            right = right.OptimizeDbExpression();
            //明确 left right 其中一边一定为 null
            if (right.AffirmExpressionRetValueIsNull())
            {
                left.Accept(this);
                this.SqlBuilder.Append(" IS NULL");
                return exp;
            }
            if (left.AffirmExpressionRetValueIsNull())
            {
                right.Accept(this);
                this.SqlBuilder.Append(" IS NULL");
                return exp;
            }
            AmendDbInfo(left, right);
            left.Accept(this);
            this.SqlBuilder.Append(" = ");
            right.Accept(this);
            return exp;
        }

        public override DbExpression Visit(DbNotEqualExpression exp)
        {
            DbExpression left = exp.Left;
            DbExpression right = exp.Right;
            left = left.OptimizeDbExpression();
            right = right.OptimizeDbExpression();
            //明确 left right 其中一边一定为 null
            if (right.AffirmExpressionRetValueIsNull())
            {
                left.Accept(this);
                this.SqlBuilder.Append(" IS NOT NULL");
                return exp;
            }
            if (left.AffirmExpressionRetValueIsNull())
            {
                right.Accept(this);
                this.SqlBuilder.Append(" IS NOT NULL");
                return exp;
            }
            AmendDbInfo(left, right);
            left.Accept(this);
            this.SqlBuilder.Append(" <> ");
            right.Accept(this);
            return exp;
        }

        public override DbExpression Visit(DbAddExpression exp)
        {
            MethodInfo method = exp.Method;
            if (method != null)
            {
                Action<DbBinaryExpression, MsSqlGenerator> handler;
                if (BinaryWithMethodHandlers.TryGetValue(method, out handler))
                {
                    handler(exp, this);
                    return exp;
                }
            }
            Stack<DbExpression> operands = GatherBinaryExpressionOperand(exp);
            this.ConcatOperands(operands, " + ");
            return exp;
        }

        public override DbExpression Visit(DbSubtractExpression exp)
        {
            Stack<DbExpression> operands = GatherBinaryExpressionOperand(exp);
            this.ConcatOperands(operands, " - ");
            return exp;
        }

        public override DbExpression Visit(DbMultiplyExpression exp)
        {
            Stack<DbExpression> operands = GatherBinaryExpressionOperand(exp);
            this.ConcatOperands(operands, " * ");
            return exp;
        }

        public override DbExpression Visit(DbDivideExpression exp)
        {
            Stack<DbExpression> operands = GatherBinaryExpressionOperand(exp);
            this.ConcatOperands(operands, " / ");
            return exp;
        }

        public override DbExpression Visit(DbModuloExpression exp)
        {
            Stack<DbExpression> operands = GatherBinaryExpressionOperand(exp);
            this.ConcatOperands(operands, " % ");
            return exp;
        }

        public override DbExpression Visit(DbLessThanExpression exp)
        {
            exp.Left.Accept(this);
            this.SqlBuilder.Append(" < ");
            exp.Right.Accept(this);
            return exp;
        }

        public override DbExpression Visit(DbLessThanOrEqualExpression exp)
        {
            exp.Left.Accept(this);
            this.SqlBuilder.Append(" <= ");
            exp.Right.Accept(this);
            return exp;
        }

        public override DbExpression Visit(DbGreaterThanExpression exp)
        {
            exp.Left.Accept(this);
            this.SqlBuilder.Append(" > ");
            exp.Right.Accept(this);

            return exp;
        }

        public override DbExpression Visit(DbGreaterThanOrEqualExpression exp)
        {
            exp.Left.Accept(this);
            this.SqlBuilder.Append(" >= ");
            exp.Right.Accept(this);
            return exp;
        }

        public override DbExpression Visit(DbBitAndExpression exp)
        {
            Stack<DbExpression> operands = GatherBinaryExpressionOperand(exp);
            this.ConcatOperands(operands, " & ");
            return exp;
        }

        public override DbExpression Visit(DbAndExpression exp)
        {
            Stack<DbExpression> operands = GatherBinaryExpressionOperand(exp);
            this.ConcatOperands(operands, " AND ");
            return exp;
        }

        public override DbExpression Visit(DbBitOrExpression exp)
        {
            Stack<DbExpression> operands = GatherBinaryExpressionOperand(exp);
            this.ConcatOperands(operands, " | ");
            return exp;
        }

        public override DbExpression Visit(DbOrExpression exp)
        {
            Stack<DbExpression> operands = GatherBinaryExpressionOperand(exp);
            this.ConcatOperands(operands, " OR ");
            return exp;
        }

        public override DbExpression Visit(DbConstantExpression exp)
        {
            if (exp.Value == null || exp.Value == DBNull.Value)
            {
                this.SqlBuilder.Append("NULL");
                return exp;
            }

            var objType = exp.Value.GetType();
            if (objType == UtilConstants.TypeOfBoolean)
            {
                this.SqlBuilder.Append(((bool)exp.Value) ? "CAST(1 AS BIT)" : "CAST(0 AS BIT)");
                return exp;
            }
            else if (objType == UtilConstants.TypeOfString)
            {
                this.SqlBuilder.Append("N'", exp.Value, "'");
                return exp;
            }
            else if (objType.IsEnum)
            {
                this.SqlBuilder.Append(Convert.ChangeType(exp.Value, Enum.GetUnderlyingType(objType)).ToString());
                return exp;
            }
            else if (NumericTypes.ContainsKey(exp.Value.GetType()))
            {
                this.SqlBuilder.Append(exp.Value);
                return exp;
            }

            DbParameterExpression p = new DbParameterExpression(exp.Value);
            p.Accept(this);

            return exp;
        }

        public override DbExpression Visit(DbNotExpression exp)
        {
            this.SqlBuilder.Append("NOT ");
            this.SqlBuilder.Append("(");
            exp.Operand.Accept(this);
            this.SqlBuilder.Append(")");
            return exp;
        }

        public override DbExpression Visit(DbNullConvertExpression exp)
        {
            this.SqlBuilder.Append("ISNULL(");
            EnsureDbExpressionReturnCSharpBoolean(exp.CheckExpression).Accept(this);
            this.SqlBuilder.Append(",");
            EnsureDbExpressionReturnCSharpBoolean(exp.ReplacementValue).Accept(this);
            this.SqlBuilder.Append(")");
            return exp;
        }

        public override DbExpression Visit(DbTableExpression exp)
        {
            if (exp.Table.Schema != null)
            {
                this.QuoteName(exp.Table.Schema);
                this.SqlBuilder.Append(".");
            }
            this.QuoteName(exp.Table.Name);
            return exp;
        }

        public override DbExpression Visit(DbSubQueryExpression exp)
        {
            this.SqlBuilder.Append("(");
            exp.SqlQuery.Accept(this);
            this.SqlBuilder.Append(")");
            return exp;
        }

        public override DbExpression Visit(DbSqlQueryExpression exp)
        {
            if (exp.SkipCount.HasValue)
            {
                this.BuildLimitSql(exp);
                return exp;
            }
            else
            {
                this.BuildGeneralSql(exp);
                return exp;
            }
        }

        public override DbExpression Visit(DbFromTableExpression exp)
        {
            this.AppendTableSegment(exp.Table);
            this.VisitDbJoinTableExpressions(exp.JoinTables);
            return exp;
        }

        public override DbExpression Visit(DbJoinTableExpression exp)
        {
            DbJoinTableExpression joinTablePart = exp;
            string joinString = null;
            if (joinTablePart.JoinType == DbJoinType.InnerJoin)
            {
                joinString = " INNER JOIN ";
            }
            else if (joinTablePart.JoinType == DbJoinType.LeftJoin)
            {
                joinString = " LEFT JOIN ";
            }
            else if (joinTablePart.JoinType == DbJoinType.RightJoin)
            {
                joinString = " RIGHT JOIN ";
            }
            else if (joinTablePart.JoinType == DbJoinType.FullJoin)
            {
                joinString = " FULL JOIN ";
            }
            else
                throw new NotSupportedException("JoinType: " + joinTablePart.JoinType);

            this.SqlBuilder.Append(joinString);
            this.AppendTableSegment(joinTablePart.Table);
            this.SqlBuilder.Append(" ON ");
            joinTablePart.Condition.Accept(this);
            this.VisitDbJoinTableExpressions(joinTablePart.JoinTables);
            return exp;
        }

        public override DbExpression Visit(DbAggregateExpression exp)
        {
            Action<DbAggregateExpression, MsSqlGenerator> aggregateHandler;
            if (!AggregateHandlers.TryGetValue(exp.Method.Name, out aggregateHandler))
            {
                throw UtilExceptions.NotSupportedMethod(exp.Method);
            }
            aggregateHandler(exp, this);
            return exp;
        }

        public override DbExpression Visit(DbUpdateExpression exp)
        {
            this.SqlBuilder.Append("UPDATE ");
            this.QuoteName(exp.Table.Name);
            this.SqlBuilder.Append(" SET ");
            bool first = true;
            foreach (var item in exp.UpdateColumns)
            {
                if (first)
                    first = false;
                else
                    this.SqlBuilder.Append(",");

                this.QuoteName(item.Key.Name);
                this.SqlBuilder.Append("=");

                DbExpression valExp = item.Value.OptimizeDbExpression();
                AmendDbInfo(item.Key, valExp);
                valExp.Accept(this.ValueExpressionVisitor);
            }
            this.BuildWhereState(exp.Condition);
            return exp;
        }

        public override DbExpression Visit(DbDeleteExpression exp)
        {
            this.SqlBuilder.Append("DELETE ");
            this.QuoteName(exp.Table.Name);
            this.BuildWhereState(exp.Condition);
            return exp;
        }

        protected void QuoteName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("name");
            }
            this.SqlBuilder.Append("[", name, "]");
        }

        protected void BuildCastState(DbExpression castExp, string targetDbTypeString)
        {
            this.SqlBuilder.Append("CAST(");
            castExp.Accept(this);
            this.SqlBuilder.Append(" AS ", targetDbTypeString, ")");
        }
        protected void BuildCastState(object castObject, string targetDbTypeString)
        {
            this.SqlBuilder.Append("CAST(", castObject, " AS ", targetDbTypeString, ")");
        }

        protected bool IsDatePart(DbMemberExpression exp)
        {
            MemberInfo member = exp.Member;
            if (member == UtilConstants.PropertyInfo_DateTime_Year)
            {
                DbFunction_DATEPART(this, "YEAR", exp.Expression);
                return true;
            }
            if (member == UtilConstants.PropertyInfo_DateTime_Month)
            {
                DbFunction_DATEPART(this, "MONTH", exp.Expression);
                return true;
            }
            if (member == UtilConstants.PropertyInfo_DateTime_Day)
            {
                DbFunction_DATEPART(this, "DAY", exp.Expression);
                return true;
            }
            if (member == UtilConstants.PropertyInfo_DateTime_Hour)
            {
                DbFunction_DATEPART(this, "HOUR", exp.Expression);
                return true;
            }
            if (member == UtilConstants.PropertyInfo_DateTime_Minute)
            {
                DbFunction_DATEPART(this, "MINUTE", exp.Expression);
                return true;
            }
            if (member == UtilConstants.PropertyInfo_DateTime_Second)
            {
                DbFunction_DATEPART(this, "SECOND", exp.Expression);
                return true;
            }
            if (member == UtilConstants.PropertyInfo_DateTime_Millisecond)
            {
                DbFunction_DATEPART(this, "MILLISECOND", exp.Expression);
                return true;
            }
            if (member == UtilConstants.PropertyInfo_DateTime_DayOfWeek)
            {
                this.SqlBuilder.Append("(");
                DbFunction_DATEPART(this, "WEEKDAY", exp.Expression);
                this.SqlBuilder.Append(" - 1)");

                return true;
            }
            return false;
        }

        private void ConcatOperands(IEnumerable<DbExpression> operands, string connector)
        {
            this.SqlBuilder.Append("(");

            bool first = true;
            foreach (DbExpression operand in operands)
            {
                if (first)
                    first = false;
                else
                    this.SqlBuilder.Append(connector);

                operand.Accept(this);
            }

            this.SqlBuilder.Append(")");
            return;
        }

        /// <summary>
        /// 构建常规的查询
        /// </summary>
        /// <param name="exp"></param>
        private void BuildGeneralSql(DbSqlQueryExpression exp)
        {
            this.SqlBuilder.Append("SELECT ");
            if (exp.TakeCount != null)
                this.SqlBuilder.Append("TOP (", exp.TakeCount.ToString(), ") ");

            List<DbColumnSegment> columns = exp.ColumnSegments;
            for (int i = 0; i < columns.Count; i++)
            {
                DbColumnSegment column = columns[i];
                if (i > 0)
                    this.SqlBuilder.Append(", ");

                this.AppendColumnSegment(column);
            }

            this.SqlBuilder.Append(" FROM ");
            exp.Table.Accept(this);
            this.BuildWhereState(exp.Condition);
            this.BuildGroupState(exp);
            this.BuildOrderState(exp.Orderings);
        }

        /// <summary>
        /// 构建分页查询
        /// </summary>
        /// <param name="exp"></param>
        protected virtual void BuildLimitSql(DbSqlQueryExpression exp)
        {
            this.SqlBuilder.Append("SELECT ");
            if (exp.TakeCount != null)
                this.SqlBuilder.Append("TOP (", exp.TakeCount.ToString(), ") ");

            string tableAlias = "T";

            List<DbColumnSegment> columns = exp.ColumnSegments;
            for (int i = 0; i < columns.Count; i++)
            {
                DbColumnSegment column = columns[i];
                if (i > 0)
                    this.SqlBuilder.Append(",");

                this.QuoteName(tableAlias);
                this.SqlBuilder.Append(".");
                this.QuoteName(column.Alias);
                this.SqlBuilder.Append(" AS ");
                this.QuoteName(column.Alias);
            }

            this.SqlBuilder.Append(" FROM ");
            this.SqlBuilder.Append("(");

            //------------------------//
            this.SqlBuilder.Append("SELECT ");
            for (int i = 0; i < columns.Count; i++)
            {
                DbColumnSegment column = columns[i];
                if (i > 0)
                    this.SqlBuilder.Append(",");

                column.Body.Accept(this.ValueExpressionVisitor);
                this.SqlBuilder.Append(" AS ");
                this.QuoteName(column.Alias);
            }

            List<DbOrdering> orderings = exp.Orderings;
            if (orderings.Count == 0)
            {
                DbOrdering ordering = new DbOrdering(UtilConstants.DbParameter_1, DbOrderType.Asc);
                orderings = new List<DbOrdering>(1);
                orderings.Add(ordering);
            }

            string row_numberName = GenRowNumberName(columns);
            this.SqlBuilder.Append(",ROW_NUMBER() OVER(ORDER BY ");
            this.ConcatOrderings(orderings);
            this.SqlBuilder.Append(") AS ");
            this.QuoteName(row_numberName);
            this.SqlBuilder.Append(" FROM ");
            exp.Table.Accept(this);
            this.BuildWhereState(exp.Condition);
            this.BuildGroupState(exp);
            //------------------------//

            this.SqlBuilder.Append(")");
            this.SqlBuilder.Append(" AS ");
            this.QuoteName(tableAlias);
            this.SqlBuilder.Append(" WHERE ");
            this.QuoteName(tableAlias);
            this.SqlBuilder.Append(".");
            this.QuoteName(row_numberName);
            this.SqlBuilder.Append(" > ");
            this.SqlBuilder.Append(exp.SkipCount.ToString());
        }


        protected void BuildWhereState(DbExpression whereExpression)
        {
            if (whereExpression != null)
            {
                this.SqlBuilder.Append(" WHERE ");
                whereExpression.Accept(this);
            }
        }
        protected void BuildOrderState(List<DbOrdering> orderings)
        {
            if (orderings.Count > 0)
            {
                this.SqlBuilder.Append(" ORDER BY ");
                this.ConcatOrderings(orderings);
            }
        }
        protected void ConcatOrderings(List<DbOrdering> orderings)
        {
            for (int i = 0; i < orderings.Count; i++)
            {
                if (i > 0)
                {
                    this.SqlBuilder.Append(",");
                }

                this.AppendOrdering(orderings[i]);
            }
        }
        protected void BuildGroupState(DbSqlQueryExpression exp)
        {
            var groupSegments = exp.GroupSegments;
            if (groupSegments.Count == 0)
                return;

            this.SqlBuilder.Append(" GROUP BY ");
            for (int i = 0; i < groupSegments.Count; i++)
            {
                if (i > 0)
                    this.SqlBuilder.Append(",");

                groupSegments[i].Accept(this);
            }

            if (exp.HavingCondition != null)
            {
                this.SqlBuilder.Append(" HAVING ");
                exp.HavingCondition.Accept(this);
            }
        }

        protected void AppendTableSegment(DbTableSegment seg)
        {
            seg.Body.Accept(this);
            this.SqlBuilder.Append(" AS ");
            this.QuoteName(seg.Alias);
        }
        protected void AppendColumnSegment(DbColumnSegment seg)
        {
            seg.Body.Accept(this.ValueExpressionVisitor);
            this.SqlBuilder.Append(" AS ");
            this.QuoteName(seg.Alias);
        }
        private void AppendOrdering(DbOrdering ordering)
        {
            if (ordering.OrderType == DbOrderType.Asc)
            {
                ordering.Expression.Accept(this);
                this.SqlBuilder.Append(" ASC");
                return;
            }
            else if (ordering.OrderType == DbOrderType.Desc)
            {
                ordering.Expression.Accept(this);
                this.SqlBuilder.Append(" DESC");
                return;
            }

            throw new NotSupportedException("OrderType: " + ordering.OrderType);
        }

        private void VisitDbJoinTableExpressions(List<DbJoinTableExpression> tables)
        {
            foreach (var table in tables)
            {
                table.Accept(this);
            }
        }
    }
}
