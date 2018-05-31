using System;

namespace TradingApp.Domain.Enums
{
    [Flags]
    public enum Indicator
    {
        Positive,
        Neutral,
        Negative,
        StrongPositive,
        ZeroRezults
    }
}