using FlyingFive.Data.DbExpressions;
using FlyingFive.Data.Descriptors;
using FlyingFive.Data.Query.Mapping;
using FlyingFive.Data.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FlyingFive.Data.Query.QueryState
{
    internal sealed class RootQueryState : QueryStateBase
    {
        Type _elementType;
        public RootQueryState(Type elementType, string explicitTableName)
            : base(CreateResultElement(elementType, explicitTableName))
        {
            this._elementType = elementType;
        }

        public override FromQueryResult ToFromQueryResult()
        {
            if (this.Result.Condition == null)
            {
                FromQueryResult result = new FromQueryResult();
                result.FromTable = this.Result.FromTable;
                result.MappingObjectExpression = this.Result.MappingObjectExpression;
                return result;
            }

            return base.ToFromQueryResult();
        }

        static ResultElement CreateResultElement(Type type, string explicitTableName)
        {
            if (type.IsAbstract || type.IsInterface)
                throw new ArgumentException("The type of input can not be abstract class or interface.");

            //TODO init _resultElement
            ResultElement resultElement = new ResultElement();

            EntityTypeDescriptor typeDescriptor = EntityTypeDescriptor.GetEntityTypeDescriptor(type);

            DbTable dbTable = typeDescriptor.Table;
            if (!string.IsNullOrWhiteSpace(explicitTableName)) { dbTable = new DbTable(explicitTableName, dbTable.Schema); }
            string alias = resultElement.GenerateUniqueTableAlias(dbTable.Name);

            resultElement.FromTable = CreateRootTable(dbTable, alias);

            ConstructorInfo constructor = typeDescriptor.EntityType.GetConstructor(Type.EmptyTypes);
            if (constructor == null)
                throw new ArgumentException(string.Format("The type of '{0}' does't define a none parameter constructor.", type.FullName));

            MappingObjectExpression moe = new MappingObjectExpression(constructor);

            DbTableSegment tableExp = resultElement.FromTable.Table;
            DbTable table = new DbTable(alias);

            foreach (MappingMemberDescriptor item in typeDescriptor.MappingMemberDescriptors.Values)
            {
                DbColumnAccessExpression columnAccessExpression = new DbColumnAccessExpression(table, item.Column);

                moe.AddMappingMemberExpression(item.MemberInfo, columnAccessExpression);
                if (item.Column.IsPrimaryKey)
                    moe.PrimaryKey = columnAccessExpression;
            }

            resultElement.MappingObjectExpression = moe;

            return resultElement;
        }
        static DbFromTableExpression CreateRootTable(DbTable table, string alias)
        {
            DbTableExpression tableExp = new DbTableExpression(table);
            DbTableSegment tableSeg = new DbTableSegment(tableExp, alias);
            var fromTableExp = new DbFromTableExpression(tableSeg);
            return fromTableExp;
        }
    }
}
