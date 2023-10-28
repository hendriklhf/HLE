using System;
using HLE.Twitch;
using HLE.Twitch.Models;
using Xunit;

namespace HLE.Tests.Twitch;

public sealed class IrcHandlerTest
{
    private const string _privMsg =
        "@badge-info=;badges=moderator/1,twitchconEU2022/1;color=#C29900;display-name=Strbhlfe;emotes=;first-msg=0;flags=;id=03c90865-31ff-493f-a711-dcd6d788624b;mod=1;rm-received-ts=1654020884037;room-id=616177816;subscriber=0;tmi-sent-ts=1654020883875;turbo=0;user-id=87633910;user-type=mod :strbhlfe!strbhlfe@strbhlfe.tmi.twitch.tv PRIVMSG #lbnshlfe :xd xd xd";
    private const string _privMsgAction =
        "@badge-info=;badges=moderator/1,twitchconEU2022/1;color=#C29900;display-name=Strbhlfe;emotes=;first-msg=0;flags=;id=03c90865-31ff-493f-a711-dcd6d788624b;mod=1;returning-chatter=0;room-id=616177816;subscriber=0;tmi-sent-ts=1654020883875;turbo=0;user-id=87633910;user-type=mod :strbhlfe!strbhlfe@strbhlfe.tmi.twitch.tv PRIVMSG #lbnshlfe :\u0001ACTION xd xd xd\u0001";
    private const string _roomstateAllOff = "@emote-only=0;followers-only=-1;r9k=0;room-id=87633910;slow=0;subs-only=0 :tmi.twitch.tv ROOMSTATE #strbhlfe";
    private const string _roomstateAllOn = "@emote-only=1;followers-only=15;r9k=1;room-id=87633910;slow=10;subs-only=1 :tmi.twitch.tv ROOMSTATE #strbhlfe";
    private const string _join = ":strbhlfe!strbhlfe@strbhlfe.tmi.twitch.tv JOIN #lbnshlfe";
    private const string _part = ":strbhlfe!strbhlfe@strbhlfe.tmi.twitch.tv PART #lbnshlfe";
    private const string _noticeWithTag = "@msg-id=already_emote_only_off :tmi.twitch.tv NOTICE #lbnshlfe :This room is not in emote-only mode.";
    private const string _noticeWithoutTag = ":tmi.twitch.tv NOTICE * :Login authentication failed";

    [Theory]
    [InlineData(ParsingMode.TimeEfficient)]
    [InlineData(ParsingMode.MemoryEfficient)]
    [InlineData(ParsingMode.Balanced)]
    public void PrivMsgTest(ParsingMode parsingMode)
    {
        IrcHandler handler = new(parsingMode);
        handler.OnChatMessageReceived += static (_, chatMessage) =>
        {
            Assert.Equal(0, chatMessage.BadgeInfos.Length);
            Assert.Equal(2, chatMessage.Badges.Length);
            Assert.Equal("1", chatMessage.Badges[0].Level);
            Assert.Equal("1", chatMessage.Badges[1].Level);
            Assert.Equal(0xC2, chatMessage.Color.Red);
            Assert.Equal(0x99, chatMessage.Color.Green);
            Assert.Equal(0x00, chatMessage.Color.Blue);
            Assert.Equal("Strbhlfe", chatMessage.DisplayName);
            Assert.False(chatMessage.IsFirstMessage);
            Assert.Equal(Guid.Parse("03c90865-31ff-493f-a711-dcd6d788624b"), chatMessage.Id);
            Assert.True(chatMessage.IsModerator);
            Assert.Equal(616177816, chatMessage.ChannelId);
            Assert.False(chatMessage.IsSubscriber);
            Assert.Equal(1654020883875, chatMessage.TmiSentTs);
            Assert.False(chatMessage.IsTurboUser);
            Assert.Equal(87633910, chatMessage.UserId);
            Assert.Equal("strbhlfe", chatMessage.Username);
            Assert.Equal("lbnshlfe", chatMessage.Channel);
            Assert.Equal("xd xd xd", chatMessage.Message);
            if (chatMessage is IDisposable disposable)
            {
                disposable.Dispose();
            }
        };

        Assert.True(handler.Handle(_privMsg));
        Assert.True(handler.Handle(_privMsgAction));
    }

