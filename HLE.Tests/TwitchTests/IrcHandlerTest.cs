using System;
using HLE.Twitch;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.TwitchTests;

[TestClass]
public class IrcHandlerTest
{
    private readonly IrcHandler _ircHandler = new();

    private readonly string[] _messages =
    {
        "@badge-info=;badges=moderator/1,twitchconEU2022/1;color=#C29900;display-name=Strbhlfe;emotes=;first-msg=0;flags=;historical=1;id=03c90865-31ff-493f-a711-dcd6d788624b;mod=1;rm-received-ts=1654020884037;room-id=616177816;subscriber=0;tmi-sent-ts=1654020883875;turbo=0;user-id=87633910;user-type=mod :strbhlfe!strbhlfe@strbhlfe.tmi.twitch.tv PRIVMSG #lbnshlfe :xd xd xd",
        "@emote-only=0;followers-only=-1;r9k=0;room-id=87633910;slow=0;subs-only=0 :tmi.twitch.tv ROOMSTATE #strbhlfe",
        "@emote-only=1;followers-only=15;r9k=1;room-id=87633910;slow=10;subs-only=1 :tmi.twitch.tv ROOMSTATE #strbhlfe",
        ":strbhlfe!strbhlfe@strbhlfe.tmi.twitch.tv JOIN #lbnshlfe",
        ":strbhlfe!strbhlfe@strbhlfe.tmi.twitch.tv PART #lbnshlfe"
    };

    [TestMethod]
    public void PrivMsgTest()
    {
        bool invoked = false;
        _ircHandler.OnChatMessageReceived += (_, msg) =>
        {
            invoked = true;
            Assert.AreEqual(0, msg.BadgeInfo.Count);
            Assert.AreEqual(2, msg.Badges.Length);
            Assert.AreEqual(1, msg.Badges[0].Level);
            Assert.AreEqual(1, msg.Badges[1].Level);
            Assert.AreEqual(0xC2, msg.Color.R);
            Assert.AreEqual(0x99, msg.Color.G);
            Assert.AreEqual(0x00, msg.Color.B);
            Assert.AreEqual("Strbhlfe", msg.DisplayName);
            Assert.AreEqual(false, msg.IsFirstMessage);
            Assert.AreEqual(Guid.Parse("03c90865-31ff-493f-a711-dcd6d788624b"), msg.Id);
            Assert.AreEqual(true, msg.IsModerator);
            Assert.AreEqual(616177816, msg.ChannelId);
            Assert.AreEqual(false, msg.IsSubscriber);
            Assert.AreEqual(1654020883875, msg.TmiSentTs);
            Assert.AreEqual(false, msg.IsTurboUser);
            Assert.AreEqual(87633910, msg.UserId);
            Assert.AreEqual("strbhlfe", msg.Username);
            Assert.AreEqual("lbnshlfe", msg.Channel);
            Assert.AreEqual("xd xd xd", msg.Message);
            Assert.AreEqual(_messages[0], msg.RawIrcMessage);
        };
        _ircHandler.Handle(_messages[0]);
        Assert.IsTrue(invoked);
    }

    [TestMethod]
    public void RoomstateTestAllOff()
    {
        bool invoked = false;
        _ircHandler.OnRoomstateReceived += (_, rmst) =>
        {
            invoked = true;
            Assert.AreEqual(false, rmst.EmoteOnly);
            Assert.AreEqual(-1, rmst.FollowersOnly);
            Assert.AreEqual(false, rmst.R9K);
            Assert.AreEqual(87633910, rmst.ChannelId);
            Assert.AreEqual(0, rmst.SlowMode);
            Assert.AreEqual(false, rmst.SubsOnly);
            Assert.AreEqual("strbhlfe", rmst.Channel);
        };
        _ircHandler.Handle(_messages[1]);
        Assert.IsTrue(invoked);
    }

    [TestMethod]
    public void RoomstateTestAllOn()
    {
        bool invoked = false;
        _ircHandler.OnRoomstateReceived += (_, rmst) =>
        {
            invoked = true;
            Assert.AreEqual(true, rmst.EmoteOnly);
            Assert.AreEqual(15, rmst.FollowersOnly);
            Assert.AreEqual(true, rmst.R9K);
            Assert.AreEqual(87633910, rmst.ChannelId);
            Assert.AreEqual(10, rmst.SlowMode);
            Assert.AreEqual(true, rmst.SubsOnly);
            Assert.AreEqual("strbhlfe", rmst.Channel);
        };
        _ircHandler.Handle(_messages[2]);
        Assert.IsTrue(invoked);
    }

    [TestMethod]
    public void JoinTest()
    {
        bool invoked = false;
        _ircHandler.OnJoinedChannel += (_, jm) =>
        {
            invoked = true;
            Assert.AreEqual("strbhlfe", jm.Username);
            Assert.AreEqual("lbnshlfe", jm.Channel);
        };
        _ircHandler.Handle(_messages[3]);
        Assert.IsTrue(invoked);
    }

    [TestMethod]
    public void PartTest()
    {
        bool invoked = false;
        _ircHandler.OnLeftChannel += (_, pm) =>
        {
            invoked = true;
            Assert.AreEqual("strbhlfe", pm.Username);
            Assert.AreEqual("lbnshlfe", pm.Channel);
        };
        _ircHandler.Handle(_messages[4]);
        Assert.IsTrue(invoked);
    }
}
