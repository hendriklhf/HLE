using System;

namespace HLE.Resources;

public interface IResourceReader
{
    Resource Read(ReadOnlySpan<char> resourceName);

    bool TryRead(ReadOnlySpan<char> resourceName, out Resource resource);
}
