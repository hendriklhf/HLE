namespace Sterbehilfe.Utils.Time
{
    public class Day : ITimeUnit
    {
        public int Count { get; set; }

        public const string Pattern = @"\d+d(ay)?s?";

        private const long _inMilliseconds = 86400000;

        public Day(int count = 1)
        {
            Count = count;
        }

        public long ToMilliseconds()
        {
            return Count * _inMilliseconds;
        }

        public long ToSeconds()
        {
            return ToMilliseconds() / 1000;
        }
    }
}