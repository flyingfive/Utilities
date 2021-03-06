﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FlyingFive
{
    /// <summary>
    /// 提供类型查找器能力
    /// </summary>
    public interface ITypeFinder
    {
        /// <summary>
        /// 获取应用程序域的bin目录
        /// </summary>
        /// <returns></returns>
        string GetBinDirectory();
        /// <summary>
        /// 查找具体类型
        /// </summary>
        /// <param name="fullName">类型完全限定名</param>
        /// <param name="stringComparison">大小写匹配模式</param>
        /// <returns></returns>
        Type FindClassOfType(string fullName, StringComparison stringComparison);
        /// <summary>
        /// 获取程序集集合
        /// </summary>
        /// <returns></returns>
        IList<Assembly> GetAssemblies();
        /// <summary>
        /// 查找类型
        /// </summary>
        /// <typeparam name="T">查找的类型</typeparam>
        /// <param name="onlyConcreteClasses">是否可实例化的具体类型</param>
        /// <returns></returns>
        IEnumerable<Type> FindClassesOfType<T>(bool onlyConcreteClasses = true);
        /// <summary>
        /// 查找类型
        /// </summary>
        /// <typeparam name="T">查找的类型</typeparam>
        /// <param name="assemblies">查找的程序集范围</param>
        /// <param name="onlyConcreteClasses">是否可实例化的具体类型</param>
        /// <returns></returns>
        IEnumerable<Type> FindClassesOfType<T>(IEnumerable<Assembly> assemblies, bool onlyConcreteClasses = true);
        /// <summary>
        /// 查找类型
        /// </summary>
        /// <param name="assignTypeFrom">查找的类型</param>
        /// <param name="onlyConcreteClasses">是否可实例化的具体类型</param>
        /// <returns></returns>
        IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, bool onlyConcreteClasses = true);
        /// <summary>
        /// 查找类型
        /// </summary>
        /// <param name="assignTypeFrom">查找的类型</param>
        /// <param name="assemblies">查找的程序集范围</param>
        /// <param name="onlyConcreteClasses">是否可实例化的具体类型</param>
        /// <returns></returns>
        IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, IEnumerable<Assembly> assemblies, bool onlyConcreteClasses = true);
    }
}
