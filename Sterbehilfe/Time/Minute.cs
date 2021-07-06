using Sterbehilfe.Time.Interfaces;

namespace Sterbehilfe.Time
{
    public class Minute : ITimeUnit
    {
        public int Count { get; set; }

        public long Milliseconds => Count * _inMilliseconds;

        public long Seconds => Milliseconds / 1000;

        public const string Pattern = @"\d+m(in(ute)?)?s?";

        private const long _inMilliseconds = 60000;

        public Minute(int count = 1)
        {
            Count = count;
        }
    }
}