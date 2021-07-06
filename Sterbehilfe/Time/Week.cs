using Sterbehilfe.Time.Interfaces;

namespace Sterbehilfe.Time
{
    public class Week : ITimeUnit
    {
        public int Count { get; set; }

        public long Milliseconds => new Day(7 * Count).Milliseconds;

        public long Seconds => Milliseconds / 1000;

        public const string Pattern = @"\d+w(eek)?s?";

        public Week(int count = 1)
        {
            Count = count;
        }
    }
}
