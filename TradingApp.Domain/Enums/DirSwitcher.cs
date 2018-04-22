using System;

namespace TradingApp.Domain.Enums
{
    [Flags]
    public enum DirSwitcher
    {
        Auto,
        Manual,
        Instant
    }
}