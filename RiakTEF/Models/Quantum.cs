using System;

namespace RiakTEF.Models
{
    public enum Unit { Days = 'd', Weeks = 'w', Months = 'm', Years = 'y' }

    public struct Quantum
    {
        public Quantum(uint interval, Unit unit)
        {
            if (0 == interval) throw new ArgumentOutOfRangeException(nameof(interval));

            Interval = interval;
            Unit     = unit;
        }

        public uint Interval { get; }
        public Unit Unit     { get; }
    }
}