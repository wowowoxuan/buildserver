///////////////////////////////////////////////////////////////////////
///  HiResTimer.cs - High Resolution Timer - Uses Win32             ///
///  ver 1.0         Performance Counters and .Net Interop          ///
///                                                                 ///
///  Language:     Visual C#                                        ///
///  Platform:     Dell Dimension 8100, Windows Pro 2000, SP2       ///
///  Application:  CSE681 Example                                   ///
///  Author:       Jim Fawcett, CST 2-187, Syracuse Univ.           ///
///                (315) 443-3948, jfawcett@twcny.rr.com            ///
///////////////////////////////////////////////////////////////////////
/// Based on:                                                       ///
/// Windows Developer Magazine Column: Tech Tips, August 2002       ///
/// Author: Shawn Van Ness, shawnv@arithex.com                      ///
///////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices; // for DllImport attribute
using System.ComponentModel; // for Win32Exception class
using System.Threading; // for Thread.Sleep method

namespace HRTimer
{
    class HiResTimer
    {
        protected ulong a, b, f;

        public HiResTimer()
        {
            a = b = 0UL;
            if (QueryPerformanceFrequency(out f) == 0)
                throw new Win32Exception();
        }

        public ulong ElapsedTicks
        {
            get
            { return (b - a); }
        }

        public ulong ElapsedMicroseconds
        {
            get
            {
                ulong d = (b - a);
                if (d < 0x10c6f7a0b5edUL) // 2^64 / 1e6
                    return (d * 1000000UL) / f;
                else
                    return (d / f) * 1000000UL;
            }
        }

        public TimeSpan ElapsedTimeSpan
        {
            get
            {
                ulong t = 10UL * ElapsedMicroseconds;
                if ((t & 0x8000000000000000UL) == 0UL)
                    return new TimeSpan((long)t);
                else
                    return TimeSpan.MaxValue;
            }
        }

        public ulong Frequency
        {
            get
            { return f; }
        }

        public void Start()
        {
            Thread.Sleep(0);
            QueryPerformanceCounter(out a);
        }

        public ulong Stop()
        {
            QueryPerformanceCounter(out b);
            return ElapsedTicks;
        }

        // Here, C# makes calls into C language functions in Win32 API
        // through the magic of .Net Interop

        [DllImport("kernel32.dll", SetLastError = true)]
        protected static extern
           int QueryPerformanceFrequency(out ulong x);

        [DllImport("kernel32.dll")]
        protected static extern
           int QueryPerformanceCounter(out ulong x);
    }
}