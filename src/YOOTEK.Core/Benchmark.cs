using System;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;
using Castle.Core.Logging;
using Abp.Application.Services;
using Castle.Services.Logging.Log4netIntegration;

namespace Yootek
{

    class AtomicLong
    {
        long value;
         
        public AtomicLong()
        {
            this.value = 0;
        }
        public AtomicLong(long val)
        {
            this.value = val;
        }

        public long increment()
        {
            return Interlocked.Increment(ref value);
        }

        public long add(long val)
        {
            return Interlocked.Add(ref value, val);
        }

        public long incrementAndGet()
        {
            return increment();
        }

        public long set(long val)
        {
            return Interlocked.Exchange(ref value, val);
        }

        public long get()
        {
            return Interlocked.Read(ref value);
        }

        public string toString()
        {
            return value.ToString();
        }
    }
    public class Benchmark
    {
        public static Log4netFactory Factory = new Log4netFactory();
        public static ILogger Logger = Factory.Create("Benchmarks");

        public volatile string metricName;
        private static AtomicLong lineCounter = new AtomicLong(0);
        public int headcycle = 10;
        private AtomicLong total = new AtomicLong(0);
        private AtomicLong current = new AtomicLong(0);
        private AtomicLong totalTime = new AtomicLong(0);
        private AtomicLong totalCurrentTime = new AtomicLong(0);
        private ConcurrentDictionary<string, AtomicLong> threshold = new ConcurrentDictionary<String, AtomicLong>();
        public static long[] thresholdTime = new long[] { 100L, 200L, 300L, 500L, 1000L, 5000L };
        private long minProcessTime = long.MaxValue;
        private long maxProcessTime;
        private double avgProcessTime;
        private double avgCurrentProcessTime;
        private double minCurrentProcessTime = Double.MaxValue;
        private double maxCurrentProcessTime;
        private double currentTps;
        private long minCurSize = long.MaxValue;
        private long maxCurSize;
        private double avgCurSize;
        private double throughput;
        private AtomicLong totalBytes = new AtomicLong();
        private AtomicLong totalCurThroughput = new AtomicLong();
        private double avgTps;
        private double minTps = double.MaxValue;
        private double maxTps;
        private long lastTime;
        private long startTime;
        public long intervalstatistic = 5000;
        public long msgStatistic = 100000;
        private static int maxObjectLength = 0;
        private static int[] max = new int[19];
        private static int[] headLen = new int[19];
        private static string[] names = { "Metrics object ", " Total ", " avgTps ", " mintime ",
            " maxTime ", " avgTime ", " current ", " curTps ", " minTps ", " maxTps ", " minCurTime ", " maxCurTime ",
            " avgCurTime ", " minCurSize ", " maxcursize ", " avgCurSize ", " Throughput  ", " TotalMB ",
            " Threshold " };


        private String[] values = new String[19];
        private long lastTotalMsg = 0;
        private const string decimalFormat = "{0: #,##0.000}";
        private StringBuilder sb = new StringBuilder();
        private StringBuilder sbh = new StringBuilder();
        private static SpinLock sl = new SpinLock();
        //private static Benchmark instance;

        static void Init()
        {

            for (int i = 0; i < headLen.Length; i++)
            {
                headLen[i] = names[i].Length;
            }


        }

        static Benchmark()
        {
            config("200,300,400,500,600,700");
            Init();
        }

        //public static Benchmark getInstance()
        //{
        //    if (instance == null)
        //    {
        //        bool gotLock = false;
        //        try
        //        {
        //            sl.Enter(ref gotLock);
        //            if (instance == null)
        //            {
        //                instance = new Benchmark();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Logger.Fatal(ex.ToString(), ex);
        //        }
        //        finally
        //        {
        //            if (gotLock) sl.Exit();
        //        }
        //    }
        //    return instance;
        //}


