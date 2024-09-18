using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using Reloaded.Mod.Interfaces;

namespace VinesauceModSettings
{
    public class Utilities
    {
        public static T TryGetValue<T>(Func<T> getValue, int timeout, int sleepTime, CancellationToken token = default(CancellationToken)) where T : new()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            bool flag = false;
            T result = Activator.CreateInstance<T>();
            while (stopwatch.ElapsedMilliseconds < (long)timeout)
            {
                if (token.IsCancellationRequested)
                {
                    return result;
                }
                try
                {
                    result = getValue();
                    flag = true;
                    break;
                }
                catch (Exception)
                {
                }
                Thread.Sleep(sleepTime);
            }
            if (!flag)
            {
                DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(24, 1);
                defaultInterpolatedStringHandler.AppendLiteral("Timeout limit ");
                defaultInterpolatedStringHandler.AppendFormatted<int>(timeout);
                defaultInterpolatedStringHandler.AppendLiteral(" exceeded.");
                throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
            }
            return result;
        }
    }

}
