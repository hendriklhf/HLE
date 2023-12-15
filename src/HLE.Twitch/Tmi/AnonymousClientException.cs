using System;

namespace HLE.Twitch.Tmi;

public sealed class AnonymousClientException() : Exception("The client is connected anonymously, therefore cannot send messages.");
