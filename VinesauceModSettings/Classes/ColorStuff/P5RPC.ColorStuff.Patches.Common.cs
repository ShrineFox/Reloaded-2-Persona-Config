using System;
using System.Runtime.CompilerServices;
using Reloaded.Hooks.Definitions;
using VinesauceModSettings.Configuration;
using VinesauceModSettings.Template.Configuration;

namespace VinesauceModSettings
{
    internal ref struct PatchContext
    {
        public IntPtr BaseAddress { readonly get; set; }

        public SigScanHelper ScanHelper { readonly get; set; }
        public Configurable<Config> Config { readonly get; set; }

        public IReloadedHooks Hooks { readonly get; set; }

        public Logger Logger { readonly get; set; }
    }
}