        public string getRange(long processTime)
        {
            int index = -1;
            for (int i = 0; i < thresholdTime.Length; i++)
            {
                if (processTime <= thresholdTime[i])
                {
                    index = i;
                    break;
                }
            }
            string range = "";
            if (index == 0)
            {
                range = "0-" + thresholdTime[index];
            }
            else if (index > 0)
            {
                range = thresholdTime[index - 1] + "_" + thresholdTime[index];
            }
            else
            {
                range = thresholdTime[thresholdTime.Length - 1] + "- Max";
            }
            return range;
        }
        private static bool settedThreshold = false;
        public static void config(string config)
        {
            if (!settedThreshold)
            {
                bool gotLock = false;
                sl.Enter(ref gotLock);
                try
                {
                    if (!settedThreshold)
                    {
                        Object o = config;
                        if (o != null)
                        {
                            string r = (string)o;
                            string[] rs = r.Split(",");
                            ArrayList ll = new ArrayList();
                            foreach (string s in rs)
                            {
                                try
                                {
                                    long l = long.Parse(s.Trim());
                                    if (l > 0)
                                    {
                                        ll.Add(l);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Logger.Fatal(ex.ToString(), ex);
                                }
                            }
                            if (ll.Count > 2)
                            {
                                ll.Sort();
                                long[] th = new long[ll.Count];
                                th = (long[])ll.ToArray(typeof(long));
                                Logger.Fatal("Config threshold time: " + string.Join(" ", th));
                                Logger.Fatal("Config threshold time: " + string.Join(" ", ll.ToArray()));
                                thresholdTime = th;
                            }
                        }
                        settedThreshold = true;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Fatal(ex.ToString(), ex);
                }
                finally
                {
                    if (gotLock) sl.Exit();
                }
            }
        }

        public void increment()
        {

            Interlocked.Add(ref headcycle, 2);
        }

        public void statisticMetris(long incommingTime, long messageSize, string objectName)
        {
            if (String.IsNullOrEmpty(metricName))
            {
                metricName = objectName;
            }
            if (startTime == 0)
            {
                startTime = TimeUtils.GetMilliseconds() - 1;
                lastTime = startTime;
            }
            total.increment();
            current.increment();
            long currentTime = TimeUtils.GetMilliseconds();
            long processTime = (TimeUtils.GetNanoseconds() - incommingTime) / 1000;

            //Logger.Fatal("Process time " + processTime);
            totalTime.add(processTime);
            totalCurrentTime.add(processTime);
            if (processTime < minProcessTime)
            {
                minProcessTime = processTime;
            }

            string rangeKey = getRange(processTime);
            AtomicLong al;
            threshold.TryGetValue(rangeKey, out al);

            if (al == null)
            {
                bool gotLock = false;
                sl.Enter(ref gotLock);
                try
                {
                    threshold.TryGetValue(rangeKey, out al);
                    if (al == null)
                    {
                        al = new AtomicLong(0);
                        threshold[rangeKey] = al;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Fatal(ex.ToString(), ex);
                }
                finally
                {
                    if (gotLock) sl.Exit();
                }
            }

            long value = al.incrementAndGet();
            if (value >= long.MaxValue - 10)
            {
                al.set(0);
            }
            if (maxProcessTime < processTime)
            {
                maxProcessTime = processTime;
            }
            if (processTime < minCurrentProcessTime)
            {
                minCurrentProcessTime = processTime;
            }
            if (maxCurrentProcessTime < processTime)
            {
                maxCurrentProcessTime = processTime;
            }

            if (maxCurSize < messageSize)
            {
                maxCurSize = messageSize;
            }
            if (minCurSize > messageSize)
            {
                minCurSize = messageSize;
            }
            totalCurThroughput.add(messageSize);
            totalBytes.add(messageSize);
            totalCurThroughput.add(messageSize);
            totalBytes.add(messageSize);

            if (currentTime - lastTime > intervalstatistic || total.get() - lastTotalMsg >= msgStatistic)
            {
                bool gotLock = false;
                sl.Enter(ref gotLock);
                try
                {
                    if (currentTime - lastTime > intervalstatistic || total.get() - lastTotalMsg >= msgStatistic)
                    {
                        currentTps = current.get() * 1000.0 / (currentTime - lastTime);
                        avgTps = total.get() * 1000.0 / (currentTime - startTime);
                        avgProcessTime = (double)totalTime.get() / total.get();
                        avgCurrentProcessTime = (double)totalCurrentTime.get() / current.get();
                        if (minTps > currentTps)
                        {
                            minTps = currentTps;
                        }
                        if (maxTps < currentTps)
                        {
                            maxTps = currentTps;
                        }
                        avgCurSize = totalCurThroughput.get() * 1.0 / current.get();
                        throughput = totalCurThroughput.get() * 1.0 / (currentTime - lastTime);
                        long lines = lineCounter.get(); lineCounter.increment();
                        sb.Clear();
                        sbh.Clear();
                        bool f = true;
                        foreach (KeyValuePair<string, AtomicLong> e in threshold)
                        {
                            if (!f)
                            {
                                sbh.Append(" |");
                            }
                            f = false;
                            sbh.Append(e.Key).Append("=").Append(e.Value.get());
                        }
                        int idx = 0;
                        values[idx++] = objectName;
                        values[idx++] = total.toString();
                        values[idx++] = String.Format(decimalFormat, avgTps);
                        values[idx++] = minProcessTime.ToString();
                        values[idx++] = maxProcessTime.ToString();
                        values[idx++] = String.Format(decimalFormat, avgProcessTime);
                        values[idx++] = current.toString();
                        values[idx++] = String.Format(decimalFormat, currentTps);
                        values[idx++] = String.Format(decimalFormat, minTps);
                        values[idx++] = String.Format(decimalFormat, maxTps);
                        values[idx++] = minCurrentProcessTime.ToString();
                        values[idx++] = maxCurrentProcessTime.ToString();
                        values[idx++] = String.Format(decimalFormat, avgCurrentProcessTime);
                        values[idx++] = minCurSize.ToString();
                        values[idx++] = maxCurSize.ToString();
                        values[idx++] = String.Format(decimalFormat, avgCurSize);
                        values[idx++] = String.Format(decimalFormat, throughput);
                        values[idx++] = String.Format(decimalFormat, totalBytes.get() * 1.0 / 1024 / 1024);
                        values[idx++] = sbh.ToString();
                        //values[idx++] = String.Format("{0:###,##0.000}", 122223333.345678);

                        String format;
                        if (maxObjectLength < objectName.Length)
                        {
                            maxObjectLength = objectName.Length;
                        }
                        if (lines % headcycle == 0)
                        {
                            max[0] = maxObjectLength + 2;
                            for (int i = 0; i < values.Length; i++)
                            {
                                headLen[i] = Math.Max(Math.Max(names[i].Length, values[i].Length + 2), max[i]);
                                format = String.Format("{0," + headLen[i] + "}|", names[i]);
                                sb.Append(format);
                            }
                            Logger.Fatal(sb.ToString());
                            for (int i = 0; i < max.Length; i++)
                            {
                                max[i] = 0;
                            }
                        }
                        sb.Clear();
                        for (int i = 1; i < max.Length; i++)
                        {
                            if (max[i] < values[i].Length + 2)
                            {
                                max[i] = values[i].Length + 2;
                            }
                        }

                        for (int i = 0; i < values.Length; i++)
                        {
                            format = String.Format("{0," + headLen[i] + "}", values[i]);
                            sb.Append(format).Append("|");
                        }
                        Logger.Fatal(sb.ToString());
                        sb.Clear();
                        sbh.Clear();
                        current.set(0);

                        totalCurrentTime.set(0);
                        lastTime = TimeUtils.GetMilliseconds();
                        maxCurrentProcessTime = 0;
                        minCurrentProcessTime = double.MaxValue;
                        lastTotalMsg = total.get();
                        maxCurSize = 0;
                        minCurSize = long.MaxValue;
                        totalCurThroughput.set(0);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Fatal(ex.ToString(), ex);
                }
                finally
                {
                    if (gotLock) sl.Exit();
                }
            }

        }






    }


    public static class TimeUtils
    {
        public static long GetNanoseconds()
        {
            double timestamp = Stopwatch.GetTimestamp();
            double nanoseconds = 1_000_000_000.0 * timestamp / Stopwatch.Frequency;

            return (long)nanoseconds;
        }

        public static long GetMilliseconds()
        {
            double timestamp = Stopwatch.GetTimestamp();
            double milliseconds = 1000.0 * timestamp / Stopwatch.Frequency;

            return (long)milliseconds;
        }
    }



    //class TestBenchmark
    //{


    //    Benchmark mb = new Benchmark();
    //    Stopwatch stopWatch = new Stopwatch();
    //    static SpinWait spin = new SpinWait();
    //    static void udelay(long us)
    //    {
    //        var sw = System.Diagnostics.Stopwatch.StartNew();
    //        long v = (us * System.Diagnostics.Stopwatch.Frequency) / 1000000;
    //        while (sw.ElapsedTicks < v)
    //        {
    //            // Thread.Yield();
    //            // spin.SpinOnce();
    //        }
    //    }

    //    private const long TicksPerSecond = TimeSpan.TicksPerSecond;
    //    private const long TicksPerMillisecond = TimeSpan.TicksPerMillisecond;
    //    private const long TicksPerMicrosecond = TimeSpan.TicksPerMillisecond / 1000;

    //    /// <summary>A scale that normalizes the hardware ticks to <see cref="TimeSpan" /> ticks which are 100ns in length.</summary>
    //    private static readonly double s_tickFrequency = (double)TicksPerSecond / Stopwatch.Frequency;

    //    public static void DelayMicroseconds(int microseconds, bool allowThreadYield)
    //    {
    //        var time = TimeSpan.FromTicks(microseconds * TicksPerMicrosecond);
    //        Delay(time, allowThreadYield);
    //    }
    //    public static void Delay(TimeSpan time, bool allowThreadYield)
    //    {
    //        long start = Stopwatch.GetTimestamp();
    //        long delta = (long)(time.Ticks / s_tickFrequency);
    //        long target = start + delta;

    //        if (!allowThreadYield)
    //        {
    //            do
    //            {
    //                Thread.SpinWait(1);
    //            }
    //            while (Stopwatch.GetTimestamp() < target);
    //        }
    //        else
    //        {
    //            SpinWait spinWait = new SpinWait();
    //            do
    //            {
    //                spinWait.SpinOnce();
    //            }
    //            while (Stopwatch.GetTimestamp() < target);
    //        }
    //    }
    //    public void testLog()
    //    {

    //        while (true)
    //        {

    //            long t1 = TimeUtils.GetNanoseconds();
    //            //Thuc hien tac vu nao do
    //            DelayMicroseconds(100, !true);
    //            mb.statisticMetris(t1, 0, "LogName");
    //        }
    //    }
    //}
}