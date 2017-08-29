using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FlyingFive.Data.Descriptors
{
    /// <summary>
    /// 表示实体成员的描述
    /// </summary>
    public abstract class EntityMemberDescriptor
    {
        private IDictionary<Type, Attribute> _customAttributes = null;
        /// <summary>
        /// 成员所声明实体的描述
        /// </summary>
        public EntityTypeDescriptor DeclaringTypeDescriptor { get; private set; }
        /// <summary>
        /// 描述的实体成员
        /// </summary>
        public MemberInfo MemberInfo { get; private set; }
        /// <summary>
        /// 成员数据类型
        /// </summary>
        public Type MemberInfoType { get; private set; }

        protected EntityMemberDescriptor(MemberInfo memberInfo, EntityTypeDescriptor declaringTypeDescriptor)
        {
            this.MemberInfo = memberInfo;
            this.DeclaringTypeDescriptor = declaringTypeDescriptor;
            this.MemberInfoType = this.MemberInfo.GetMemberType();
            this._customAttributes = new Dictionary<Type, Attribute>();
        }
    }
}
