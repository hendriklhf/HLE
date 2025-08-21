using System;
using System.Threading.Channels;
using HLE.Twitch.Tmi;

namespace HLE.Twitch.UnitTests;

public sealed class IrcHandlerTest
{
    private static ReadOnlySpan<byte> PrivMsg =>
        "@badge-info=;badges=moderator/1,twitchconEU2022/1;color=#C29900;display-name=Strbhlfe;emotes=;first-msg=0;flags=;id=03c90865-31ff-493f-a711-dcd6d788624b;mod=1;rm-received-ts=1654020884037;room-id=616177816;subscriber=0;tmi-sent-ts=1654020883875;turbo=0;user-id=87633910;user-type=mod :strbhlfe!strbhlfe@strbhlfe.tmi.twitch.tv PRIVMSG #lbnshlfe :xd xd xd"u8;
    private static ReadOnlySpan<byte> PrivMsgAction =>
        "@badge-info=;badges=moderator/1,twitchconEU2022/1;color=#C29900;display-name=Strbhlfe;emotes=;first-msg=0;flags=;id=03c90865-31ff-493f-a711-dcd6d788624b;mod=1;returning-chatter=0;room-id=616177816;subscriber=0;tmi-sent-ts=1654020883875;turbo=0;user-id=87633910;user-type=mod :strbhlfe!strbhlfe@strbhlfe.tmi.twitch.tv PRIVMSG #lbnshlfe :\u0001ACTION xd xd xd\u0001"u8;
    private static ReadOnlySpan<byte> RoomstateAllOff =>
        "@emote-only=0;followers-only=-1;r9k=0;room-id=87633910;slow=0;subs-only=0 :tmi.twitch.tv ROOMSTATE #strbhlfe"u8;
    private static ReadOnlySpan<byte> RoomstateAllOn =>
        "@emote-only=1;followers-only=15;r9k=1;room-id=87633910;slow=10;subs-only=1 :tmi.twitch.tv ROOMSTATE #strbhlfe"u8;

    private static ReadOnlySpan<byte> Join => ":strbhlfe!strbhlfe@strbhlfe.tmi.twitch.tv JOIN #lbnshlfe"u8;

    private static ReadOnlySpan<byte> Part => ":strbhlfe!strbhlfe@strbhlfe.tmi.twitch.tv PART #lbnshlfe"u8;

    private static ReadOnlySpan<byte> NoticeWithTag =>
        "@msg-id=already_emote_only_off :tmi.twitch.tv NOTICE #lbnshlfe :This room is not in emote-only mode."u8;

    private static ReadOnlySpan<byte> NoticeWithoutTag => ":tmi.twitch.tv NOTICE * :Login authentication failed"u8;

    [Fact]
    public void PrivMsgTest()
    {
        Channel<ChatMessage> channel = System.Threading.Channels.Channel.CreateUnbounded<ChatMessage>();

        IrcHandler handler = new(null!, channel.Writer, null, null, null, null);

        Assert.True(handler.Handle(PrivMsg));
        Assert.True(handler.Handle(PrivMsgAction));

        for (int i = 0; i < 2; i++)
        {
            bool success = channel.Reader.TryRead(out ChatMessage? chatMessage);
            Assert.True(success);

            Assert.Equal(0, chatMessage!.BadgeInfos.Length);
            Assert.Equal(2, chatMessage.Badges.Length);
            Assert.Equal("1", chatMessage.Badges[0].Level);
            Assert.Equal("1", chatMessage.Badges[1].Level);
            Assert.Equal(0xC2, chatMessage.Color.Red);
            Assert.Equal(0x99, chatMessage.Color.Green);
            Assert.Equal(0x00, chatMessage.Color.Blue);
            Assert.Equal("Strbhlfe", chatMessage.DisplayName.ToString());
            Assert.False(chatMessage.IsFirstMessage);
            Assert.Equal(Guid.Parse("03c90865-31ff-493f-a711-dcd6d788624b"), chatMessage.Id);
            Assert.True(chatMessage.IsModerator);
            Assert.Equal(616_177_816, chatMessage.ChannelId);
            Assert.False(chatMessage.IsSubscriber);
            Assert.Equal(1_654_020_883_875, chatMessage.TmiSentTs);
            Assert.False(chatMessage.IsTurboUser);
            Assert.Equal(87_633_910, chatMessage.UserId);
            Assert.Equal("strbhlfe", chatMessage.Username.ToString());
            Assert.Equal("lbnshlfe", chatMessage.Channel);
            Assert.Equal("xd xd xd", chatMessage.Message.ToString());

            chatMessage.Dispose();
        }
    }

