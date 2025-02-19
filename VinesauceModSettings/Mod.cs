using VinesauceModSettings.Template;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using CriFs.V2.Hook;
using CriFs.V2.Hook.Interfaces;
using PAK.Stream.Emulator;
using PAK.Stream.Emulator.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using BGME.Framework;
using BGME.Framework.Interfaces;
using BF.File.Emulator;
using BF.File.Emulator.Interfaces;
using System.Diagnostics;
using BMD.File.Emulator.Interfaces;
using SPD.File.Emulator.Interfaces;
using Dolphin.ShadowTheHedgehog.RPC;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using VinesauceModSettings.Configuration;
using P5RPC.ColorStuff.Patches;
using P5RPC.ColorStuff.Utilities;
using P5RPC.ColorStuff.Patches.Common;

namespace VinesauceModSettings
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

        private VinesauceRpc _vinesauceRpc;
        private readonly Process _currentProcess;

        public Mod(ModContext context)
		{
			_modLoader = context.ModLoader;
			_hooks = context.Hooks;
			_logger = context.Logger;
			_owner = context.Owner;
			_configuration = context.Configuration;
			_modConfig = context.ModConfig;
            _vinesauceRpc = new VinesauceRpc(Process.GetCurrentProcess());

			var modDir = _modLoader.GetDirectoryForModId(_modConfig.ModId); // modDir variable for file emulation

            // For more information about this template, please see
            // https://reloaded-project.github.io/Reloaded-II/ModTemplate/

            // If you want to implement e.g. unload support in your mod,
            // and some other neat features, override the methods in ModBase.

            // TODO: Implement some mod logic

            // Define controllers and other variables, set warning messages
            var criFsController = _modLoader.GetController<ICriFsRedirectorApi>();
			if (criFsController == null || !criFsController.TryGetTarget(out var criFsApi))
			{
				_logger.WriteLine($"Something in CriFS shit its pants! Normal files will not load properly!", System.Drawing.Color.Red);
				return;
            }

			/*
            var AwbEmulatorController = _modLoader.GetController<IAwbEmulator>();
            if (AwbEmulatorController == null || !AwbEmulatorController.TryGetTarget(out var _AwbEmulator))
            {
                _logger.WriteLine($"Something in AWB Emulator shit its pants! Normal files will not load properly!", System.Drawing.Color.Red);
                return;
            }
			*/

            var PakEmulatorController = _modLoader.GetController<IPakEmulator>();
            if (PakEmulatorController == null || !PakEmulatorController.TryGetTarget(out var _PakEmulator))
            {
                _logger.WriteLine($"Something in PAK Emulator shit its pants! Files requiring bin merging will not load properly!", System.Drawing.Color.Red);
                return;
            }

            var BfEmulatorController = _modLoader.GetController<IBfEmulator>();
            if (BfEmulatorController == null || !BfEmulatorController.TryGetTarget(out var _BfEmulator))
            {
                _logger.WriteLine($"Something in BF Emulator shit its pants! Files requiring bf merging will not load properly!", System.Drawing.Color.Red);
                return;
            }

            var SpdEmulatorController = _modLoader.GetController<ISpdEmulator>();
            if (SpdEmulatorController == null || !SpdEmulatorController.TryGetTarget(out var _SpdEmulator))
            {
                _logger.WriteLine($"Something in BF Emulator shit its pants! Files requiring bf merging will not load properly!", System.Drawing.Color.Red);
                return;
            }

            var BmdEmulatorController = _modLoader.GetController<IBmdEmulator>();
            if (BmdEmulatorController == null || !BmdEmulatorController.TryGetTarget(out var _BmdEmulator))
            {
                _logger.WriteLine($"Something in BMD Emulator shit its pants! Files requiring bmd merging will not load properly!", System.Drawing.Color.Red);
                return;
            }

            var BGMEController = _modLoader.GetController<IBgmeApi>();
			if (BGMEController == null || !BGMEController.TryGetTarget(out var _BGME))
			{
				_logger.WriteLine($"Something in BGME shit its pants! Files requiring bin merging will not load properly!", System.Drawing.Color.Red);
				return;
            }

            // Main Files
            criFsApi.AddProbingPath($"{modDir}\\Mod Files\\Main\\_CPK");
            _BGME.AddFolder($"{modDir}\\Mod Files\\Main\\BGME");
            _BfEmulator.AddDirectory($"{modDir}\\Mod Files\\Main\\BF");
            _BmdEmulator.AddDirectory($"{modDir}\\Mod Files\\Main\\BMD");
            _PakEmulator.AddDirectory($"{modDir}\\Mod Files\\Main\\PAK");
            _SpdEmulator.AddDirectory($"{modDir}\\Mod Files\\Main\\SPD");

            // Toggleable

            // Config Option: Alt Scoot AoA by NeonWillowLeaf
            if (_configuration.NeonWillowLeaf)
                criFsApi.AddProbingPath($"{modDir}\\Mod Files\\Toggleable\\Alt Scoot AoA");

            // Config Option: Randomize Chat Messages
            criFsApi.AddProbingPath($"{modDir}\\Mod Files\\Toggleable\\Chat Navi\\CPK");
            _BmdEmulator.AddDirectory($"{modDir}\\Mod Files\\Toggleable\\Chat Navi\\BMD");
            if (_configuration.RandomizeChatMsgs || 
                !Directory.Exists($"{modDir}\\Mod Files\\Toggleable\\Chat Navi\\BMD"))
            {
                RewriteChatMessages($"{modDir}\\Mod Files\\Toggleable\\Chat Navi\\chat.txt");
            }
            UpdateChatPingSFX(modDir, _configuration.UsePingSFX);

            // Config Option: New Story

            if (_configuration.NewStory)
            {
                criFsApi.AddProbingPath($"{modDir}\\Mod Files\\Toggleable\\New Story\\CPK");
                if (_configuration.UseRepackedTextures)
                {
                    criFsApi.AddProbingPath($"{modDir}\\Mod Files\\Toggleable\\New Story\\Textures\\RepackedBINs");
                }
                criFsApi.AddProbingPath($"{modDir}\\Mod Files\\Toggleable\\New Story\\Scripts\\CPK");
                _BfEmulator.AddDirectory($"{modDir}\\Mod Files\\Toggleable\\New Story\\Scripts\\BF");
                _BmdEmulator.AddDirectory($"{modDir}\\Mod Files\\Toggleable\\New Story\\Scripts\\BMD");

                // Config Option: Emulate Unpacked Palace Textures
                if (_configuration.EmulateTextures)
                    _PakEmulator.AddDirectory($"{modDir}\\Mod Files\\Toggleable\\New Story\\Textures\\LooseBINs");
            }

            // Config Option: Overwrite P5R Custom Bonus Tweaks config.toml
            try
            {
                var cbtDir = _modLoader.GetDirectoryForModId("p5r.enhance.cbt");
                if (_configuration.OverwriteP5RCBTConfig)
                    CopyP5RCBTConfig(modDir, cbtDir);
            }
            catch
            {
                _logger.WriteLine($"Failed to update P5RCBT config.toml, could not locate Persona 5 Royal Custom Bonus Tweaks mod.", System.Drawing.Color.Red);
            }

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
            _vinesauceRpc.Suspend();
        }

        public override void Resume()
        {
            _vinesauceRpc.Resume();
        }

        public override void Unload()
        {
            Suspend();
            _vinesauceRpc.Dispose();
        }

        private void CopyP5RCBTConfig(string modDir, string cbtDir)
        {
            string cbtTomlConfig = Path.Combine(modDir, "config.toml");
            string destTomlPath = Path.Combine(cbtDir, "config.toml");

            string cbtReloadConfig = Path.Combine(modDir, "Config.json");
            string reloadConfigDir = cbtDir.Replace("\\Mods\\", "\\User\\Mods\\");
            string destReloadConfigPath = Path.Combine(reloadConfigDir, "Config.json");

            _logger.WriteLine($"cbtTomlConfig: {cbtTomlConfig}\n" +
                $"destTomlPath: {destTomlPath}\n" +
                $"cbtReloadConfig: {cbtReloadConfig}\n" +
                $"reloadConfigPath: {destReloadConfigPath}\n" +
                $"destReloadConfigPath: {cbtTomlConfig}\n", System.Drawing.Color.Yellow);

            if (!File.Exists(cbtReloadConfig))
                _logger.WriteLine($"Couldn't find cbtReloadConfig path: {cbtReloadConfig}", System.Drawing.Color.Red);
            if (!File.Exists(destReloadConfigPath))
                _logger.WriteLine($"Couldn't find destReloadConfigPath path: {destReloadConfigPath}", System.Drawing.Color.Red);

            Directory.CreateDirectory(reloadConfigDir);
            File.Copy(cbtTomlConfig, destTomlPath, true);
            File.Copy(cbtReloadConfig, destReloadConfigPath, true);

            _logger.WriteLine($"Updated P5RCBT config.toml and Config.json using Vinesauce Mod settings.", System.Drawing.Color.Green);
        }

        #region Standard Overrides
        public override void ConfigurationUpdated(Config configuration)
	    {
		    // Apply settings from configuration.
		    // ... your code here.
		    _configuration = configuration;
		    _logger.WriteLine($"[{_modConfig.ModId}] Applying Green Menu Color (only works properly with P5R v1.0.3)...");
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