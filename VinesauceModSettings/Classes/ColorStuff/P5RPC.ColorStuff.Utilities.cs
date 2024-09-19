using System;
using System.Runtime.CompilerServices;
using Reloaded.Mod.Interfaces;
using Reloaded.Memory.Sigscan.Definitions.Structs;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;

// original code by zarroboogs
namespace P5RPC.ColorStuff.Utilities
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

namespace P5RPC.ColorStuff.Utilities
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


namespace P5RPC.ColorStuff.Utilities
{
    internal static class OffsetHelper
    {
        public unsafe static IntPtr FromInst(IntPtr rip, int instLength)
        {
            // Old decompiled code that wouldn't compile
            // int num = *(rip + (IntPtr)instLength - (IntPtr)4);
            // return rip + (IntPtr)num + (IntPtr)instLength;

            // Cast IntPtr to a pointer to int
            int* ptr = (int*)((byte*)rip + instLength - 4); // Adjust pointer calculation
            int num = *ptr; // Dereference to get the value at the calculated pointer
            return rip + num + (IntPtr)instLength;
        }
    }
}

namespace P5RPC.ColorStuff.Utilities
{
    internal class SigScanHelper
    {
        public SigScanHelper(ILogger logger, IStartupScanner startupScanner)
        {
            this._logger = new Logger(logger, LogSeverity.Error);
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
                    if (!string.IsNullOrEmpty(name))
                    {
                        Logger logger = this._logger;
                        if (logger != null)
                        {
                            logger.Info(name + " found");
                        }
                    }
                    else
                    {
                        Logger logger2 = this._logger;
                        if (logger2 != null)
                        {
                            logger2.Info(pattern + " found");
                        }
                    }
                    action((uint)res.Offset);
                    return;
                }
                if (!string.IsNullOrEmpty(name))
                {
                    Logger logger3 = this._logger;
                    if (logger3 == null)
                    {
                        return;
                    }
                    logger3.Info(name + " not found");
                    return;
                }
                else
                {
                    Logger logger4 = this._logger;
                    if (logger4 == null)
                    {
                        return;
                    }
                    logger4.Info(pattern + " not found");
                    return;
                }
            });
        }

        private readonly IStartupScanner _startupScanner;

        private readonly Logger _logger;
    }
}
