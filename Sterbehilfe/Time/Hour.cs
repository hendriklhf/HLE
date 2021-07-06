using Sterbehilfe.Time.Interfaces;

namespace Sterbehilfe.Time
{
    public class Hour : ITimeUnit
    {
        public int Count { get; set; }

        public long Milliseconds => Count * _inMilliseconds;

        public long Seconds => Milliseconds / 1000;

        public const string Pattern = @"\d+h(our)?s?";

        private const long _inMilliseconds = 3600000;

        public Hour(int count = 1)
        {
            Count = count;
        }
    }
}