using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text;
using HLE.Collections;
using HLE.Memory;
using HLE.Strings;

#pragma warning disable CS0660, CS0661, CS0659 // Justification: base class overrides GetHashCode

namespace HLE.Twitch.Tmi.Models;

public sealed class MemoryEfficientChatMessage : ChatMessage, IEquatable<MemoryEfficientChatMessage>
{
    public override ReadOnlySpan<Badge> BadgeInfos
    {
        get
        {
            if (_badgeInfos is null)
            {
                ThrowHelper.ThrowObjectDisposedException<MemoryEfficientChatMessage>();
            }

            return _badgeInfos.AsSpanUnsafe(.._badgeInfoCount);
        }
    }

    public override ReadOnlySpan<Badge> Badges
    {
        get
        {
            if (_badges is null)
            {
                ThrowHelper.ThrowObjectDisposedException<MemoryEfficientChatMessage>();
            }

            return _badges.AsSpanUnsafe(.._badgeCount);
        }
    }

    public override required string DisplayName
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _displayName ??= GetDisplayNameFromBuffer();
        init { }
    }

    public override required string Username
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _username ??= GetUsernameFromBuffer();
        init { }
    }

    private Badge[]? _badgeInfos;
    private readonly int _badgeInfoCount;

    private Badge[]? _badges;
    private readonly int _badgeCount;

    private string? _displayName;
    private string? _username;

    private byte[]? _displayNameBuffer;
    internal byte[]? _usernameBuffer;
    internal readonly int _nameLength;

    /// <summary>
    /// The default constructor of <see cref="MemoryEfficientChatMessage"/>.
    /// </summary>
    /// <param name="badgeInfos">The badge info buffer.</param>
    /// <param name="badgeInfoCount">The amount of written elements in the badge info buffer.</param>
    /// <param name="badges">The badge buffer.</param>
    /// <param name="badgeCount">The amount of written elements in the badge buffer.</param>
    /// <param name="flags">The chat message flags.</param>
    /// <param name="displayName">The user's display name.</param>
    /// <param name="username">The user's username buffer.</param>
    /// <param name="nameLength">The length of the user's username.</param>
    public MemoryEfficientChatMessage(Badge[] badgeInfos, int badgeInfoCount, Badge[] badges, int badgeCount, ChatMessageFlags flags,
        byte[] displayName, byte[] username, int nameLength)
    {
        _badgeInfos = badgeInfos;
        _badgeInfoCount = badgeInfoCount;
        _badges = badges;
        _badgeCount = badgeCount;
        _flags = flags;

        _displayNameBuffer = displayName;
        _usernameBuffer = username;
        _nameLength = nameLength;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        ArrayPool<Badge>.Shared.Return(_badgeInfos);
        _badgeInfos = null;

        ArrayPool<Badge>.Shared.Return(_badges);
        _badges = null;

        ArrayPool<byte>.Shared.Return(_displayNameBuffer);
        _displayNameBuffer = null;

        ArrayPool<byte>.Shared.Return(_usernameBuffer);
        _usernameBuffer = null;

        Message.Dispose();
    }

    [MethodImpl(MethodImplOptions.Synchronized | MethodImplOptions.NoInlining)]
    private string GetDisplayNameFromBuffer()
    {
        byte[]? displayNameBuffer = _displayNameBuffer;
        if (displayNameBuffer is null)
        {
            ThrowHelper.ThrowObjectDisposedException<MemoryEfficientChatMessage>();
        }

        ReadOnlySpan<byte> displayNameBufferSpan = displayNameBuffer.AsSpan(0, _nameLength);
        string displayName = StringPool.Shared.GetOrAdd(displayNameBufferSpan, Encoding.ASCII);

        ArrayPool<byte>.Shared.Return(displayNameBuffer);
        _displayNameBuffer = null;
        return displayName;
    }

    [MethodImpl(MethodImplOptions.Synchronized | MethodImplOptions.NoInlining)]
    private string GetUsernameFromBuffer()
    {
        byte[]? usernameBuffer = _usernameBuffer;
        if (usernameBuffer is null)
        {
            ThrowHelper.ThrowObjectDisposedException<MemoryEfficientChatMessage>();
        }

        ReadOnlySpan<byte> usernameBufferSpan = usernameBuffer.AsSpan(0, _nameLength);
        string username = StringPool.Shared.GetOrAdd(usernameBufferSpan, Encoding.ASCII);

        ArrayPool<byte>.Shared.Return(usernameBuffer);
        _usernameBuffer = null;
        return username;
    }

    [Pure]
    public bool Equals([NotNullWhen(true)] MemoryEfficientChatMessage? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    public static bool operator ==(MemoryEfficientChatMessage? left, MemoryEfficientChatMessage? right) => Equals(left, right);

    public static bool operator !=(MemoryEfficientChatMessage? left, MemoryEfficientChatMessage? right) => !(left == right);
}
