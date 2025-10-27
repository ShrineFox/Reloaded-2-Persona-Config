using System.Runtime.InteropServices;

namespace VinesauceModSettings;

[StructLayout(LayoutKind.Explicit, Size = 0x00000030)]
internal struct EPLAnimationController {
  [FieldOffset(0x00000004)] public float Duration;
}
