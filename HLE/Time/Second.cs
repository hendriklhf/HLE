using HLE.Time.Interfaces;

namespace HLE.Time
{
    public class Second : ITimeUnit
    {
        public int Count { get; set; }

        public long Milliseconds => Count * _inMilliseconds;

        public long Seconds => Milliseconds / 1000;

        public string Pattern => @"\d+s(ec(ond)?)?s?";

        private const long _inMilliseconds = 1000;

        public Second(int count = 1)
        {
            Count = count;
        }
    }
}