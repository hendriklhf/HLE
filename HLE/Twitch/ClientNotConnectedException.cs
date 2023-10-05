using System;

namespace HLE.Twitch;

public sealed class ClientNotConnectedException() : Exception("The client is not connected.");
