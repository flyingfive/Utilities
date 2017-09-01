using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Infrastructure
{
    /// <summary>
    /// 受支持的聚合操作函数
    /// </summary>
    public static class AggregateFunctions
    {
        public static int Count()
        {
            return 0;
        }
        public static long LongCount()
        {
            return 0;
        }

        public static TResult Max<TResult>(TResult p)
        {
            return p;
        }
        public static TResult Min<TResult>(TResult p)
        {
            return p;
        }

        public static int Sum(int p)
        {
            return p;
        }
        public static int? Sum(int? p)
        {
            return p;
        }
        public static long Sum(long p)
        {
            return p;
        }
        public static long? Sum(long? p)
        {
            return p;
        }
        public static decimal Sum(decimal p)
        {
            return p;
        }
        public static decimal? Sum(decimal? p)
        {
            return p;
        }
        public static double Sum(double p)
        {
            return p;
        }
        public static double? Sum(double? p)
        {
            return p;
        }
        public static float Sum(float p)
        {
            return p;
        }
        public static float? Sum(float? p)
        {
            return p;
        }

        public static double Average(int p)
        {
            return p;
        }
        public static double? Average(int? p)
        {
            return p;
        }
        public static double Average(long p)
        {
            return p;
        }
        public static double? Average(long? p)
        {
            return p;
        }
        public static decimal Average(decimal p)
        {
            return p;
        }
        public static decimal? Average(decimal? p)
        {
            return p;
        }
        public static double Average(double p)
        {
            return p;
        }
        public static double? Average(double? p)
        {
            return p;
        }
        public static float Average(float p)
        {
            return p;
        }
        public static float? Average(float? p)
        {
            return p;
        }
    }
}
