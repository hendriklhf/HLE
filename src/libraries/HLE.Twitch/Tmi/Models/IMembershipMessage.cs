namespace HLE.Twitch.Tmi.Models;

public interface IMembershipMessage<out T>
{
    string Username { get; }

    string Channel { get; }

    static abstract T Create(string username, string channel);
}
