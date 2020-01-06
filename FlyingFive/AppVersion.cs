using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

namespace FlyingFive
{
    /// <summary>
    /// 表示程序版本
    /// </summary>
    [Serializable]
    public class AppVersion
    {
        /// <summary>
        /// 年份减去的起始基值
        /// </summary>
        public const int BASE_YEAR = 2014;
        /// <summary>
        /// 产品发布起始年份(如果存在则为设置值,不存在则为:BASE_YEAR)
        /// </summary>
        public int BirthYear { get; private set; }

        private int _year = 0;

        /// <summary>
        /// 年份
        /// </summary>
        public int Year
        {
            get { return _year; }
            set
            {
                this.GetBirthYear();
                if (value < 1 || value == this.BirthYear) { value = 1; }
                if (value > this.BirthYear) { value = value - this.BirthYear; }
                _year = value;
            }
        }
        /// <summary>
        /// 月份
        /// </summary>
        public int Month { get; set; }
        /// <summary>
        /// 日期
        /// </summary>
        public int Day { get; set; }
        /// <summary>
        /// 序号,修订版本
        /// </summary>
        public int No { get; set; }

        /// <summary>
        /// 版本发布日期
        /// </summary>
        public DateTime PublishDate
        {
            get
            {
                this.GetBirthYear();
                var year = _year == 1 ? 0 : _year - 1;
                year += this.BirthYear;
                return new DateTime(year, Month < 1 ? 1 : Month, Day < 1 ? 1 : Day);
            }
        }


        /// <summary>
        /// 最小版本,不应小于此版本
        /// </summary>
        public static readonly AppVersion MinimalVersion = new AppVersion() { Year = 1, Month = 1, Day = 1, No = 0, BirthYear = 2014 };

        /// <summary>
        /// 获取版本对象的Hash值
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            var hash = this.Year.GetHashCode() ^ this.Month.GetHashCode() ^ this.Day.GetHashCode() ^ this.No.GetHashCode();
            return hash;
        }

