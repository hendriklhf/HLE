using HLE.Time.Interfaces;

namespace HLE.Time
{
    public class Minute : ITimeUnit
    {
        public int Count { get; set; }

        public long Milliseconds => Count * _inMilliseconds;

        public long Seconds => Milliseconds / 1000;

        public string Pattern => @"\d+m(in(ute)?)?s?";

        private const long _inMilliseconds = 60000;

        public Minute(int count = 1)
        {
            Count = count;
        }
    }
}