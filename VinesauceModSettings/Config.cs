﻿using VinesauceModSettings.Template.Configuration;
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
        [Description("Skip the game's original story events and use new material.")]
        [DefaultValue(true)]
        public bool NewStory { get; set; } = true; // bool used in Mod.CS, not the folder name, but the bool name

        [Category("Chat Messages")]
        [DisplayName("Randomize Chat Messages")]
        [Description("Re-order navigator chat messages every time you launch the game.")]
        [DefaultValue(false)]
        public bool RandomizeChatMsgs { get; set; } = true; // bool used in Mod.CS, not the folder name, but the bool name

        [Category("Chat Messages")]
        [DisplayName("Mimic Twitch Chat")]
        [Description("Shows multiple navi chat messages at once in quick succession.")]
        [DefaultValue(false)]
        public bool MimicTwitchChat { get; set; } = true; // bool used in Mod.CS, not the folder name, but the bool name


        [Category("UI Artwork")]
        [DisplayName("Use NeonWillowLeaf's Scoot All Out Attack")]
        [Description("Enable this option to see NeonWillowLeaf's Scoot AoA in place of the default one by CheesyDraws.")]
        [DefaultValue(false)]
        public bool NeonWillowLeaf { get; set; } = false; // bool used in Mod.CS, not the folder name, but the bool name

        [Category("Mod Testing")]
        [DisplayName("Debug Mode")]
        [Description("Show title screen debug menu & override save button with Mod Menu.")]
        [DefaultValue(true)]
        public bool Debug { get; set; } = true; // bool used in Mod.CS, not the folder name, but the bool name
        
        [Category("Mod Testing")]
        [DisplayName("Skip Events")]
        [Description("Use dummy EVT files to quickly skip over cinematic cutscenes.")]
        [DefaultValue(true)]
        public bool EventSkip { get; set; } = true; // bool used in Mod.CS, not the folder name, but the bool name

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