        /// <summary>
        /// 判断指定对象是否对当前实例相等
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var version = obj as AppVersion;
            if (version == null) { return false; }
            return version.Year == this.Year &&
                version.Month == this.Month &&
                version.Day == this.Day &&
                version.No == this.No;
        }

        /// <summary>
        /// 比较两个版本信息是否相等
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool operator ==(AppVersion v1, AppVersion v2)
        {
            if (Object.ReferenceEquals(v1, null) && Object.ReferenceEquals(v2, null)) { return true; }
            if (Object.ReferenceEquals(v1, null) && !Object.ReferenceEquals(v2, null)) { return false; }
            if (!Object.ReferenceEquals(v1, null) && Object.ReferenceEquals(v2, null)) { return false; }
            return v1.Year == v2.Year &&
                v1.Month == v2.Month &&
                v1.Day == v2.Day &&
                v1.No == v2.No;
        }

        /// <summary>
        /// 比较两个版本号是否不等
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool operator !=(AppVersion v1, AppVersion v2)
        {
            return !(v1 == v2);
        }

        /// <summary>
        /// 比较版本号大小
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool operator >(AppVersion v1, AppVersion v2)
        {
            if (Object.ReferenceEquals(v1, null) || Object.ReferenceEquals(v2, null)) { return false; }
            if (v1.Year > v2.Year)
            {
                return true;
            }
            if (v1.Year < v2.Year)
            {
                return false;
            }
            //等于的下继续比下一位值，大于，小于都直接返回
            if (v1.Month > v2.Month)
            {
                return true;
            }
            if (v1.Month < v2.Month)
            {
                return false;
            }
            if (v1.Day > v2.Day)
            {
                return true;
            }
            if (v1.Day < v2.Day)
            {
                return false;
            }
            if (v1.No > v2.No) { return true; }         //前面3位都相等,最后一位大于则true,否则小于或等于则为false
            return false;
        }

        /// <summary>
        /// 大于等于判断
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool operator >=(AppVersion v1, AppVersion v2)
        {
            return v1 > v2 || v1 == v2;
        }

        /// <summary>
        /// 小于判断
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool operator <(AppVersion v1, AppVersion v2)
        {
            if (Object.ReferenceEquals(v1, null) || Object.ReferenceEquals(v2, null)) { return false; }
            if (v1.Year < v2.Year)
            {
                return true;
            }
            if (v1.Year > v2.Year)
            {
                return false;
            }
            if (v1.Month < v2.Month)
            {
                return true;
            }
            if (v1.Month > v2.Month)
            {
                return false;
            }
            if (v1.Day < v2.Day)
            {
                return true;
            }
            if (v1.Day > v2.Day)
            {
                return false;
            }
            if (v1.No < v2.No) { return true; }
            return false;
        }

        /// <summary>
        /// 小于等于判断
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool operator <=(AppVersion v1, AppVersion v2)
        {
            return v1 < v2 || v1 == v2;
        }

        /// <summary>
        /// 返回版本信息的字符串形式
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}.{1}.{2}.{3}", Year.ToString(), Month.ToString(), Day.ToString(), No.ToString());
        }

        /// <summary>
        /// 尝试版本转换
        /// </summary>
        /// <param name="version">表示版本的字符串</param>
        /// <param name="throwExceptionIfOverflow">版本溢出时是否引发异常,false:返回null</param>
        /// <returns></returns>
        public static AppVersion TryParse(string version, bool throwExceptionIfOverflow = true)
        {
            if (string.IsNullOrWhiteSpace(version)) { return null; }
            var bits = version.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (bits.Length < 3) { return null; }
            var year = 0;
            int month = 0;
            int day = 0;
            int no = 0;
            if (!int.TryParse(bits[0], out year)) { return null; }
            if (!int.TryParse(bits[1], out month)) { return null; }
            if (!int.TryParse(bits[2], out day)) { return null; }
            if (!int.TryParse(bits[3], out no)) { return null; }
            var ver = new AppVersion() { Year = year, Month = month, Day = day, No = no };
            if (ver < MinimalVersion)
            {
                if (throwExceptionIfOverflow)
                {
                    throw new AppVersionOverflowException(string.Format("程序版本:{0}不正确!", ver.ToString()));
                }
                else
                {
                    return null;
                }
            }
            return ver;
        }

        /// <summary>
        /// 尝试版本转换
        /// </summary>
        /// <param name="versionString">表示版本的字符串</param>
        /// <param name="version">转换成功时的版本实例</param>
        /// <returns></returns>
        public static bool TryParse(string versionString, out AppVersion version)
        {
            version = TryParse(versionString, false);
            return version != null;
        }

        private AppVersion()
        {
            BirthYear = 0;
            GetBirthYear();
        }

        private static object _locker = new object();

        /// <summary>
        /// 应用程序的当前版本
        /// </summary>
        public static AppVersion CurrentVersion
        {
            get
            {
                lock (_locker)
                {
                    if (Singleton<AppVersion>.Instance == null)
                    {
                        lock (_locker)
                        {
                            var entryAssembly = AppDomain.CurrentDomain.GetEntryAssembly();
                            var version = AppVersion.TryParse(entryAssembly.GetName().Version.ToString());
                            Singleton<AppVersion>.Instance = version;
                        }
                    }
                }
                return Singleton<AppVersion>.Instance;
            }
        }

        /// <summary>
        /// 从应用程序入口程序集的AssemblyCopyrightAttribute特性中获取产品发布的起始年份
        /// </summary>
        /// <returns></returns>
        public int GetBirthYear()
        {
            if (BirthYear != 0) { return BirthYear; }
            var entryAssembly = AppDomain.CurrentDomain.GetEntryAssembly();
            var copyright = entryAssembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false).FirstOrDefault() as AssemblyCopyrightAttribute;
            if (copyright != null)
            {
                var year = Regex.Match(copyright.Copyright, "(20[0-9]{2})").Value;
                if (year.IsInt(false))
                {
                    this.BirthYear = Convert.ToInt32(year);
                    return this.BirthYear;
                }
            }
            this.BirthYear = BASE_YEAR;
            return this.BirthYear;
        }
    }


    /// <summary>
    /// 程序版本溢出(过大/过小)时引发的异常
    /// </summary>
    [Serializable]
    public class AppVersionOverflowException : Exception
    {
        /// <summary>
        /// 初始化一个版本溢出异常实例
        /// </summary>
        /// <param name="message"></param>
        public AppVersionOverflowException(string message) : base(message) { }
        /// <summary>
        /// 初始化一个版本溢出异常实例
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public AppVersionOverflowException(string message, Exception innerException) : base(message, innerException) { }
        /// <summary>
        /// 初始化一个版本溢出异常实例
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public AppVersionOverflowException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
