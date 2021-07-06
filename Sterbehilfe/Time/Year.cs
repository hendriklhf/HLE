using Sterbehilfe.Time.Interfaces;

namespace Sterbehilfe.Time
{
    public class Year : ITimeUnit
    {
        public int Count { get; set; }

        public long Milliseconds => Count * _inMilliseconds;

        public long Seconds => Milliseconds / 1000;

        public const string Pattern = @"\d+y(ear)?s?";

        private const long _inMilliseconds = 31556952000;

        public Year(int count = 1)
        {
            Count = count;
        }
    }
}