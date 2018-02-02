using FlyingFive.Data.DbExpressions;
using FlyingFive.Data.Descriptors;
using FlyingFive.Data.Schema;
using FlyingFive.Data.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace FlyingFive.Data
{
    public abstract partial class DbContext
    {
        private static Expression<Func<TEntity, bool>> BuildPredicate<TEntity>(object key)
        {
            /*
             * key:
             * 如果实体是单一主键，则传入的 key 与主键属性类型相同的值
             * 如果实体是多主键，则传入的 key 须是包含了与实体主键类型相同的属性的对象，如：new { Key1 = "1", Key2 = "2" }
             */

            //Utils.CheckNull(key);
            if (key == null) { throw new ArgumentNullException("参数: key不能为NULL"); }
            Type entityType = typeof(TEntity);
            var typeDescriptor = EntityTypeDescriptor.GetEntityTypeDescriptor(entityType);
            EnsureEntityHasPrimaryKey(typeDescriptor);

            var keyValueMap = new List<KeyValuePair<MemberInfo, object>>();

            if (typeDescriptor.PrimaryKeys.Count == 1)
            {
                keyValueMap.Add(new KeyValuePair<MemberInfo, object>(typeDescriptor.PrimaryKeys[0].MemberInfo, key));
            }
            else
            {
                /*
                 * key: new { Key1 = "1", Key2 = "2" }
                 */

                object multipleKeyObject = key;
                Type multipleKeyObjectType = multipleKeyObject.GetType();

                for (int i = 0; i < typeDescriptor.PrimaryKeys.Count; i++)
                {
                    MappingMemberDescriptor keyMemberDescriptor = typeDescriptor.PrimaryKeys[i];
                    MemberInfo keyMember = multipleKeyObjectType.GetProperty(keyMemberDescriptor.MemberInfo.Name);
                    if (keyMember == null)
                        throw new ArgumentException(string.Format("The input object does not define property for key '{0}'.", keyMemberDescriptor.MemberInfo.Name));

                    object value = keyMember.GetMemberValue(multipleKeyObject);
                    if (value == null)
                        throw new ArgumentException(string.Format("The primary key '{0}' could not be null.", keyMemberDescriptor.MemberInfo.Name));

                    keyValueMap.Add(new KeyValuePair<MemberInfo, object>(keyMemberDescriptor.MemberInfo, value));
                }
            }

            ParameterExpression parameter = Expression.Parameter(entityType, "a");
            Expression lambdaBody = null;

            foreach (var keyValue in keyValueMap)
            {
                Expression propOrField = Expression.PropertyOrField(parameter, keyValue.Key.Name);
                Expression wrappedValue = Extensions.MakeWrapperAccess(keyValue.Value, keyValue.Key.GetMemberType());
                Expression e = Expression.Equal(propOrField, wrappedValue);
                lambdaBody = lambdaBody == null ? e : Expression.AndAlso(lambdaBody, e);
            }

            Expression<Func<TEntity, bool>> predicate = Expression.Lambda<Func<TEntity, bool>>(lambdaBody, parameter);

            return predicate;
        }

        private static void EnsureEntityHasPrimaryKey(EntityTypeDescriptor typeDescriptor)
        {
            if (!typeDescriptor.HasPrimaryKeys)
            {
                throw new DataAccessException(string.Format("实体类型 '{0}' 没有定义任何主键映射.", typeDescriptor.EntityType.FullName));
            }
        }
        private static List<KeyValuePair<DbJoinType, Expression>> ResolveJoinInfo(LambdaExpression joinInfoExp)
        {
            /*
             * Useage:
             * var view = context.JoinQuery<User, City, Province, User, City>((user, city, province, user1, city1) => new object[] 
             * { 
             *     JoinType.LeftJoin, user.CityId == city.Id, 
             *     JoinType.RightJoin, city.ProvinceId == province.Id,
             *     JoinType.InnerJoin,user.Id==user1.Id,
             *     JoinType.FullJoin,city.Id==city1.Id
             * }).Select((user, city, province, user1, city1) => new { User = user, City = city, Province = province, User1 = user1, City1 = city1 });
             * 
             * To resolve join infomation:
             * JoinType.LeftJoin, user.CityId == city.Id               index of joinType is 0
             * JoinType.RightJoin, city.ProvinceId == province.Id      index of joinType is 2
             * JoinType.InnerJoin,user.Id==user1.Id                    index of joinType is 4
             * JoinType.FullJoin,city.Id==city1.Id                     index of joinType is 6
            */

            NewArrayExpression body = joinInfoExp.Body as NewArrayExpression;

            if (body == null)
            {
                throw new ArgumentException(string.Format("Invalid join infomation '{0}'. The correct usage is like: {1}", joinInfoExp, "context.JoinQuery<User, City>((user, city) => new object[] { JoinType.LeftJoin, user.CityId == city.Id })"));
            }

            var ret = new List<KeyValuePair<DbJoinType, Expression>>();

            if ((joinInfoExp.Parameters.Count - 1) * 2 != body.Expressions.Count)
            {
                throw new ArgumentException(string.Format("Invalid join infomation '{0}'.", joinInfoExp));
            }

            for (int i = 0; i < joinInfoExp.Parameters.Count - 1; i++)
            {
                /*
                 * 0  0
                 * 1  2
                 * 2  4
                 * 3  6
                 * ...
                 */
                int indexOfJoinType = i * 2;

                Expression joinTypeExpression = body.Expressions[indexOfJoinType];
                object inputJoinType = ExpressionEvaluator.Evaluate(joinTypeExpression);
                if (inputJoinType == null || inputJoinType.GetType() != typeof(DbJoinType))
                {
                    throw new ArgumentException(string.Format("Not support '{0}', please pass correct type of 'Chloe.JoinType'.", joinTypeExpression));
                }
                /*
                 * The next expression of join type must be join condition.
                 */
                Expression joinCondition = body.Expressions[indexOfJoinType + 1].StripConvert();

                if (joinCondition.Type != UtilConstants.TypeOfBoolean)
                {
                    throw new ArgumentException(string.Format("Not support '{0}', please pass correct join condition.", joinCondition));
                }

                ParameterExpression[] parameters = joinInfoExp.Parameters.Take(i + 2).ToArray();

                List<Type> typeArguments = parameters.Select(a => a.Type).ToList();
                typeArguments.Add(UtilConstants.TypeOfBoolean);

                Type delegateType = UtilConstants.GetFuncDelegateType(typeArguments.ToArray());
                LambdaExpression lambdaOfJoinCondition = Expression.Lambda(delegateType, joinCondition, parameters);

                ret.Add(new KeyValuePair<DbJoinType, Expression>((DbJoinType)inputJoinType, lambdaOfJoinCondition));
            }

            return ret;
        }
        private static Dictionary<MappingMemberDescriptor, object> CreateKeyValueMap(EntityTypeDescriptor typeDescriptor)
        {
            Dictionary<MappingMemberDescriptor, object> keyValueMap = new Dictionary<MappingMemberDescriptor, object>();
            foreach (MappingMemberDescriptor keyMemberDescriptor in typeDescriptor.PrimaryKeys)
            {
                keyValueMap.Add(keyMemberDescriptor, null);
            }

            return keyValueMap;
        }
        private static DbExpression MakeCondition(Dictionary<MappingMemberDescriptor, object> keyValueMap, DbTable dbTable)
        {
            DbExpression conditionExp = null;
            foreach (var kv in keyValueMap)
            {
                MappingMemberDescriptor keyMemberDescriptor = kv.Key;
                object keyVal = kv.Value;

                if (keyVal == null)
                    throw new ArgumentException(string.Format("The primary key '{0}' could not be null.", keyMemberDescriptor.MemberInfo.Name));

                DbExpression left = new DbColumnAccessExpression(dbTable, keyMemberDescriptor.Column);
                DbExpression right = DbExpression.Parameter(keyVal, keyMemberDescriptor.MemberInfoType);
                DbExpression equalExp = new DbEqualExpression(left, right);
                conditionExp = conditionExp == null ? equalExp : DbExpression.And(conditionExp, equalExp);
            }

            return conditionExp;
        }
    }
}
