using System;

namespace HLE.Twitch.Tmi;

public sealed class ClientNotConnectedException() : Exception("The client is not connected.");