    [Fact]
    public void Roomstate_AllOff_Test()
    {
        Channel<Roomstate> channel = System.Threading.Channels.Channel.CreateUnbounded<Roomstate>();

        IrcHandler handler = new(null!, null, channel.Writer, null, null, null);

        Assert.True(handler.Handle(RoomstateAllOff));

        bool success = channel.Reader.TryRead(out Roomstate roomstate);
        Assert.True(success);

        Assert.False(roomstate.EmoteOnly);
        Assert.Equal(-1, roomstate.FollowersOnly);
        Assert.False(roomstate.R9K);
        Assert.Equal(87_633_910, roomstate.ChannelId);
        Assert.Equal(0, roomstate.SlowMode);
        Assert.False(roomstate.SubsOnly);
        Assert.Equal("strbhlfe", roomstate.Channel);
    }

    [Fact]
    public void Roomstate_AllOn_Test()
    {
        Channel<Roomstate> channel = System.Threading.Channels.Channel.CreateUnbounded<Roomstate>();

        IrcHandler handler = new(null!, null, channel.Writer, null, null, null);

        Assert.True(handler.Handle(RoomstateAllOn));

        bool success = channel.Reader.TryRead(out Roomstate roomstate);
        Assert.True(success);

        Assert.True(roomstate.EmoteOnly);
        Assert.Equal(15, roomstate.FollowersOnly);
        Assert.True(roomstate.R9K);
        Assert.Equal(87_633_910, roomstate.ChannelId);
        Assert.Equal(10, roomstate.SlowMode);
        Assert.True(roomstate.SubsOnly);
        Assert.Equal("strbhlfe", roomstate.Channel);
    }

    [Fact]
    public void JoinTest()
    {
        Channel<JoinChannelMessage> channel = System.Threading.Channels.Channel.CreateUnbounded<JoinChannelMessage>();

        IrcHandler handler = new(null!, null, null, channel.Writer, null, null);

        Assert.True(handler.Handle(Join));

        bool success = channel.Reader.TryRead(out JoinChannelMessage joinChannelMessage);
        Assert.True(success);

        Assert.Equal("strbhlfe", joinChannelMessage.Username);
        Assert.Equal("lbnshlfe", joinChannelMessage.Channel);
    }

    [Fact]
    public void PartTest()
    {
        Channel<PartChannelMessage> channel = System.Threading.Channels.Channel.CreateUnbounded<PartChannelMessage>();

        IrcHandler handler = new(null!, null, null, null, channel.Writer, null);

        Assert.True(handler.Handle(Part));

        bool success = channel.Reader.TryRead(out PartChannelMessage partChannelMessage);
        Assert.True(success);

        Assert.Equal("strbhlfe", partChannelMessage.Username);
        Assert.Equal("lbnshlfe", partChannelMessage.Channel);
    }

    [Fact]
    public void Notice_WithTag_Test()
    {
        Channel<Notice> channel = System.Threading.Channels.Channel.CreateUnbounded<Notice>();

        IrcHandler handler = new(null!, null, null, null, null, channel.Writer);

        Assert.True(handler.Handle(NoticeWithTag));

        bool success = channel.Reader.TryRead(out Notice notice);
        Assert.True(success);

        Assert.Equal(NoticeType.AlreadyEmoteOnlyOff, notice.Type);
        Assert.Equal("lbnshlfe", notice.Channel);
        Assert.Equal("This room is not in emote-only mode.", notice.Message);
    }

    [Fact]
    public void Notice_WithoutTag_Test()
    {
        Channel<Notice> channel = System.Threading.Channels.Channel.CreateUnbounded<Notice>();

        IrcHandler handler = new(null!, null, null, null, null, channel.Writer);

        Assert.True(handler.Handle(NoticeWithoutTag));

        bool success = channel.Reader.TryRead(out Notice notice);
        Assert.True(success);

        Assert.Equal(NoticeType.Unknown, notice.Type);
        Assert.Equal("*", notice.Channel);
        Assert.Equal("Login authentication failed", notice.Message);
    }
}
