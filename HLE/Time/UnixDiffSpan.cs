using HLE.Time.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HLE.Time;

public class UnixDiffSpan
{
    [TimeUnit("y")]
    public uint Years { get; set; }

    [TimeUnit("d")]
    public ushort Days { get; set; }

    [TimeUnit("h")]
    public byte Hours { get; set; }

    [TimeUnit("min")]
    public byte Minutes { get; set; }

    [TimeUnit("s")]
    public byte Seconds { get; set; }

    [TimeUnit("ms")]
    public ushort Milliseconds { get; set; }

    public override string ToString()
    {
        StringBuilder builder = new();

        IEnumerable<string> timeValues = typeof(UnixDiffSpan).GetProperties().Select(p =>
        {
            TimeUnit? unit = p.GetCustomAttribute<TimeUnit>();
            string? value = p.GetValue(this)?.ToString();

            if (unit is null)
            {
                throw new ArgumentNullException(nameof(unit));
            }

            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return $"{value}{unit.Value}";
        }).Where(t => t[0] != '0');

        builder.AppendJoin(", ", timeValues);
        return builder.ToString();
    }
}
