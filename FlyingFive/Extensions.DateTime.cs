using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive
{
    /// <summary>
    /// 日期类扩展
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// 计算两个时间间的间隔
        /// </summary>
        /// <param name="begin">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <param name="dateformat">间隔格式(y:年,M:月,d:天,h:小时,m:分钟,s:秒,fff:毫秒)</param>
        /// <returns></returns>
        public static long CompareInterval(this DateTime begin, DateTime end, string dateformat = "d")
        {
            long interval = begin.Ticks - end.Ticks;
            DateTime dt11;
            DateTime dt22;
            switch (dateformat)
            {
                //case "fffffff"://100毫微妙   
                //    break;
                //case "ffffff"://微妙   
                //    interval /= 10;
                //    break;
                case "fff"://毫秒   
                    interval /= 10000;
                    break;
                case "s"://秒   
                    interval /= 10000000;
                    break;
                case "m"://分鐘   
                    interval /= 600000000;
                    break;
                case "h"://小時   
                    interval /= 36000000000;
                    break;
                case "d"://天   
                    interval /= 864000000000;
                    break;
                case "M"://月   
                    dt11 = (begin.CompareTo(end) >= 0) ? end : begin;
                    dt22 = (begin.CompareTo(end) >= 0) ? begin : end;
                    interval = -1;
                    while (dt22.CompareTo(dt11) >= 0)
                    {
                        interval++;
                        dt11 = dt11.AddMonths(1);
                    }
                    break;
                case "y"://年   
                    dt11 = (begin.CompareTo(end) >= 0) ? end : begin;
                    dt22 = (begin.CompareTo(end) >= 0) ? begin : end;
                    interval = -1;
                    while (dt22.CompareTo(dt11) >= 0)
                    {
                        interval++;
                        dt11 = dt11.AddMonths(1);
                    }
                    interval /= 12;
                    break;
            }
            return interval;
        }

        /// <summary>
        /// 判断当前时间是否在时间段内(不比较日期)
        /// </summary>
        /// <param name="current">比较的时间</param>
        /// <param name="begin">范围起始时间</param>
        /// <param name="end">范围截止时间</param>
        /// <returns></returns>
        public static bool IsBetweenTime(this DateTime current, DateTime begin, DateTime end)
        {
            var dspWorkingDayAM = begin.TimeOfDay;
            var dspWorkingDayPM = end.TimeOfDay;

            TimeSpan dspNow = current.TimeOfDay;
            if (dspWorkingDayPM < dspWorkingDayAM) //截止时间小于开始时间，表示跨天了
            {
                if (current.TimeOfDay <= dspWorkingDayPM || current.TimeOfDay >= dspWorkingDayAM)
                {
                    return true;
                }
            }
            if (current.TimeOfDay >= dspWorkingDayAM && current.TimeOfDay <= dspWorkingDayPM)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 判断当前时间是否在日期段内(不比较时间)
        /// </summary>
        /// <param name="current">比较的日期</param>
        /// <param name="begin">范围起始日期</param>
        /// <param name="end">范围截止日期</param>
        /// <returns></returns>
        public static bool IsBetweenDate(this DateTime current, DateTime begin, DateTime end)
        {
            begin = begin.Date;
            end = end.Date;
            current = current.Date;
            var flag = current >= begin && current <= end;
            return flag;
        }

        /// <summary>
        /// 转换指定日期到当天的开始时间
        /// </summary>
        /// <param name="current">任何时间的指定日期</param>
        /// <returns></returns>
        public static DateTime ToBeginTime(this DateTime current)
        {
            return current.Date;
        }

        /// <summary>
        /// 转换指定日期到当天的结束时间
        /// </summary>
        /// <param name="current">任何时间的指定日期</param>
        /// <returns></returns>
        public static DateTime ToEndTime(this DateTime current)
        {
            return current.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
        }
    }
}
