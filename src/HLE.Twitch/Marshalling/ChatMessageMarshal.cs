using System;
using System.Text;
using HLE.Twitch.Tmi.Models;

namespace HLE.Twitch.Marshalling;

public static class ChatMessageMarshal
{
    public static int CopyUsernameTo(IChatMessage chatMessage, Span<char> destination)
    {
        if (chatMessage is not MemoryEfficientChatMessage memoryEfficientChatMessage || memoryEfficientChatMessage._usernameBuffer is null)
        {
            string username = chatMessage.Username;
            username.CopyTo(destination);
            return username.Length;
        }

        byte[] usernameBuffer = memoryEfficientChatMessage._usernameBuffer;
        lock (usernameBuffer)
        {
            ReadOnlySpan<byte> byteSpan = usernameBuffer.AsSpan(0, memoryEfficientChatMessage._nameLength);
            return Encoding.ASCII.GetChars(byteSpan, destination);
        }
    }

    public static int CopyMessageTo(IChatMessage chatMessage, Span<char> destination)
    {
        if (chatMessage is not MemoryEfficientChatMessage memoryEfficientChatMessage || memoryEfficientChatMessage._messageBuffer is null)
        {
            string message = chatMessage.Message;
            message.CopyTo(destination);
            return message.Length;
        }

        byte[] messageBuffer = memoryEfficientChatMessage._messageBuffer!;
        lock (messageBuffer)
        {
            ReadOnlySpan<byte> byteSpan = messageBuffer.AsSpan(0, memoryEfficientChatMessage._messageLength);
            return Encoding.ASCII.GetChars(byteSpan, destination);
        }
    }
}
