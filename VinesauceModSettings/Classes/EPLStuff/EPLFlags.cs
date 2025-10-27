namespace VinesauceModSettings;

internal enum EPLFlags {
  LoopPlayback           = (1 << 0),
  PlayOnce               = (1 << 1),
  StopAtDuration         = (1 << 2),
  TimeFromWin32Timestamp = (1 << 4),
}
