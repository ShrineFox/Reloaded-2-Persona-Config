using VinesauceModSettings.Configuration;
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

            // Define controllers and other variables, set warning messages

            var criFsController = _modLoader.GetController<ICriFsRedirectorApi>();
			if (criFsController == null || !criFsController.TryGetTarget(out var criFsApi))
			{
				_logger.WriteLine($"Something in CriFS shit its pants! Normal files will not load properly!", System.Drawing.Color.Red);
				return;
            }

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




            // Set configuration options - obviously you don't need all of these, pick and choose what you need!

            // criFS
            if (_configuration.NeonWillowLeaf)
			{
				criFsApi.AddProbingPath("NeonWillowLeaf\\CPK"); // folder path. place a subfolder inside and then start your file path. for example: "(mod folder)\Test\(any name)\..."
			}
            if (_configuration.NewStory)
            {
                criFsApi.AddProbingPath("NewStory\\CPK"); // folder path. place a subfolder inside and then start your file path. for example: "(mod folder)\Test\(any name)\..."
            }
            criFsApi.AddProbingPath("Textures\\RepackedBINs"); // folder path. place a subfolder inside and then start your file path. for example: "(mod folder)\Test\(any name)\..."

            // PAK Emulator
            if (_configuration.EmulateTextures)
            {
                _PakEmulator.AddDirectory(Path.Combine(modDir, "Textures\\LooseBINs")); // folder path. immediately start your file path inside this folder. for example: "(mod folder)\Test\..."
            }

            // BF Emulator
            if (_configuration.NewStory)
            {
                _BfEmulator.AddDirectory(Path.Combine(modDir, "NewStory\\BF")); // folder path. immediately start your file path inside this folder. for example: "(mod folder)\Test\..."
            }
            // BMD Emulator
            if (_configuration.NewStory)
            {
                _BmdEmulator.AddDirectory(Path.Combine(modDir, "NewStory\\BMD")); // folder path. immediately start your file path inside this folder. for example: "(mod folder)\Test\..."
            }
			// Chat Messages
            criFsApi.AddProbingPath("Chat\\CPK"); // folder path. place a subfolder inside and then start your file path. for example: "(mod folder)\Test\(any name)\..."
            _BmdEmulator.AddDirectory(Path.Combine(modDir, "Chat\\BMD")); // folder path. immediately start your file path inside this folder. for example: "(mod folder)\Test\..."
            if (_configuration.RandomizeChatMsgs)
            {
                RewriteChatMessages($"{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}\\Chat\\chat.txt");
            }

			/*
            // BGME
            if (_configuration.NeonWillowLeaf)
            {
				_BGME.AddFolder(Path.Combine(modDir, "BGME")); // folder path. immediately start your file path inside this folder. for example: "(mod folder)\Test\..."
            }
			*/

			if (_configuration.OverwriteP5RCBTConfig)
				CopyP5RCBTConfig();
        }

        private void CopyP5RCBTConfig()
        {
			string cbtConfig = $"{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}\\config.toml";
			string destPath = $"{Path.GetDirectoryName(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location))}\\p5r.enhance.cbt\\config.toml";
		
			if (!File.Exists(destPath))
                _logger.WriteLine($"Failed to update P5RCBT config.toml, could not locate file:\n\"{Path.GetFullPath(destPath)}\"", System.Drawing.Color.Red);
			else
			{
				File.Copy(cbtConfig,destPath, true);
                _logger.WriteLine($"Updated P5RCBT config.toml using Vinesauce Mod settings.\n\"{Path.GetFullPath(destPath)}\"", System.Drawing.Color.Green);
            }

        }

        #region Standard Overrides
        public override void ConfigurationUpdated(Config configuration)
	{
		// Apply settings from configuration.
		// ... your code here.
		_configuration = configuration;
		_logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
	}
	#endregion
	
		#region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	public Mod() { }
#pragma warning restore CS8618
	#endregion
	}

}