    [Theory]
    [InlineData(ParsingMode.TimeEfficient)]
    [InlineData(ParsingMode.MemoryEfficient)]
    [InlineData(ParsingMode.Balanced)]
    public void Roomstate_AllOff_Test(ParsingMode parsingMode)
    {
        IrcHandler handler = new(parsingMode);
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        handler.OnRoomstateReceived += static (_, roomstateArgs) =>
        {
            Assert.False(roomstateArgs.EmoteOnly);
            Assert.Equal(-1, roomstateArgs.FollowersOnly);
            Assert.False(roomstateArgs.R9K);
            Assert.Equal(87633910, roomstateArgs.ChannelId);
            Assert.Equal(0, roomstateArgs.SlowMode);
            Assert.False(roomstateArgs.SubsOnly);
            Assert.Equal("strbhlfe", roomstateArgs.Channel);
        };

        Assert.True(handler.Handle(_roomstateAllOff));
    }

    [Theory]
    [InlineData(ParsingMode.TimeEfficient)]
    [InlineData(ParsingMode.MemoryEfficient)]
    [InlineData(ParsingMode.Balanced)]
    public void Roomstate_AllOn_Test(ParsingMode parsingMode)
    {
        IrcHandler handler = new(parsingMode);
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        handler.OnRoomstateReceived += static (_, roomstateArgs) =>
        {
            Assert.True(roomstateArgs.EmoteOnly);
            Assert.Equal(15, roomstateArgs.FollowersOnly);
            Assert.True(roomstateArgs.R9K);
            Assert.Equal(87633910, roomstateArgs.ChannelId);
            Assert.Equal(10, roomstateArgs.SlowMode);
            Assert.True(roomstateArgs.SubsOnly);
            Assert.Equal("strbhlfe", roomstateArgs.Channel);
        };

        Assert.True(handler.Handle(_roomstateAllOn));
    }

    [Theory]
    [InlineData(ParsingMode.TimeEfficient)]
    [InlineData(ParsingMode.MemoryEfficient)]
    [InlineData(ParsingMode.Balanced)]
    public void JoinTest(ParsingMode parsingMode)
    {
        IrcHandler handler = new(parsingMode);
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        handler.OnJoinReceived += static (_, joinedChannelArgs) =>
        {
            Assert.Equal("strbhlfe", joinedChannelArgs.Username);
            Assert.Equal("lbnshlfe", joinedChannelArgs.Channel);
        };

        Assert.True(handler.Handle(_join));
    }

    [Theory]
    [InlineData(ParsingMode.TimeEfficient)]
    [InlineData(ParsingMode.MemoryEfficient)]
    [InlineData(ParsingMode.Balanced)]
    public void PartTest(ParsingMode parsingMode)
    {
        IrcHandler handler = new(parsingMode);
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        handler.OnPartReceived += static (_, leftChannelArgs) =>
        {
            Assert.Equal("strbhlfe", leftChannelArgs.Username);
            Assert.Equal("lbnshlfe", leftChannelArgs.Channel);
        };

        Assert.True(handler.Handle(_part));
    }

    [Theory]
    [InlineData(ParsingMode.TimeEfficient)]
    [InlineData(ParsingMode.MemoryEfficient)]
    [InlineData(ParsingMode.Balanced)]
    public void Notice_WithTag_Test(ParsingMode parsingMode)
    {
        IrcHandler handler = new(parsingMode);
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        handler.OnNoticeReceived += static (_, notice) =>
        {
            Assert.Equal(NoticeType.AlreadyEmoteOnlyOff, notice.Type);
            Assert.Equal("lbnshlfe", notice.Channel);
            Assert.Equal("This room is not in emote-only mode.", notice.Message);
        };

        Assert.True(handler.Handle(_noticeWithTag));
    }

    [Theory]
    [InlineData(ParsingMode.TimeEfficient)]
    [InlineData(ParsingMode.MemoryEfficient)]
    [InlineData(ParsingMode.Balanced)]
    public void Notice_WithoutTag_Test(ParsingMode parsingMode)
    {
        IrcHandler handler = new(parsingMode);
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        handler.OnNoticeReceived += static (_, notice) =>
        {
            Assert.Equal(NoticeType.Unknown, notice.Type);
            Assert.Equal("*", notice.Channel);
            Assert.Equal("Login authentication failed", notice.Message);
        };

        Assert.True(handler.Handle(_noticeWithoutTag));
    }
}
