using p5rpc.menucolor.Template;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using System.Diagnostics;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using p5rpc.menucolor.Configuration;
using P5RPC.ColorStuff.Patches;
using P5RPC.ColorStuff.Utilities;
using P5RPC.ColorStuff.Patches.Common;

namespace p5rpc.menucolor
{
	/// <summary>
	/// Your mod logic goes here.
	/// </summary>
	public partial class Mod : ModBase // <= Do not Remove.
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

        private readonly Process _currentProcess;

        public Mod(ModContext context)
		{
			_modLoader = context.ModLoader;
			_hooks = context.Hooks;
			_logger = context.Logger;
			_owner = context.Owner;
			_configuration = context.Configuration;
			_modConfig = context.ModConfig;

			var modDir = _modLoader.GetDirectoryForModId(_modConfig.ModId); // modDir variable for file emulation

            // For more information about this template, please see
            // https://reloaded-project.github.io/Reloaded-II/ModTemplate/

            // If you want to implement e.g. unload support in your mod,
            // and some other neat features, override the methods in ModBase.

            // TODO: Implement some mod logic


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

        #region Standard Overrides
        public override void ConfigurationUpdated(Config configuration)
	    {
		    // Apply settings from configuration.
		    // ... your code here.
		    _configuration = configuration;
		    _logger.WriteLine($"[{_modConfig.ModId}] Applying Menu Color...");
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