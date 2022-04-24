using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HLE.Time;

public class UnixDiffSpan
{
    [Attributes.TimeUnit("y")]
    public uint Years { get; set; }

    [Attributes.TimeUnit("d")]
    public ushort Days { get; set; }

    [Attributes.TimeUnit("h")]
    public byte Hours { get; set; }

    [Attributes.TimeUnit("min")]
    public byte Minutes { get; set; }

    [Attributes.TimeUnit("s")]
    public byte Seconds { get; set; }

    [Attributes.TimeUnit("ms")]
    public ushort Milliseconds { get; set; }

    public override string ToString()
    {
        IEnumerable<string> timeValues = typeof(UnixDiffSpan).GetProperties().Select(p =>
        {
            Attributes.TimeUnit? unit = p.GetCustomAttribute<Attributes.TimeUnit>();
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

        return string.Join(", ", timeValues);
    }
}
