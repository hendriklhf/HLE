namespace Sterbehilfe.Numbers
{
    public static class NumberHelper
    {
        public static double ToDouble(this int i)
        {
            return i;
        }

        public static double ToDouble(this long l)
        {
            return l;
        }

        public static long ToLong(this double d)
        {
            return (long)d;
        }
    }
}