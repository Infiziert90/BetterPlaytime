using System;

namespace BetterPlaytime.Data;

[Flags]
public enum TimeOptions
{
    Normal = 0,
    Seconds = 1,
    Minutes = 2,
    Hours = 4,
    Days = 8,
}