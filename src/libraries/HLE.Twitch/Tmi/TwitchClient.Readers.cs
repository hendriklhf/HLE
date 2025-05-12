using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;

namespace HLE.Twitch.Tmi;

public sealed partial class TwitchClient
{
    [SuppressMessage("Critical Code Smell", "S4487:Unread \"private\" fields should be removed", Justification = "analyzer is wrong")]
    private ref struct Readers
    {
        public ref ChannelReader<ChatMessage>? ChatMessageReader;
        public ref ChannelReader<Roomstate> RoomstateReader;
        public ref ChannelReader<JoinChannelMessage>? JoinReader;
        public ref ChannelReader<PartChannelMessage>? PartReader;
        public ref ChannelReader<Notice>? NoticeReader;
        public ref ChannelReader<Bytes> PingReader;
    }
}
