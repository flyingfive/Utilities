using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace FlyingFive.Utils
{
    /// <summary>
    /// 注册表处理工具
    /// </summary>
    public class RegistryUtility
    {
        /// <summary>
        /// 读取注册表的值(32或64位OS)
        /// </summary>
        /// <param name="root">分类根</param>
        /// <param name="registryRath">注册表路径</param>
        /// <param name="settingName">配置键名</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns></returns>
        public static string ReadRegistryValue(Microsoft.Win32.RegistryHive root, string registryRath, string settingName, string defaultValue = "")
        {
            var view = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;
            var node = Microsoft.Win32.RegistryKey.OpenBaseKey(root, view).OpenSubKey(registryRath);
            var value = node.GetValue(settingName, defaultValue).ToString();
            return value;
        }

        /// <summary>
        /// 写入注册表字符串配置，存在则覆盖
        /// </summary>
        /// <param name="root">顶级节点</param>
        /// <param name="registryRath">注册表路径</param>
        /// <param name="settingName">配置键</param>
        /// <param name="value">配置值</param>
        public static void WriteRegistryValue(Microsoft.Win32.RegistryHive root, string registryRath, string settingName, string value)
        {
            var view = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;
            var node = Microsoft.Win32.RegistryKey.OpenBaseKey(root, view).OpenSubKey(registryRath, true);
            if (node.GetValueNames().Contains(settingName))
            {
                node.DeleteValue(settingName);
            }
            node.SetValue(settingName, value, RegistryValueKind.String);
        }

        /// <summary>
        /// 判断注册表子目录是否存在
        /// </summary>
        /// <param name="root">顶级节点</param>
        /// <param name="registryRath">注册表路径</param>
        /// <param name="subKeyName">配置键名</param>
        /// <returns></returns>
        public static bool ExistsRegistrySubKeys(Microsoft.Win32.RegistryHive root, string registryRath, string subKeyName)
        {
            var view = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;
            var node = Microsoft.Win32.RegistryKey.OpenBaseKey(root, view).OpenSubKey(registryRath);
            var exists = node.GetSubKeyNames().Contains(subKeyName);
            return exists;
        }

        /// <summary>
        /// 创建注册表子目录，如果存在则直接返回
        /// </summary>
        /// <param name="root">顶级节点</param>
        /// <param name="parentPath">注册表路径</param>
        /// <param name="subKey">配置键名</param>
        /// <param name="readOnly">是否只读模式</param>
        /// <returns></returns>
        public static RegistryKey CreateSubRegistryKey(Microsoft.Win32.RegistryHive root, string parentPath, string subKey, bool readOnly = false)
        {
            var view = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;
            var node = Microsoft.Win32.RegistryKey.OpenBaseKey(root, view).OpenSubKey(parentPath, true);
            if (node.GetSubKeyNames().Contains(subKey))
            {
                var currentRegistryKey = node.OpenSubKey(subKey, !readOnly);
                return currentRegistryKey;
            }
            var newRegistryKey = node.CreateSubKey(subKey);
            return newRegistryKey;
        }

        /// <summary>
        /// 64位系统下创建32位注册表子目录，如果存在则直接返回
        /// </summary>
        /// <param name="root">顶级节点</param>
        /// <param name="parentPath">注册表路径</param>
        /// <param name="subKey">配置键名</param>
        /// <param name="readOnly"></param>
        /// <returns></returns>
        public static RegistryKey CreateWin32SubRegistryKey(Microsoft.Win32.RegistryHive root, string parentPath, string subKey, bool readOnly = false)
        {
            var node = Microsoft.Win32.RegistryKey.OpenBaseKey(root, RegistryView.Registry32).OpenSubKey(parentPath, true);
            if (node.GetSubKeyNames().Contains(subKey))
            {
                var currentRegistryKey = node.OpenSubKey(subKey, !readOnly);
                return currentRegistryKey;
            }
            var newRegistryKey = node.CreateSubKey(subKey);
            return newRegistryKey;
        }

        /// <summary>
        /// 打开64位环境中的Win32注册表节点
        /// </summary>
        /// <param name="root">顶级节点</param>
        /// <param name="registryPath">注册表路径</param>
        /// <param name="readOnly">是否只读模式</param>
        /// <returns></returns>
        public static RegistryKey OpenWin32RegistryKey(Microsoft.Win32.RegistryHive root, string registryPath, bool readOnly = false)
        {
            var node = Microsoft.Win32.RegistryKey.OpenBaseKey(root, RegistryView.Registry32).OpenSubKey(registryPath, !readOnly);
            return node;
        }

        /// <summary>
        /// 打开注册表节点
        /// </summary>
        /// <param name="root">顶级节点</param>
        /// <param name="registryPath">注册表路径</param>
        /// <param name="readOnly">是否只读模式</param>
        /// <returns></returns>
        public static RegistryKey OpenRegistryKey(Microsoft.Win32.RegistryHive root, string registryPath, bool readOnly = false)
        {
            var view = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;
            var node = Microsoft.Win32.RegistryKey.OpenBaseKey(root, view).OpenSubKey(registryPath, !readOnly);
            return node;
        }

        /// <summary>
        /// 写入注册表字符串配置，存在则覆盖
        /// </summary>
        /// <param name="registryKey">注册表节点</param>
        /// <param name="keyName">配置键名</param>
        /// <param name="value">配置值</param>
        public static void WriteStringValue(RegistryKey registryKey, string keyName, string value)
        {
            if (registryKey.GetValueNames().Contains(keyName))
            {
                registryKey.DeleteValue(keyName);
            }
            registryKey.SetValue(keyName, value, RegistryValueKind.String);
        }

        /// <summary>
        /// 读取注册表指定路径下的所有配置键名
        /// </summary>
        /// <param name="root"></param>
        /// <param name="registryRath"></param>
        /// <returns></returns>
        public static string[] ReadRegistryNames(Microsoft.Win32.RegistryHive root, string registryRath)
        {
            var view = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;
            var node = Microsoft.Win32.RegistryKey.OpenBaseKey(root, view).OpenSubKey(registryRath);
            var names = node.GetValueNames();
            return names;
        }
    }
}
