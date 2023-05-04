using System;
using HLE.Twitch;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.Twitch;

[TestClass]
public class IrcHandlerTest
{
    private readonly IrcHandler _ircHandler = new();

    private readonly string[] _messages =
    {
        "@badge-info=;badges=moderator/1,twitchconEU2022/1;color=#C29900;display-name=Strbhlfe;emotes=;first-msg=0;flags=;id=03c90865-31ff-493f-a711-dcd6d788624b;mod=1;rm-received-ts=1654020884037;room-id=616177816;subscriber=0;tmi-sent-ts=1654020883875;turbo=0;user-id=87633910;user-type=mod :strbhlfe!strbhlfe@strbhlfe.tmi.twitch.tv PRIVMSG #lbnshlfe :xd xd xd",
        "@emote-only=0;followers-only=-1;r9k=0;room-id=87633910;slow=0;subs-only=0 :tmi.twitch.tv ROOMSTATE #strbhlfe",
        "@emote-only=1;followers-only=15;r9k=1;room-id=87633910;slow=10;subs-only=1 :tmi.twitch.tv ROOMSTATE #strbhlfe",
        ":strbhlfe!strbhlfe@strbhlfe.tmi.twitch.tv JOIN #lbnshlfe",
        ":strbhlfe!strbhlfe@strbhlfe.tmi.twitch.tv PART #lbnshlfe",
        "@badge-info=;badges=moderator/1,twitchconEU2022/1;color=#C29900;display-name=Strbhlfe;emotes=;first-msg=0;flags=;id=03c90865-31ff-493f-a711-dcd6d788624b;mod=1;returning-chatter=0;room-id=616177816;subscriber=0;tmi-sent-ts=1654020883875;turbo=0;user-id=87633910;user-type=mod :strbhlfe!strbhlfe@strbhlfe.tmi.twitch.tv PRIVMSG #lbnshlfe :\u0001ACTION xd xd xd\u0001"
    };

    [DataRow(0)]
    [DataRow(5)]
    [TestMethod]
    public void PrivMsgTest(int messageIdx)
    {
        _ircHandler.OnChatMessageReceived += (_, chatMessage) =>
        {
            Assert.AreEqual(0, chatMessage.BadgeInfos.Length);
            Assert.AreEqual(2, chatMessage.Badges.Length);
            Assert.AreEqual("1", chatMessage.Badges[0].Level);
            Assert.AreEqual("1", chatMessage.Badges[1].Level);
            Assert.AreEqual(0xC2, chatMessage.Color.Red);
            Assert.AreEqual(0x99, chatMessage.Color.Green);
            Assert.AreEqual(0x00, chatMessage.Color.Blue);
            Assert.AreEqual("Strbhlfe", chatMessage.DisplayName);
            Assert.AreEqual(false, chatMessage.IsFirstMessage);
            Assert.AreEqual(Guid.Parse("03c90865-31ff-493f-a711-dcd6d788624b"), chatMessage.Id);
            Assert.AreEqual(true, chatMessage.IsModerator);
            Assert.AreEqual(616177816, chatMessage.ChannelId);
            Assert.AreEqual(false, chatMessage.IsSubscriber);
            Assert.AreEqual(1654020883875, chatMessage.TmiSentTs);
            Assert.AreEqual(false, chatMessage.IsTurboUser);
            Assert.AreEqual(87633910, chatMessage.UserId);
            Assert.AreEqual("strbhlfe", chatMessage.Username);
            Assert.AreEqual("lbnshlfe", chatMessage.Channel);
            Assert.AreEqual("xd xd xd", chatMessage.Message);
        };
        Assert.IsTrue(_ircHandler.Handle(_messages[messageIdx]));
    }

    [TestMethod]
    public void RoomstateTestAllOff()
    {
        _ircHandler.OnRoomstateReceived += (_, roomstateArgs) =>
        {
            Assert.AreEqual(false, roomstateArgs.EmoteOnly);
            Assert.AreEqual(-1, roomstateArgs.FollowersOnly);
            Assert.AreEqual(false, roomstateArgs.R9K);
            Assert.AreEqual(87633910, roomstateArgs.ChannelId);
            Assert.AreEqual(0, roomstateArgs.SlowMode);
            Assert.AreEqual(false, roomstateArgs.SubsOnly);
            Assert.AreEqual("strbhlfe", roomstateArgs.Channel);
        };
        Assert.IsTrue(_ircHandler.Handle(_messages[1]));
    }

    [TestMethod]
    public void RoomstateTestAllOn()
    {
        _ircHandler.OnRoomstateReceived += (_, roomstateArgs) =>
        {
            Assert.AreEqual(true, roomstateArgs.EmoteOnly);
            Assert.AreEqual(15, roomstateArgs.FollowersOnly);
            Assert.AreEqual(true, roomstateArgs.R9K);
            Assert.AreEqual(87633910, roomstateArgs.ChannelId);
            Assert.AreEqual(10, roomstateArgs.SlowMode);
            Assert.AreEqual(true, roomstateArgs.SubsOnly);
            Assert.AreEqual("strbhlfe", roomstateArgs.Channel);
        };
        Assert.IsTrue(_ircHandler.Handle(_messages[2]));
    }

    [TestMethod]
    public void JoinTest()
    {
        _ircHandler.OnJoinedChannel += (_, joinedChannelArgs) =>
        {
            Assert.AreEqual("strbhlfe", joinedChannelArgs.Username);
            Assert.AreEqual("lbnshlfe", joinedChannelArgs.Channel);
        };
        Assert.IsTrue(_ircHandler.Handle(_messages[3]));
    }

    [TestMethod]
    public void PartTest()
    {
        _ircHandler.OnLeftChannel += (_, leftChannelArgs) =>
        {
            Assert.AreEqual("strbhlfe", leftChannelArgs.Username);
            Assert.AreEqual("lbnshlfe", leftChannelArgs.Channel);
        };
        Assert.IsTrue(_ircHandler.Handle(_messages[4]));
    }
}
