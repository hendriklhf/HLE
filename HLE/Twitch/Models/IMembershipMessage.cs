namespace HLE.Twitch.Models;

public interface IMembershipMessage<out T>
{
    string Username { get; }

    string Channel { get; }

    static abstract T Create(string username, string channel);
}
