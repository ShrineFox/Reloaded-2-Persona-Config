using VinesauceModSettings.Template.Configuration;
using Reloaded.Mod.Interfaces.Structs;
using System.ComponentModel;
using CriFs.V2.Hook;
using CriFs.V2.Hook.Interfaces;
using System.Reflection;

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

        [Category("Chat Messages")]
        [DisplayName("Randomize Chat Messages")]
        [Description("Re-order navigator chat messages every time you launch the game.")]
        [DefaultValue(false)]
        public bool RandomizeChatMsgs { get; set; } = true; 

        [Category("Chat Messages")]
        [DisplayName("Mimic Twitch Chat")]
        [Description("Shows multiple navi chat messages at once in quick succession.")]
        [DefaultValue(false)]
        public bool MimicTwitchChat { get; set; } = true; 
        
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

        [Category("Miscellaneous")]
        [DisplayName("Overwrite P5RCBT Config")]
        [Description("Disable this option if you want to use your own P5RCBT settings.")]
        [DefaultValue(true)]
        public bool OverwriteP5RCBTConfig { get; set; } = true; 

        [Category("Miscellaneous")]
        [DisplayName("Load Emulated Textures")]
        [Description("Use loose unpacked files for texture edits. Disable for quicker startup while testing.")]
        [DefaultValue(true)]
        public bool EmulateTextures { get; set; } = true;

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