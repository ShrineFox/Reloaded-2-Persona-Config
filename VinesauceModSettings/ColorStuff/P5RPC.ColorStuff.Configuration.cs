using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using P5RPC.ColorStuff.Template.Configuration;
using VinesauceModSettings.Template.Configuration;

namespace P5RPC.ColorStuff.Configuration
{
    public class Config : Configurable<Config>
    {
        [Category("Camp BG")]
        [DisplayName("Mode")]
        [Description]
        [DefaultValue(Config.Mode.Off)]
        public Config.Mode CmpBgColorMode { get; set; }

        [Category("Camp BG")]
        [DisplayName("Color")]
        [Description]
        [DefaultValue("#FF0023")]
        public string CmpBgColor { get; set; } = "#FF0023";

        public enum Mode
        {
            Off,
            Color,
            ColorSweep
        }
    }
}

namespace P5RPC.ColorStuff.Configuration
{
    public class ConfiguratorMixin : ConfiguratorMixinBase
    {
    }
}
