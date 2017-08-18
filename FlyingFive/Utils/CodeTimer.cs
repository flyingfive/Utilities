using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace FlyingFive.Utils
{
    /// <summary>
    /// 性能检测工具
    /// </summary>
    public static class CodeTimer
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetThreadTimes(IntPtr hThread, out long lpCreationTime, out long lpExitTime, out long lpKernelTime, out long lpUserTime);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetCurrentThread();

        private static long GetCurrentThreadTimes()
        {
            long l;
            long kernelTime, userTimer;
            GetThreadTimes(GetCurrentThread(), out l, out l, out kernelTime, out userTimer);
            return kernelTime + userTimer;
        }

        static CodeTimer()
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
        }

        public static TimerResult Time(string name, int iteration, Action action)
        {
            if (String.IsNullOrEmpty(name))
            {
                return null;
            }
            if (action == null)
            {
                return null;
            }
            //1. Print name
            var result = new TimerResult() { ActionName = name, Iteration = iteration };

            // 2. Record the latest GC counts
            //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.Collect(GC.MaxGeneration);
            int[] gcCounts = new int[GC.MaxGeneration + 1];
            for (int i = 0; i <= GC.MaxGeneration; i++)
            {
                gcCounts[i] = GC.CollectionCount(i);
            }

            // 3. Run action
            Stopwatch watch = new Stopwatch();
            watch.Start();
            long ticksFst = GetCurrentThreadTimes(); //100 nanosecond one tick
            for (int i = 0; i < iteration; i++) action();
            long ticks = GetCurrentThreadTimes() - ticksFst;
            watch.Stop();

            // 4. Print CPU
            result.ElapsedMillisecondsTotal = watch.ElapsedMilliseconds;
            result.ElapsedMillisecondsOnce = (watch.ElapsedMilliseconds / iteration);
            result.CpuTimeTotal = (ticks * 100);
            result.CpuTimeOnce = (ticks * 100 / iteration);

            // 5. Print GC
            for (int i = 0; i <= GC.MaxGeneration; i++)
            {
                int count = GC.CollectionCount(i) - gcCounts[i];
                result.GC.Add(i, count);
            }
            return result;
        }
    }

    /// <summary>
    /// 性能测试结果
    /// </summary>
    public class TimerResult
    {
        /// <summary>
        /// 方法名称
        /// </summary>
        public string ActionName { get; set; }
        /// <summary>
        /// 循环次数
        /// </summary>
        public int Iteration { get; set; }
        /// <summary>
        /// 总共执行时间
        /// </summary>
        public long ElapsedMillisecondsTotal { get; set; }
        /// <summary>
        /// 平均执行时间
        /// </summary>
        public long ElapsedMillisecondsOnce { get; set; }
        /// <summary>
        /// CPU总耗时
        /// </summary>
        public long CpuTimeTotal { get; set; }
        /// <summary>
        /// CPU平均耗时
        /// </summary>
        public long CpuTimeOnce { get; set; }
        /// <summary>
        /// GC内存垃圾回收次数
        /// </summary>
        public IDictionary<int, int> GC { get; set; }

        public TimerResult()
        {
            GC = new Dictionary<int, int>();
        }

        public override string ToString()
        {
            var result = new StringBuilder(string.Format("Action Name:\t\t{0}\r\nIteration:\t\t{1}\r\nTime Elapsed:\t\t{2}ms\r\nTime Elapsed(one time):\t{3}ms\r\nCPU time:\t\t{4}ns\r\nCPU time(one time):\t{5}ns\r\n"
                , ActionName
                , Iteration.ToString()
                , ElapsedMillisecondsTotal.ToString("N2")
                , ElapsedMillisecondsOnce.ToString("N2")
                , CpuTimeTotal.ToString("N0")
                , CpuTimeOnce.ToString("N0")));
            foreach (var item in GC.Keys)
            {
                result.AppendFormat("Gen{0}:\t\t\t{1}{2}", item.ToString(), GC[item].ToString(), Environment.NewLine);
            }
            return result.ToString();
        }
    }
}
