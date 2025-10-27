using VinesauceModSettings.Template.Configuration;
using System.ComponentModel;

namespace VinesauceModSettings.Configuration
{
	public class Config : Configurable<Config>
	{
        /*
            User Properties:
                - Please put all of your configurable properties here.

            By default, configuration saves as "Config.json" in mod user config folder.    
            Need more config files/classes? See Configuration.cs

            Available Attributes:
            - Category
            - DisplayName
            - Description
            - DefaultValue

            // Technically Supported but not Useful
            - Browsable
            - Localizable

            The `DefaultValue` attribute is used as part of the `Reset` button in Reloaded-Launcher.
        */

        [Category("New Story Mode")]
        [DisplayName("Replace Story Events")]
        [Description("Skip the game's original story events in favor of new material.")]
        [DefaultValue(true)]
        public bool NewStory { get; set; } = true;

        [Category("New Story Mode")]
        [DisplayName("Reskin Bosses")]
        [Description("Use the Vinesauce-themed boss fights. Disable for vanilla.")]
        [DefaultValue(true)]
        public bool NewBosses { get; set; } = true;

        [Category("New Story Mode")]
        [DisplayName("Reskin Dungeons")]
        [Description("Use the Vinesauce-themed Palaces. Disable for vanilla.")]
        [DefaultValue(true)]
        public bool NewDungeons { get; set; } = true;

        [Category("Chat Messages")]
        [DisplayName("Randomize Chat Messages")]
        [Description("Re-order navigator chat messages every time you launch the game.")]
        [DefaultValue(true)]
        public bool RandomizeChatMsgs { get; set; } = true; 

        /*
        [Category("Chat Messages")]
        [DisplayName("Mimic Twitch Chat")]
        [Description("Shows multiple navi chat messages at once in quick succession.")]
        [DefaultValue(false)]
        public bool MimicTwitchChat { get; set; } = false; 
        */
        
        [Category("Chat Messages")]
        [DisplayName("Use Ping SFX")]
        [Description("Hear a slightly perceptible messenger ping when chat navi messages appear.")]
        [DefaultValue(false)]
        public bool UsePingSFX { get; set; } = true; 


        [Category("UI Artwork")]
        [DisplayName("Use NeonWillowLeaf's Scoot All Out Attack")]
        [Description("Enable this option to see NeonWillowLeaf's Scoot AoA in place of the default one by CheesyDraws.")]
        [DefaultValue(false)]
        public bool NeonWillowLeaf { get; set; } = false; 

        [Category("Textures")]
        [DisplayName("Load Emulated Textures")]
        [Description("Use loose unpacked files for texture edits. Disable for quicker startup while testing.")]
        [DefaultValue(true)]
        public bool EmulateTextures { get; set; } = true;

        [Category("Textures")]
        [DisplayName("Load Repacked Textures")]
        [Description("Use repacked files for texture edits. Disable to ensure only unpacked textures are loaded.")]
        [DefaultValue(true)]
        public bool UseRepackedTextures { get; set; } = true;

        [Category("Sound")]
        [DisplayName("Use Silenced Base AWBs")]
        [Description("Silence voice clips by default. Emulated ACB/AWB files override this. Disable for faster startup.")]
        [DefaultValue(true)]
        public bool UseSilencedBaseAWBs { get; set; } = true;

        [Category("Debug")]
        [DisplayName("Overwrite P5RCBT Config")]
        [Description("Disable this option if you want to use your own P5RCBT settings.")]
        [DefaultValue(true)]
        public bool OverwriteP5RCBTConfig { get; set; } = true;

        [Category("Debug")]
        [DisplayName("Use Custom Scripts")]
        [Description("Disable this option for faster startup when not testing script edits.")]
        [DefaultValue(true)]
        public bool UseCustomScripts { get; set; } = true;

        [Category("Debug")]
        [DisplayName("Use Test Script on Startup")]
        [Description("Loads edits from _1A_Testing.flow on Title Screen.")]
        [DefaultValue(false)]
        public bool LoadTestScriptOnTitle { get; set; } = false;

        [Category("Debug")]
        [DisplayName("Disable Randomized Screen Effects")]
        [Description("Use this to prevent jumpscares from happening.")]
        [DefaultValue(false)]
        public bool DisableEPLEffects { get; set; } = false;

        [Category("Debug")]
        [DisplayName("Randomized Screen Effect Probability")]
        [Description("The higher the number, the less often jumpscares will happen.")]
        [DefaultValue(100000)]
        public int EPLEffectRate { get; set; } = 100000;
    }

    /// <summary>
    /// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
    /// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
    /// </summary>
	public class ConfiguratorMixin : ConfiguratorMixinBase
	{
		// 
	}
}