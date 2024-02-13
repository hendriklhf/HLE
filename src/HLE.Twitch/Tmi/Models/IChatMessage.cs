using System;
using HLE.Strings;

namespace HLE.Twitch.Tmi.Models;

public interface IChatMessage : IDisposable, IEquatable<IChatMessage>, ISpanFormattable
{
    /// <summary>
    /// Holds information about a badge, that can be obtained by its name found in <see cref="Badges"/>.
    /// </summary>
    ReadOnlySpan<Badge> BadgeInfos { get; }

    /// <summary>
    /// Holds all the badges the user has.
    /// </summary>
    ReadOnlySpan<Badge> Badges { get; }

    /// <summary>
    /// The color of the user's name in a Twitch chat overlay.
    /// If the user does not have a color, the value is "Color.Empty".
    /// </summary>
    Color Color { get; }

    /// <summary>
    /// The display name of the user with the preferred casing.
    /// </summary>
    LazyString DisplayName { get; }

    /// <summary>
    /// Indicates whether the message is the first message the user has sent in the channel or not.
    /// </summary>
    bool IsFirstMessage { get; }

    /// <summary>
    /// The unique message id.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Indicates whether the user is a moderator or not.
    /// </summary>
    bool IsModerator { get; }

    /// <summary>
    /// The user id of the channel owner.
    /// </summary>
    long ChannelId { get; }

    /// <summary>
    /// Indicates whether the user is a subscriber or not.
    /// The subscription age can be obtained from <see cref="Badges"/> and <see cref="BadgeInfos"/>.
    /// </summary>
    bool IsSubscriber { get; }

    /// <summary>
    /// The unix timestamp in milliseconds of the moment the message has been sent.
    /// </summary>
    long TmiSentTs { get; }

    /// <summary>
    /// Indicates whether the user is subscribing to Twitch Turbo or not.
    /// </summary>
    bool IsTurboUser { get; }

    /// <summary>
    /// The user id of the user who sent the message.
    /// </summary>
    long UserId { get; }

    /// <summary>
    /// Indicates whether the message was sent as an action (prefixed with "/me") or not.
    /// </summary>
    bool IsAction { get; }

    /// <summary>
    /// The username of the user who sent the message. All lower case.
    /// </summary>
    LazyString Username { get; }

    /// <summary>
    /// The username of the channel owner. All lower case, without '#'.
    /// </summary>
    string Channel { get; }

    /// <summary>
    /// The message content.
    /// </summary>
    LazyString Message { get; }
}
