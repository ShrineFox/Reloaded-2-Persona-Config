using BF.File.Emulator.Interfaces;
using BGME.Framework.Interfaces;
using BMD.File.Emulator.Interfaces;
using CriFs.V2.Hook.Interfaces;
using Dolphin.ShadowTheHedgehog.RPC;
using Newtonsoft.Json;
using p5rpc.lib.interfaces;
using P5RPC.ColorStuff.Patches;
using P5RPC.ColorStuff.Patches.Common;
using P5RPC.ColorStuff.Utilities;
using PAK.Stream.Emulator.Interfaces;
using Reloaded.Hooks.Definitions.X64;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Memory.Sigscan;
using Reloaded.Memory.Sigscan.Definitions.Structs;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using SPD.File.Emulator.Interfaces;
using System;
using System.Diagnostics;
using System.Drawing;
using VinesauceModSettings.Configuration;
using VinesauceModSettings.Template;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace VinesauceModSettings
{
	/// <summary>
	/// Your mod logic goes here.
	/// </summary>
	public unsafe partial class Mod : ModBase // <= Do not Remove.
	{
        [Function(CallingConventions.Microsoft)]
        private delegate void* AddMeshToGlobalAttachmentList(
            void* param_1,
            void* param_2, string? name, byte param_3);

        [Function(CallingConventions.Microsoft)]
        private delegate EPL* LoadEPLFromFilename(string param_1);

        [Function(CallingConventions.Microsoft)]
        private delegate void PlayFromSystemACB(int param_1);

        [Function(CallingConventions.Microsoft)]
        private delegate void RestartEPLPlayback(void* param_1);
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

        private readonly AddMeshToGlobalAttachmentList? _addMeshToGlobalAttachmentList;
        private readonly bool* _pauseScreenVisible;
        private readonly LoadEPLFromFilename? _loadEplFromFilename;
        private readonly PlayFromSystemACB? _playFromSystemACB;
        private readonly RestartEPLPlayback? _restartEplPlayback;
        private readonly nuint* _titleResProcInstance;
        private readonly void** _rootGfdGlobalScene;

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



            // Define controllers and other variables, set warning messages
            var criFsController = _modLoader.GetController<ICriFsRedirectorApi>();
			if (criFsController == null || !criFsController.TryGetTarget(out var criFsApi))
			{
				_logger.WriteLine($"Something in CriFS shit its pants! Edited binary files will not load properly!", System.Drawing.Color.Red);
				return;
            }

            /*
            var RyoApi = _modLoader.GetController<IRyoApi>();
            if (RyoApi == null || !RyoApi.TryGetTarget(out var _RyoApi))
            {
                _logger.WriteLine($"Something in Ryo shit its pants! Randomized voice/sfx audio will not load properly!", System.Drawing.Color.Red);
                return;
            }
            */

            var PakEmulatorController = _modLoader.GetController<IPakEmulator>();
            if (PakEmulatorController == null || !PakEmulatorController.TryGetTarget(out var _PakEmulator))
            {
                _logger.WriteLine($"Something in PAK Emulator shit its pants! Files requiring bin merging will not load properly!", System.Drawing.Color.Red);
                return;
            }

            var SpdEmulatorController = _modLoader.GetController<ISpdEmulator>();
            if (SpdEmulatorController == null || !SpdEmulatorController.TryGetTarget(out var _SpdEmulator))
            {
                _logger.WriteLine($"Something in BF Emulator shit its pants! Files requiring sprite merging will not load properly!", System.Drawing.Color.Red);
                return;
            }

            var BfEmulatorController = _modLoader.GetController<IBfEmulator>();
            if (BfEmulatorController == null || !BfEmulatorController.TryGetTarget(out var _BfEmulator))
            {
                _logger.WriteLine($"Something in BF Emulator shit its pants! Files requiring script merging will not load properly!", System.Drawing.Color.Red);
                return;
            }

            var BmdEmulatorController = _modLoader.GetController<IBmdEmulator>();
            if (BmdEmulatorController == null || !BmdEmulatorController.TryGetTarget(out var _BmdEmulator))
            {
                _logger.WriteLine($"Something in BMD Emulator shit its pants! Files requiring text merging will not load properly!", System.Drawing.Color.Red);
                return;
            }

            var BGMEController = _modLoader.GetController<IBgmeApi>();
			if (BGMEController == null || !BGMEController.TryGetTarget(out var _BGME))
			{
				_logger.WriteLine($"Something in BGME shit its pants! Randomized battle music will not load properly!", System.Drawing.Color.Red);
				return;
            }

            // Main Files
            criFsApi.AddProbingPath($"{modDir}\\Mod Files\\Main\\_CPK");
            criFsApi.AddProbingPath($"{modDir}\\Mod Files\\Main\\_Models");
            criFsApi.AddProbingPath($"{modDir}\\Mod Files\\Main\\_Personas");
            criFsApi.AddProbingPath($"{modDir}\\Mod Files\\Main\\_Fields");
            criFsApi.AddProbingPath($"{modDir}\\Mod Files\\Main\\_Tables");
            criFsApi.AddProbingPath($"{modDir}\\Mod Files\\Main\\_UI");
            _BGME.AddFolder($"{modDir}\\Mod Files\\Main\\BGME");
            _BfEmulator.AddDirectory($"{modDir}\\Mod Files\\Main\\BF");
            _BmdEmulator.AddDirectory($"{modDir}\\Mod Files\\Main\\BMD");
            _PakEmulator.AddDirectory($"{modDir}\\Mod Files\\Main\\PAK");
            _SpdEmulator.AddDirectory($"{modDir}\\Mod Files\\Main\\SPD");

            //
            // Toggleable
            // 

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
                if (_configuration.UseCustomScripts)
                {
                    criFsApi.AddProbingPath($"{modDir}\\Mod Files\\Toggleable\\New Story\\Scripts\\CPK");
                    _BfEmulator.AddDirectory($"{modDir}\\Mod Files\\Toggleable\\New Story\\Scripts\\BF");
                    _BmdEmulator.AddDirectory($"{modDir}\\Mod Files\\Toggleable\\New Story\\Scripts\\BMD");
                }

                // Config Option: Emulate Unpacked Palace Textures
                if (_configuration.EmulateTextures)
                    _PakEmulator.AddDirectory($"{modDir}\\Mod Files\\Toggleable\\New Story\\Textures\\LooseBINs");

                // Random Corruption EPL Jumpscares CPK Dir
                criFsApi.AddProbingPath($"{modDir}\\Mod Files\\Toggleable\\New Story\\Effects");
            }

            // Config Option: Reskin Bosses

            if (_configuration.NewBosses)
            {
                criFsApi.AddProbingPath($"{modDir}\\Mod Files\\Toggleable\\Bosses\\CPK");
                _BfEmulator.AddDirectory($"{modDir}\\Mod Files\\Toggleable\\Bosses\\BF");
                _BmdEmulator.AddDirectory($"{modDir}\\Mod Files\\Toggleable\\Bosses\\BMD");
            }

            // Config Option: Reskin Dungeons

            if (_configuration.NewDungeons)
            {
                criFsApi.AddProbingPath($"{modDir}\\Mod Files\\Toggleable\\Dungeons\\CPK");
                _BfEmulator.AddDirectory($"{modDir}\\Mod Files\\Toggleable\\Dungeons\\BF");
                _BmdEmulator.AddDirectory($"{modDir}\\Mod Files\\Toggleable\\Dungeons\\BMD");
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

            // Config Option: Emulate ACB Files
            /*
            if (_configuration.UseEmulatedACBs)
                foreach (string yamlPath in Directory.GetFiles($"{modDir}\\Mod Files\\Main\\ACB", "*.yaml", SearchOption.AllDirectories))
                {
                    var audioConfig = ParseConfigFile(yamlPath, _logger);
                    if (string.IsNullOrEmpty(audioConfig.AcbName))
                        audioConfig.AcbName = Path.GetFileName(FindAcbFolder(yamlPath)).Replace(".ACB", "").Replace(".acb", "");
                    if (audioConfig.UsePlayerVolume == null)
                        audioConfig.UsePlayerVolume = true;
                    if (audioConfig.PlayerId == null)
                        audioConfig.PlayerId = -1;
                    if (audioConfig.IsEnabled == null)
                        audioConfig.IsEnabled = true;
                    if (audioConfig.CategoryIds == null)
                        audioConfig.CategoryIds = new int[] { 3 };
                    if (audioConfig.VolumeCategoryId == null)
                        audioConfig.VolumeCategoryId = 3;
                    if (audioConfig.PlaybackMode == null)
                        audioConfig.PlaybackMode = Ryo.Interfaces.Enums.PlaybackMode.Random;
                    audioConfig.Apply(audioConfig);
                    _RyoApi.AddAudioPath(Path.GetDirectoryName(yamlPath), audioConfig);

                    string jsonText = JsonConvert.SerializeObject(audioConfig, Newtonsoft.Json.Formatting.Indented);
                    _logger.WriteLine($"{Path.GetDirectoryName(yamlPath)}\r\n{jsonText}", System.Drawing.Color.AliceBlue);
                }
            */

            // Config Option: Use Silenced Base AWB files
            if (_configuration.UseSilencedBaseAWBs)
                criFsApi.AddProbingPath($"{modDir}\\Mod Files\\Toggleable\\Silence");

            // Config Option: Use Test Script on Startup
            if (_configuration.LoadTestScriptOnTitle)
                EditScript(modDir, true);
            else
                EditScript(modDir, false);

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


            /*
             * EPL STUFF - used to show randomized corruption effects
             */

            // courtesy of Wisteria and NotAMitten's FNAF mod: https://gamebanana.com/mods/629637
            // Original source code here: https://github.com/NM-20/P5RPC.Fnaf2

            if (_configuration.DisableEPLEffects)
            {
                _logger.PrintMessage("Randomized Screen Effects disabled by user config.", Color.Red);
                return;
            }

            Process process = Process.GetCurrentProcess();
            using Scanner scanner = new Scanner(process);
            PatternScanResult addResult = scanner.FindPattern("48 89 5C 24 08 48 89 6C " +
                "24 10 48 89 74 24 18 57 41 54 41 55 41 56 41 57 48 81 EC 20 01 00 00 45");

            if (!addResult.Found)
            {
                _logger.PrintMessage(
                  "Failed to locate `AddMeshToGlobalAttachmentList`! Randomized effects will not work...",
                  Color.Red);

                return;
            }

            _addMeshToGlobalAttachmentList = _hooks.CreateWrapper<AddMeshToGlobalAttachmentList>(
                  (process.MainModule!.BaseAddress + addResult.Offset), out _);

            PatternScanResult pauseResult = scanner.FindPattern("80 3D ?? ?? ?? ?? ?? 0F " +
              "84 A1 00 00 00");

            if (!pauseResult.Found)
            {
                _logger.PrintMessage(
                  "Failed to locate `PauseScreenVisible`! Randomized effects will not work...",
                  Color.Red);

                return;
            }

            _pauseScreenVisible = (bool*)(CmpInstructionToAbsoluteAddress(
              (byte*)(process.MainModule!.BaseAddress + pauseResult.Offset), 7));

            PatternScanResult loadResult = scanner.FindPattern("48 89 5C 24 08 57 48 83 " +
              "EC 60 48 8B D9 E8 ?? ?? ?? ?? 45 33 C9 45 33 C0 33 D2 48 8B CB 48 8B F8 81 48");

            if (!loadResult.Found)
            {
                _logger.PrintMessage(
                  "Failed to locate `LoadEPLFromFilename`! Randomized effects will not work...",
                  Color.Red);

                return;
            }

            _loadEplFromFilename = _hooks.CreateWrapper<LoadEPLFromFilename>(
              (process.MainModule!.BaseAddress + loadResult.Offset), out _);

            PatternScanResult playResult = scanner.FindPattern(
              "40 53 48 83 EC 30 48 8B 1D ?? ?? ?? ?? 48 85 DB 74 25 8B 53 08 41");

            if (!playResult.Found)
            {
                _logger.PrintMessage(
                  "Failed to locate `PlayFromSystemACB`! Randomized effects will not work...",
                  Color.Red);

                return;
            }

            _playFromSystemACB = _hooks.CreateWrapper<PlayFromSystemACB>(
              (process.MainModule!.BaseAddress + playResult.Offset), out _);

            PatternScanResult deleteResult = scanner.FindPattern("48 89 6C 24 18 57 48 " +
              "83 EC 20 48 8B 79 50");

            if (!deleteResult.Found)
            {
                _logger.PrintMessage(
                  "Failed to locate `RestartEPLPlayback`! Randomized effects will not work...",
                  Color.Red);

                return;
            }

            _restartEplPlayback = _hooks.CreateWrapper<RestartEPLPlayback>(
              (process.MainModule!.BaseAddress + deleteResult.Offset), out _);

            PatternScanResult titleResult = scanner.FindPattern("48 8B 0D ?? ?? ?? ?? 48 " +
              "85 C9 74 0F 48 8B 41 48 66 44 39 78 02 0F 82 6B 07 00 00");

            if (!titleResult.Found)
            {
                _logger.PrintMessage(
                  "Failed to locate `TitleResProcInstance`! Randomized effects will not work...",
                  Color.Red);

                return;
            }

            _titleResProcInstance = (nuint*)(MovInstructionToAbsoluteAddress(
              (byte*)(process.MainModule!.BaseAddress + titleResult.Offset), 7));

            PatternScanResult rootResult = scanner.FindPattern("48 8B 0D ?? ?? ?? ?? 48 " +
              "85 C9 74 05 E8 ?? ?? ?? ?? 48 8B 05");

            if (!rootResult.Found)
            {
                _logger.PrintMessage(
                  "Failed to locate `RootGfdGlobalScene`! Randomized effects will not work...",
                  Color.Red);

                return;
            }

            _rootGfdGlobalScene = (void**)(MovInstructionToAbsoluteAddress(
              (byte*)(process.MainModule!.BaseAddress + rootResult.Offset), 7));

            /* TODO: Look into a better way of executing code, potentially via a `Present`
               hook?
            */
            Task.Factory.StartNew(TaskMain, null);
        }

        /* https://reloaded-project.github.io/Reloaded-II/CheatSheet/SignatureScanning/ */
        private byte* CmpInstructionToAbsoluteAddress(byte* instructionAddress,
          int instructionLength)
        {
            byte* nextInstructionAddress = instructionAddress + instructionLength;
            var offset =
              (*((uint*)(instructionAddress + 2)));
            return (nextInstructionAddress + offset);
        }

        private byte* MovInstructionToAbsoluteAddress(byte* instructionAddress,
          int instructionLength)
        {
            byte* nextInstructionAddress = instructionAddress + instructionLength;
            var offset =
              (*((uint*)(instructionAddress + 3)));
            return (nextInstructionAddress + offset);
        }

        private bool HasEPLAnimationFinished(EPL* epl)
        {
            if ((epl->eplFlags & EPLFlags.LoopPlayback) is not 0)
                return false;

            if (epl->eplAnimation is null)
                return true;

            float duration;
            if (epl->eplAnimation->eplAnimStart is null)
                duration = epl->eplAnimation->Animation->Duration;
            else
                duration = epl->eplAnimation->eplAnimStart->Duration;

            return (epl->timeElapsed >= duration);
        }

        private void TaskMain(object? state)
        {
            /* We have a guarantee that our state is not null, so we can directly cast it
               to `IP5RLib`.
            */
            var parameters = (object[])(state!);

            Stopwatch watch = Stopwatch.StartNew();
            Random random = new();

            /* Wait until we pass the initial loading screen before executing the loop. */
            while ((*_titleResProcInstance) is 0)
                Thread.Yield();

            // Get list of EPLs
            var modDir = _modLoader.GetDirectoryForModId(_modConfig.ModId);
            string eplJson = $"{modDir}\\Mod Files\\Toggleable\\New Story\\Effects\\eplEffects.json";
            if (!File.Exists(eplJson))
            {
                _logger.PrintMessage($"[{_modConfig.ModId}] Could Not Load Randomized Effect List", Color.Red);
                return;
            }
            List<EPLEffect> eplEffects = JsonConvert.DeserializeObject<List<EPLEffect>>(File.ReadAllText(eplJson));

            // Get access to P5R flow functions
            var p5rLibController = _modLoader.GetController<IP5RLib>();
            if (p5rLibController == null || !p5rLibController.TryGetTarget(out var p5rLib))
            {
                _logger.WriteLine($"Something in P5R Library shit its pants! Corruption level won't be detected properly.", System.Drawing.Color.Red);
                return;
            }

            while (true)
            {
                if (watch.ElapsedMilliseconds < 1000)
                    continue;

                /* Don't `Restart` the timer and instead postpone the `Random.Next` call. */
                if ((*_pauseScreenVisible))
                    continue;

                int number = random.Next(1, _configuration.EPLEffectRate);
                //_logger.PrintMessage($"[{_modConfig.ModId}] Random Number: {number}", Color.Pink);

                int corruptLvlCountID = 3;
                int corruptionLevel = p5rLib.FlowCaller.GET_COUNT(3);

                // Filter list by eplEffects where corruptLevel is less than or equal to current level
                foreach (var eplEffect in eplEffects.Where(x => x.corruptLevel <= corruptionLevel).OrderBy(a => random.Next()))
                {
                    if (number <= eplEffect.chance)
                    {
                        /* Jumpscare implementation. */
                        _logger.PrintMessage($"[{_modConfig.ModId}] Playing Effect: {eplEffect.Name}", Color.Pink);
                        EPL* epl = _loadEplFromFilename!($"FIELD/EFFECT/BANK/FB{eplEffect.eplID.ToString("D3")}.EPL");
                        void* mesh = null;

                        if (mesh is null)
                            mesh = _addMeshToGlobalAttachmentList!((*_rootGfdGlobalScene), epl, null, 0);

                        _playFromSystemACB!(eplEffect.systemCueID);
                        _restartEplPlayback!(epl);

                        while (!HasEPLAnimationFinished(epl))
                            Thread.Yield();

                        /* Once one second has elapsed, we'll need to `Restart` to measure again. */
                        watch.Restart();
                        continue;
                    }
                }

                /* If the random number does not match the set beginning of the range, there
                   should not be a jumpscare. 
                */
                watch.Restart();
                continue;
            }
        }

        private void EditScript(string modDir, bool enable)
        {
            string titleScreenScript = Path.Combine(modDir, "Mod Files\\Toggleable\\New Story\\Scripts\\BF\\_CustomScripts\\DEBUG_TitleMenu.flow");
            if (File.Exists(titleScreenScript))
            {
                var lines = File.ReadAllLines(titleScreenScript);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains("!Test_OnTitleLoad"))
                    {
                        if (enable)
                            lines[i] = "\tif (!Test_OnTitleLoad(true))";
                        else
                            lines[i] = "\tif (!Test_OnTitleLoad(false))";
                    }
                }
                string newLines = string.Join('\n', lines);
                File.WriteAllText(titleScreenScript, newLines);
                _logger.WriteLine($"Edited DEBUG_TitleMenu.flow to {(enable ? "enable" : "disable")} Test_OnTitleLoad script.", System.Drawing.Color.Green);
            }
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

        /*
        private static AudioConfig? ParseConfigFile(string configFile, ILogger _logger)
        {
            try
            {
                _logger.WriteLine($"Loading audio config: {configFile}");
                var config = YamlSerializer.DeserializeFile<AudioConfig>(configFile);
                return config;
            }
            catch (Exception ex)
            {
                _logger.WriteLine($"Failed to parse audio config.\nFile: {configFile}");
                return null;
            }
        }

        public static string FindAcbFolder(string startPath)
        {
            var dir = new DirectoryInfo(startPath);

            if (!dir.Exists && File.Exists(startPath))
                dir = new FileInfo(startPath).Directory;

            while (dir != null)
            {
                foreach (var subDir in dir.GetDirectories())
                {
                    if (subDir.Name.EndsWith(".ACB", StringComparison.OrdinalIgnoreCase))
                    {
                        return subDir.FullName;
                    }
                }

                dir = dir.Parent;
            }

            return null;
        }
        */

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

    public class EPLEffect
    {
        public string Name { get; set; } = "";
        public int eplID { get; set; } = -1;
        public int systemCueID { get; set; } = -1;
        public int chance { get; set; } = -1;
        public int corruptLevel { get; set; } = -1;
    }

    public static class YamlSerializer
    {
        private static readonly IDeserializer deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();

        public static T DeserializeFile<T>(string filePath)
            => deserializer.Deserialize<T>(File.ReadAllText(filePath));
    }

}