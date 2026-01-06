using Dolphin.ShadowTheHedgehog.RPC;
using P5RPC.ColorStuff.Patches;
using P5RPC.ColorStuff.Patches.Common;
using P5RPC.ColorStuff.Utilities;
using Reloaded.Hooks.Definitions.X64;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using System.Diagnostics;
using BorangeModSettings.Configuration;
using BorangeModSettings.Template;

namespace BorangeModSettings
{
	/// <summary>
	/// Your mod logic goes here.
	/// </summary>
	public unsafe partial class Mod : ModBase // <= Do not Remove.
	{
        /// <summary>
        /// Provides access to the mod loader API.
        /// </summary>
        private readonly IModLoader _modLoader;
	
		/// <summary>
		/// Provides access to the Reloaded.Hooks API.
		/// </summary>
		/// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
		private readonly IReloadedHooks? _hooks;
	
		/// <summary>
		/// Provides access to the Reloaded logger.
		/// </summary>
		private readonly ILogger _logger;
	
		/// <summary>
		/// Entry point into the mod, instance that created this class.
		/// </summary>
		private readonly IMod _owner;
	
		/// <summary>
		/// Provides access to this mod's configuration.
		/// </summary>
		private Config _configuration;
	
		/// <summary>
		/// The configuration of the currently executing mod.
		/// </summary>
		private readonly IModConfig _modConfig;

        private BorangeRpc _borangeRpc;
        private readonly Process _currentProcess;

        public Mod(ModContext context)
		{
			_modLoader = context.ModLoader;
			_hooks = context.Hooks;
			_logger = context.Logger;
			_owner = context.Owner;
			_configuration = context.Configuration;
			_modConfig = context.ModConfig;
            _borangeRpc = new BorangeRpc(Process.GetCurrentProcess());

			var modDir = _modLoader.GetDirectoryForModId(_modConfig.ModId); // modDir variable for file emulation

            // ColorStuff by zarroboogs
            IStartupScanner startupScanner;
            _modLoader.GetController<IStartupScanner>().TryGetTarget(out startupScanner);
            SigScanHelper scanHelper = new SigScanHelper(_logger, startupScanner);
            _currentProcess = Process.GetCurrentProcess();
            IntPtr baseAddress = _currentProcess.MainModule.BaseAddress;
            PatchContext patchContext = new PatchContext
            {
                BaseAddress = baseAddress,
                Hooks = _hooks,
                Config = _configuration,
                ScanHelper = scanHelper
            };
            CmpBgColor.Activate(patchContext);
        }

        /* Mod loader actions. */
        public override void Suspend()
        {
            _borangeRpc.Suspend();
        }

        public override void Resume()
        {
            _borangeRpc.Resume();
        }

        public override void Unload()
        {
            Suspend();
            _borangeRpc.Dispose();
        }

        #region Standard Overrides
        public override void ConfigurationUpdated(Config configuration)
	    {
		    // Apply settings from configuration.
		    // ... your code here.
		    _configuration = configuration;
		    _logger.WriteLine($"[{_modConfig.ModId}] Applying Orange Menu Color...");
            CmpBgColor.UpdateConfig();
        }
	#endregion
	
		#region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	public Mod() { }
#pragma warning restore CS8618
	#endregion
	}

}