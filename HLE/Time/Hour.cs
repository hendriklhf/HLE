using HLE.Time.Interfaces;

namespace HLE.Time
{
    public class Hour : ITimeUnit
    {
        public int Count { get; set; }

        public long Milliseconds => Count * _inMilliseconds;

        public long Seconds => Milliseconds / 1000;

        public string Pattern => @"\d+h(our)?s?";

        private const long _inMilliseconds = 3600000;

        public Hour(int count = 1)
        {
            Count = count;
        }
    }
}