﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FlyingFive.Data
{
    /// <summary>
    /// 动态生产有规律的ID
    /// </summary>
    public class SnowflakeId
    {
        private readonly long _machineId = 0L;//机器ID
        private readonly long _datacenterId = 0L;//数据ID
        private static long _sequence = 0L;//计数从零开始
        /// <summary>
        /// 唯一时间随机量
        /// </summary>
        private static readonly long _twepoch = 687888001020L;
        /// <summary>
        /// 机器码字节数
        /// </summary>
        private static readonly int _machineIdBits = 5;
        /// <summary>
        /// 数据字节数
        /// </summary>
        private static readonly int _dataCenterIdBits = 5;
        /// <summary>
        /// 计数器字节数，12个字节用来保存计数码  
        /// </summary>
        private static readonly int _sequenceBits = 12;
        /// <summary>
        /// 最大机器ID
        /// </summary>
        private static readonly long _maxMachineId = -1L ^ -1L << _machineIdBits;
        /// <summary>
        /// 最大数据ID
        /// </summary>
        private static readonly long _maxDataCenterId = -1L ^ (-1L << _dataCenterIdBits);
        /// <summary>
        /// 机器码数据左移位数，就是后面计数器占用的位数
        /// </summary>
        private static readonly int _machineIdShift = _sequenceBits;
        /// <summary>
        /// 数据标识id向左移17位(12+5) 
        /// </summary>
        private static readonly int _datacenterIdShift = _sequenceBits + _machineIdBits;
        /// <summary>
        /// 时间戳左移动位数就是机器码+计数器总字节数+数据字节数
        /// </summary>
        private static readonly int _timestampLeftShift = _sequenceBits + _machineIdBits + _dataCenterIdBits;
        /// <summary>
        /// 一微秒内可以产生计数，如果达到该值则等到下一微妙在进行生成
        /// </summary>
        private static long _sequenceMask = -1L ^ -1L << _sequenceBits;
        /// <summary>
        /// 最后时间戳
        /// </summary>
        private static long _lastTimestamp = -1L;

        private static object _syncObj = new object();

        private static SnowflakeId _instance = null;
        /// <summary>
        /// 默认实例
        /// </summary>
        public static SnowflakeId Default
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncObj)
                    {
                        if (_instance == null)
                        {
                            _instance = new SnowflakeId();
                        }
                    }
                }
                return _instance;
            }
        }

        public SnowflakeId() : this(0L, -1) { }

        public SnowflakeId(long machineId) : this(machineId, -1) { }

        public SnowflakeId(long machineId, long datacenterId)
        {
            if (machineId >= 0)
            {
                if (machineId > _maxMachineId)
                {
                    throw new ArgumentOutOfRangeException("机器码ID非法");
                }
                this._machineId = machineId;
            }
            if (datacenterId >= 0)
            {
                if (datacenterId > _maxDataCenterId)
                {
                    throw new ArgumentOutOfRangeException("数据中心ID非法");
                }
                this._datacenterId = datacenterId;
            }
        }

        private static readonly DateTime _startTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// 生成当前时间戳
        /// </summary>
        /// <returns>毫秒</returns>
        private static long GetTimestamp()
        {
            return (long)(DateTime.UtcNow - _startTime).TotalMilliseconds;
        }

        /// <summary>
        /// 获取下一微秒时间戳
        /// </summary>
        /// <param name="lastTimestamp"></param>
        /// <returns></returns>
        private static long GetNextTimestamp(long lastTimestamp)
        {
            long timestamp = GetTimestamp();
            int count = 0;
            while (timestamp <= lastTimestamp)//这里获取新的时间,可能会有错,这算法与comb一样对机器时间的要求很严格
            {
                count++;
                if (count > 10)
                {
                    throw new InvalidOperationException("机器的时间可能不对");
                }
                Thread.Sleep(1);
                timestamp = GetTimestamp();
            }
            return timestamp;
        }

        /// <summary>
        /// 解析雪花ID
        /// </summary>
        /// <returns></returns>
        public static string AnalyzeId(long id)
        {
            StringBuilder sb = new StringBuilder();

            var timestamp = id >> (int)_timestampLeftShift;
            var time = _startTime.AddMilliseconds(timestamp + _twepoch);
            sb.Append(time.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss:fff"));

            var datacenterId = (id ^ (timestamp << _timestampLeftShift)) >> _datacenterIdShift;
            sb.Append("_" + datacenterId);

            var workerId = (id ^ ((timestamp << _timestampLeftShift) | (datacenterId << _datacenterIdShift))) >> _machineIdShift;
            sb.Append("_" + workerId);

            var sequence = id & _sequenceMask;
            sb.Append("_" + sequence);

            return sb.ToString();
        }

        /// <summary>
        /// 获取长整形的ID
        /// </summary>
        /// <returns></returns>
        public long CreateUniqueId()
        {
            lock (_syncObj)
            {
                var timestamp = GetTimestamp();
                if (SnowflakeId._lastTimestamp == timestamp)
                { //同一微妙中生成ID
                    _sequence = (_sequence + 1) & _sequenceMask; //用&运算计算该微秒内产生的计数是否已经到达上限
                    if (_sequence == 0)
                    {
                        //一微妙内产生的ID计数已达上限，等待下一微妙
                        timestamp = GetNextTimestamp(SnowflakeId._lastTimestamp);
                    }
                }
                else
                {
                    //不同微秒生成ID
                    _sequence = 0L;
                }
                if (timestamp < _lastTimestamp)
                {
                    throw new Exception("时间戳比上一次生成ID时时间戳还小，故异常");
                }
                SnowflakeId._lastTimestamp = timestamp; //把当前时间戳保存为最后生成ID的时间戳
                var id = ((timestamp - _twepoch) << _timestampLeftShift)
                    | (_datacenterId << _datacenterIdShift)
                    | (_machineId << _machineIdShift)
                    | _sequence;
                if (id == 0)
                {
                    throw new InvalidOperationException("产生了不正确的数据。");
                }
                return id;
            }
        }
    }
}
