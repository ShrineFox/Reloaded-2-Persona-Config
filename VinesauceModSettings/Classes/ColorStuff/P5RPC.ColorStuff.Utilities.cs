using System;
using System.Runtime.CompilerServices;
using Reloaded.Memory.Sigscan.Definitions.Structs;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;

namespace VinesauceModSettings
{
    public class Logger
    {
        public ILogger Log { get; private set; }

        public LogSeverity LogLevel { get; set; }

        public Logger(ILogger log, LogSeverity logLevel)
        {
            this.Log = log;
            this.LogLevel = logLevel;
        }

        public bool IsEnabled(LogSeverity severity)
        {
            return this.LogLevel <= severity;
        }

        public void Fatal(string message)
        {
            this.Log.WriteLineAsync(message, this.Log.ColorRed);
        }

        public void Error(string message)
        {
            if (this.IsEnabled(LogSeverity.Error))
            {
                this.Log.WriteLineAsync(message, this.Log.ColorRed);
            }
        }

        public void Warning(string message)
        {
            if (this.IsEnabled(LogSeverity.Warning))
            {
                this.Log.WriteLineAsync(message, this.Log.ColorYellow);
            }
        }

        public void Info(string message)
        {
            if (this.IsEnabled(LogSeverity.Information))
            {
                this.Log.WriteLineAsync(message, this.Log.ColorLightBlue);
            }
        }

        public void Debug(string message)
        {
            if (this.IsEnabled(LogSeverity.Debug))
            {
                this.Log.WriteLineAsync(message);
            }
        }
    }
}

namespace VinesauceModSettings
{
    public enum LogSeverity
    {
        Debug,
        Information,
        Warning,
        Error,
        Fatal
    }
}


namespace VinesauceModSettings
{
    internal static class OffsetHelper
    {
        public unsafe static IntPtr FromInst(IntPtr rip, int instLength)
        {
            //int num = *(rip + (IntPtr)instLength - (IntPtr)4);
            //return rip + (IntPtr)num + (IntPtr)instLength;

            // Cast IntPtr to a pointer to int
            int* ptr = (int*)((byte*)rip + instLength - 4); // Adjust pointer calculation
            int num = *ptr; // Dereference to get the value at the calculated pointer
            return rip + num + (IntPtr)instLength;
        }
    }
}

namespace VinesauceModSettings
{
    internal class SigScanHelper
    {
        public SigScanHelper(IStartupScanner startupScanner)
        {
            this._startupScanner = startupScanner;
        }

        public void FindPatternOffset(string pattern,  Action<uint> action, string name = null)
        {
            IStartupScanner startupScanner = this._startupScanner;
            if (startupScanner == null)
            {
                return;
            }
            startupScanner.AddMainModuleScan(pattern, delegate (PatternScanResult res)
            {
                if (res.Found)
                {
                    action((uint)res.Offset);
                    return;
                }
                if (!string.IsNullOrEmpty(name))
                {
                    return;
                }
                else
                {
                    return;
                }
            });
        }

        private readonly IStartupScanner _startupScanner;

    }
}
