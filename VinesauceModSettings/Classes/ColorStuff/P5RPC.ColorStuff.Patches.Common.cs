using System;
using System.Runtime.CompilerServices;
using P5RPC.ColorStuff.Utilities;
using Reloaded.Hooks.Definitions;

// original code by zarroboogs
namespace P5RPC.ColorStuff.Patches.Common
{
    internal ref struct PatchContext
    {
        public IntPtr BaseAddress { readonly get; set; }

        public p5rpc.menucolor.Configuration.Config Config { readonly get; set; }

        public SigScanHelper ScanHelper { readonly get; set; }

        public IReloadedHooks Hooks { readonly get; set; }

        public Logger Logger { readonly get; set; }
    }
}
