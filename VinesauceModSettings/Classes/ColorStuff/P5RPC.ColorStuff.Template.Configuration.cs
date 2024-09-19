using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Reloaded.Mod.Interfaces;
using System.Threading;

// original code by zarroboogs
namespace P5RPC.ColorStuff.Template.Configuration
{
    public class Configurable<TParentType> : IUpdatableConfigurable, IConfigurable where TParentType : Configurable<TParentType>, new()
    {        
        public static JsonSerializerOptions SerializerOptions {  get; } = new JsonSerializerOptions
        {
            Converters =
            {
                new JsonStringEnumConverter()
            },
            WriteIndented = true
        };

        [Browsable(false)]
        public event Action<IUpdatableConfigurable> ConfigurationUpdated;

        [JsonIgnore]
        [Browsable(false)]
        public string FilePath { get; private set; }

        [JsonIgnore]
        [Browsable(false)]
        public string ConfigName { get; private set; }

        [JsonIgnore]
        [Browsable(false)]
        private FileSystemWatcher ConfigWatcher { get; set; }

        
        private void Initialize(string filePath, string configName)
        {
            this.FilePath = filePath;
            this.ConfigName = configName;
            this.MakeConfigWatcher();
            this.Save = new Action(this.OnSave);
        }

        public void DisposeEvents()
        {
            FileSystemWatcher configWatcher = this.ConfigWatcher;
            if (configWatcher != null)
            {
                configWatcher.Dispose();
            }
            this.ConfigurationUpdated = null;
        }

        [JsonIgnore]
        [Browsable(false)]
        public Action Save { get; private set; }
        
        public static TParentType FromFile(string filePath, string configName)
        {
            return Configurable<TParentType>.ReadFrom(filePath, configName);
        }

        private void MakeConfigWatcher()
        {
            this.ConfigWatcher = new FileSystemWatcher(Path.GetDirectoryName(this.FilePath), Path.GetFileName(this.FilePath));
            this.ConfigWatcher.Changed += delegate (object sender, FileSystemEventArgs e)
            {
                this.OnConfigurationUpdated();
            };
            this.ConfigWatcher.EnableRaisingEvents = true;
        }

        private void OnConfigurationUpdated()
        {
            object readLock = Configurable<TParentType>._readLock;
            lock (readLock)
            {
                TParentType tparentType = Utilities.TryGetValue<TParentType>(() => Configurable<TParentType>.ReadFrom(this.FilePath, this.ConfigName), 250, 2, default(CancellationToken));
                tparentType.ConfigurationUpdated = this.ConfigurationUpdated;
                this.DisposeEvents();
                Action<IUpdatableConfigurable> configurationUpdated = tparentType.ConfigurationUpdated;
                if (configurationUpdated != null)
                {
                    configurationUpdated(tparentType);
                }
            }
        }

        private void OnSave()
        {
            TParentType value = (TParentType)((object)this);
            File.WriteAllText(this.FilePath, JsonSerializer.Serialize<TParentType>(value, Configurable<TParentType>.SerializerOptions));
        }

        private static TParentType ReadFrom(string filePath, string configName)
        {
            TParentType tparentType;
            if ((tparentType = (File.Exists(filePath) ? JsonSerializer.Deserialize<TParentType>(File.ReadAllBytes(filePath), Configurable<TParentType>.SerializerOptions) : Activator.CreateInstance<TParentType>())) == null)
            {
                tparentType = Activator.CreateInstance<TParentType>();
            }
            TParentType tparentType2 = tparentType;
            tparentType2.Initialize(filePath, configName);
            return tparentType2;
        }
        
        [Browsable(false)]
        private static object _readLock = new object();
    }

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
