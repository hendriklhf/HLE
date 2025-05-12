using System;
using System.Diagnostics.CodeAnalysis;

namespace HLE.Twitch.Tmi;

public readonly partial struct ClientOptions
{
    [Flags]
    [SuppressMessage("Design", "CA1028:Enum Storage should be Int32")]
    [SuppressMessage("Minor Code Smell", "S4022:Enumerations should have \"Int32\" storage")]
    [SuppressMessage("Minor Code Smell", "S2344:Enumeration type names should not have \"Flags\" or \"Enum\" suffixes")]
    private enum Flags : byte
    {
        None = 0,
        UseSsl = 1 << 0,
        IsVerifiedBot = 1 << 1,
        ReceiveMembershipMessages = 1 << 2,
        ReceiveRoomstateMessages = 1 << 3,
        ReceiveChatMessages = 1 << 4,
        ReceiveNoticeMessages = 1 << 5
    }
}
