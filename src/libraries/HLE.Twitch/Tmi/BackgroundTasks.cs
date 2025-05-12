using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using HLE.Memory;

namespace HLE.Twitch.Tmi;

internal static class BackgroundTasks
{
    private static ReadOnlySpan<byte> PongPrefix => "PONG :"u8;

    public static async Task ReadWebsocketBytesAsync(TwitchClient client, CancellationToken stoppingToken)
    {
        do
        {
            using Bytes bytes = await client._client.ReceiveAsync(stoppingToken).ConfigureAwait(false);
            client._ircHandler.Handle(bytes.AsSpan());
        }
        while (!stoppingToken.IsCancellationRequested);
    }

    public static async Task ReadPingsAsync(WebSocketIrcClient client, ChannelReader<Bytes> pingReader, CancellationToken stoppingToken)
    {
        do
        {
            Bytes message = await pingReader.ReadAsync(stoppingToken).ConfigureAwait(false);

            byte[] buffer = ArrayPool<byte>.Shared.Rent(PongPrefix.Length + message.Length);
            UnsafeBufferWriter<byte> builder = new(buffer);
            builder.Write(PongPrefix);
            builder.Write(message.AsSpan());
            message.Dispose();

            Bytes pong = Bytes.AsBytes(buffer, builder.Count);
            await client.SendRawAsync(pong).ConfigureAwait(false);
        }
        while (!stoppingToken.IsCancellationRequested);
    }

    public static async Task ReadRoomstatesAsync(ChannelList channels, ChannelReader<Roomstate> roomstateReader, ChannelWriter<Roomstate>? publicRoomstateWriter, CancellationToken stoppingToken)
    {
        do
        {
            Roomstate roomstate = await roomstateReader.ReadAsync(stoppingToken).ConfigureAwait(false);
            channels.Update(ref roomstate);

            if (publicRoomstateWriter is not null)
            {
                await publicRoomstateWriter.WriteAsync(roomstate, stoppingToken).ConfigureAwait(false);
            }
        }
        while (!stoppingToken.IsCancellationRequested);
    }
}
