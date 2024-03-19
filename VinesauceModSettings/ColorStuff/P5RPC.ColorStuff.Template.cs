using System;
using System.Runtime.CompilerServices;
using P5RPC.ColorStuff.Configuration;
using Reloaded.Mod.Interfaces;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using VinesauceModSettings.Template.Configuration;

namespace P5RPC.ColorStuff.Template
{
    public class ModBase
    {
        public virtual bool CanSuspend()
        {
            return false;
        }

        public virtual bool CanUnload()
        {
            return false;
        }

        public virtual void Suspend()
        {
        }

        public virtual void Unload()
        {
        }

        public virtual void Disposing()
        {
        }

        public virtual void Resume()
        {
        }

        public virtual void ConfigurationUpdated(Config configuration)
        {
        }
    }
}

namespace P5RPC.ColorStuff.Template
{
    public class ModContext
    {
        public IModLoader ModLoader { get; set; }
        
        public IReloadedHooks Hooks {  get;  set; }

        public ILogger Logger { get; set; }

        public Config Configuration { get; set; }

        public IModConfig ModConfig { get; set; }

        public IMod Owner { get; set; }
    }
}

namespace P5RPC.ColorStuff.Template
{
    public class Startup : IMod, IModV1, IModV2
    {
        public void StartEx(IModLoaderV1 loaderApi, IModConfigV1 modConfig)
        {
            this._modLoader = (IModLoader)loaderApi;
            this._modConfig = (IModConfig)modConfig;
            this._logger = (ILogger)this._modLoader.GetLogger();
            WeakReference<IReloadedHooks> controller = this._modLoader.GetController<IReloadedHooks>();
            if (controller != null)
            {
                controller.TryGetTarget(out this._hooks);
            }
            Configurator configurator = new Configurator(this._modLoader.GetModConfigDirectory(this._modConfig.ModId));
            this._configuration = configurator.GetConfiguration<Config>(0);
            //this._configuration.ConfigurationUpdated += new Action<IUpdatableConfigurable>(this.OnConfigurationUpdated);
            this._mod = new Mod(new ModContext
            {
                Logger = this._logger,
                Hooks = this._hooks,
                ModLoader = this._modLoader,
                ModConfig = this._modConfig,
                Owner = this,
                Configuration = this._configuration
            });
        }

        private void OnConfigurationUpdated(IConfigurable obj)
        {
            this._configuration = (Config)obj;
            this._mod.ConfigurationUpdated(this._configuration);
        }

        public void Suspend()
        {
            this._mod.Suspend();
        }

        public void Resume()
        {
            this._mod.Resume();
        }

        public void Unload()
        {
            this._mod.Unload();
        }

        public bool CanUnload()
        {
            return this._mod.CanUnload();
        }

        public bool CanSuspend()
        {
            return this._mod.CanSuspend();
        }

        public Action Disposing
        {
            get
            {
                return delegate ()
                {
                    this._mod.Disposing();
                };
            }
        }

        private ILogger _logger;

        private IModLoader _modLoader;

        private Config _configuration;
        
        private IReloadedHooks _hooks;

        private IModConfig _modConfig;

        private ModBase _mod = new Mod();
    }
}
