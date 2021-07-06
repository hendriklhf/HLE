using Sterbehilfe.Time.Interfaces;

namespace Sterbehilfe.Time
{
    public class Second : ITimeUnit
    {
        public int Count { get; set; }

        public long Milliseconds => Count * _inMilliseconds;

        public long Seconds => Milliseconds / 1000;

        public const string Pattern = @"\d+s(ec(ond)?)?s?";

        private const long _inMilliseconds = 1000;

        public Second(int count = 1)
        {
            Count = count;
        }
    }
}