using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace FlyingFive.Utils
{
    /// <summary>
    /// 应用程序域内的类型查找器
    /// </summary>
    public class AppDomainTypeFinder : ITypeFinder
    {
        /// <summary>
        /// 当前.NET应用程序
        /// </summary>
        public AppDomain App
        {
            get { return AppDomain.CurrentDomain; }
        }

        private bool _loadAppDomainAssemblies = true;
        /// <summary>
        /// 是否加载程序集
        /// </summary>
        public bool LoadAppDomainAssemblies
        {
            get { return _loadAppDomainAssemblies; }
            set { _loadAppDomainAssemblies = value; }
        }

        private string _assemblySkipLoadingPattern = "^System|^mscorlib|^Microsoft|^CppCodeProvider|^VJSharpCodeProvider|^WebDev|^Castle|^Iesi|^log4net|^NHibernate|^nunit|^TestDriven|^MbUnit|^Rhino|^QuickGraph|^TestFu|^Telerik|^ComponentArt|^MvcContrib|^Antlr3|^Remotion|^Recaptcha|^Autofac|^AutoMapper|^EntityFramework";
        /// <summary>
        /// 忽略加载的程序集
        /// </summary>
        public string AssemblySkipLoadingPattern
        {
            get { return _assemblySkipLoadingPattern; }
            set { _assemblySkipLoadingPattern = value; }
        }

        private string _assemblyRestrictToLoadingPattern = ".*";
        /// <summary>
        /// 程序集加载限定符
        /// </summary>
        public string AssemblyRestrictToLoadingPattern
        {
            get { return _assemblyRestrictToLoadingPattern; }
            set { _assemblyRestrictToLoadingPattern = value; }
        }

        private IList<string> _assemblyNames = new List<string>();
        /// <summary>
        /// 程序集名称
        /// </summary>
        public IList<string> AssemblyNames
        {
            get { return _assemblyNames; }
            set { _assemblyNames = value; }
        }

        #region ITypeFinder 成员

        /// <summary>
        /// 查询类型
        /// </summary>
        /// <param name="fullName">类型全名</param>
        /// <param name="stringComparison">名称比较规则</param>
        /// <returns></returns>
        public Type FindClassOfType(string fullName, StringComparison stringComparison)
        {
            if (string.IsNullOrWhiteSpace(fullName)) { return null; }
            var assemblies = GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var type = assembly.GetTypes()
                    .Where(t => t.FullName.Equals(fullName, stringComparison)).SingleOrDefault();
                if (type == null) { continue; }
                return type;
            }
            return null;
        }

        /// <summary>
        /// 获取程序集
        /// </summary>
        /// <returns></returns>
        public virtual IList<Assembly> GetAssemblies()
        {
            var addedAssemblyNames = new List<string>();
            var assemblies = new List<Assembly>();

            if (LoadAppDomainAssemblies)
                AddAssembliesInAppDomain(addedAssemblyNames, assemblies);
            AddConfiguredAssemblies(addedAssemblyNames, assemblies);

            return assemblies;
        }

        /// <summary>
        /// 查询类型
        /// </summary>
        /// <typeparam name="T">查询的类型</typeparam>
        /// <param name="onlyConcreteClasses">是否只匹配Class</param>
        /// <returns></returns>
        public IEnumerable<Type> FindClassesOfType<T>(bool onlyConcreteClasses = true)
        {
            return FindClassesOfType(typeof(T), onlyConcreteClasses);
        }

        /// <summary>
        /// 查询类型
        /// </summary>
        /// <typeparam name="T">查询的类型</typeparam>
        /// <param name="assemblies">程序集范围</param>
        /// <param name="onlyConcreteClasses">是否只匹配Class</param>
        /// <returns></returns>
        public IEnumerable<Type> FindClassesOfType<T>(IEnumerable<Assembly> assemblies, bool onlyConcreteClasses = true)
        {
            return FindClassesOfType(typeof(T), assemblies, onlyConcreteClasses);
        }

        /// <summary>
        /// 查询类型
        /// </summary>
        /// <param name="assignTypeFrom">分配的源类型</param>
        /// <param name="onlyConcreteClasses">是否只匹配Class</param>
        /// <returns></returns>
        public IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, bool onlyConcreteClasses = true)
        {
            return FindClassesOfType(assignTypeFrom, GetAssemblies(), onlyConcreteClasses);
        }

        /// <summary>
        /// 查询类型
        /// </summary>
        /// <param name="assignTypeFrom">分配的源类型</param>
        /// <param name="assemblies">程序集范围</param>
        /// <param name="onlyConcreteClasses">是否只匹配Class</param>
        /// <returns></returns>
        public IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, IEnumerable<Assembly> assemblies, bool onlyConcreteClasses = true)
        {
            var result = new List<Type>();
            try
            {
                foreach (var a in assemblies)
                {
                    foreach (var t in a.GetTypes())
                    {
                        if (assignTypeFrom.IsAssignableFrom(t) || (assignTypeFrom.IsGenericTypeDefinition && DoesTypeImplementOpenGeneric(t, assignTypeFrom)))
                        {
                            if (!t.IsInterface)
                            {
                                if (onlyConcreteClasses)
                                {
                                    if (t.IsClass && !t.IsAbstract)
                                    {
                                        result.Add(t);
                                    }
                                }
                                else
                                {
                                    result.Add(t);
                                }
                            }
                        }
                        //liubq: 泛型父类
                        if (t.BaseType != null && t.BaseType.IsGenericType && assignTypeFrom.IsGenericTypeDefinition)
                        {
                            if (t.BaseType.GetGenericTypeDefinition() == assignTypeFrom)
                            {
                                result.Add(t);
                            }
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                var msg = string.Empty;
                foreach (var e in ex.LoaderExceptions)
                    msg += e.Message + Environment.NewLine;

                var fail = new Exception(msg, ex);
                //Debug.WriteLine(fail.Message, fail);

                throw fail;
            }
            return result;
        }

        #endregion

        /// <summary>
        /// 程序集名称是否匹配
        /// </summary>
        /// <param name="assemblyFullName">程序集全名</param>
        /// <returns></returns>
        public virtual bool Matches(string assemblyFullName)
        {
            return !Matches(assemblyFullName, AssemblySkipLoadingPattern)
                   && Matches(assemblyFullName, AssemblyRestrictToLoadingPattern);
        }
        /// <summary>
        /// 类型是否实现泛型
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="openGeneric">泛型类型</param>
        /// <returns></returns>
        protected virtual bool DoesTypeImplementOpenGeneric(Type type, Type openGeneric)
        {
            try
            {
                var genericTypeDefinition = openGeneric.GetGenericTypeDefinition();
                foreach (var implementedInterface in type.FindInterfaces((objType, objCriteria) => true, null))
                {
                    if (!implementedInterface.IsGenericType)
                        continue;

                    var isMatch = genericTypeDefinition.IsAssignableFrom(implementedInterface.GetGenericTypeDefinition());
                    return isMatch;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 添加配置了的程序集
        /// </summary>
        /// <param name="addedAssemblyNames"></param>
        /// <param name="assemblies"></param>
        /// <param name="assemblies"></param>
        protected virtual void AddConfiguredAssemblies(List<string> addedAssemblyNames, List<Assembly> assemblies)
        {
            foreach (string assemblyName in AssemblyNames)
            {
                Assembly assembly = Assembly.Load(assemblyName);
                if (!addedAssemblyNames.Contains(assembly.FullName))
                {
                    assemblies.Add(assembly);
                    addedAssemblyNames.Add(assembly.FullName);
                }
            }
        }

        /// <summary>
        /// 程序集名称是否匹配
        /// </summary>
        /// <param name="assemblyFullName"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        protected virtual bool Matches(string assemblyFullName, string pattern)
        {
            return Regex.IsMatch(assemblyFullName, pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        private void AddAssembliesInAppDomain(List<string> addedAssemblyNames, List<Assembly> assemblies)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (Matches(assembly.FullName))
                {
                    if (!addedAssemblyNames.Contains(assembly.FullName))
                    {
                        assemblies.Add(assembly);
                        addedAssemblyNames.Add(assembly.FullName);
                    }
                }
            }
        }
    }
}
