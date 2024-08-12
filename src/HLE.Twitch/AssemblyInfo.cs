using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("HLE.Debug")]
[assembly: InternalsVisibleTo("HLE.Twitch.UnitTests")]
[assembly: SuppressMessage("Major Code Smell", "S6354:Use a testable date/time provider")] // TODO: remove
