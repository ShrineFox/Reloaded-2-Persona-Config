using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using P5RPC.ColorStuff.Configuration;
using P5RPC.ColorStuff.Patches;
using P5RPC.ColorStuff.Patches.Common;
using P5RPC.ColorStuff.Template;
using P5RPC.ColorStuff.Utilities;
using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using VinesauceModSettings.Configuration;
using VinesauceModSettings.Template;

// code by zarroboogs
namespace P5RPC.ColorStuff
{
    public class Mod : ModBase
    {
        public Mod(ModContext context)
        {
            this._modLoader = context.ModLoader;
            this._hooks = context.Hooks;
            this._logger = context.Logger;
            this._owner = context.Owner;
            this._configuration = context.Configuration;
            this._modConfig = context.ModConfig;
            Logger logger = new Logger(context.Logger, LogSeverity.Debug);
            IStartupScanner startupScanner;
            this._modLoader.GetController<IStartupScanner>().TryGetTarget(out startupScanner);
            SigScanHelper scanHelper = new SigScanHelper(logger, startupScanner);
            this._currentProcess = Process.GetCurrentProcess();
            IntPtr baseAddress = this._currentProcess.MainModule.BaseAddress;
            PatchContext patchContext = new PatchContext
            {
                BaseAddress = baseAddress,
                Config = this._configuration,
                Logger = logger,
                Hooks = this._hooks,
                ScanHelper = scanHelper
            };
            CmpBgColor.Activate(patchContext);
        }

        public override void ConfigurationUpdated(Config configuration)
        {
            this._configuration = configuration;
            this._logger.WriteLine("[" + this._modConfig.ModId + "] Config Updated: Applying");
            CmpBgColor.UpdateConfig(configuration);
        }

        public Mod()
        {
        }

        private readonly IModLoader _modLoader;

        private readonly IReloadedHooks _hooks;

        private readonly ILogger _logger;

        private readonly IMod _owner;

        private Config _configuration;

        private readonly IModConfig _modConfig;

        private readonly Process _currentProcess;
    }
}
