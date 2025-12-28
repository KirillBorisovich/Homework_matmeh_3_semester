// <copyright file="OnlineChat.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace Test_23_12_2025;

using System.Net;
using System.Net.Sockets;

/// <summary>
/// Online Chat.
/// </summary>
public class OnlineChat(Stream outputStream)
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private readonly StreamReader outputReader = new StreamReader(outputStream);
    private readonly StreamWriter outputWriter = new StreamWriter(outputStream) { AutoFlush = true };

    /// <summary>
    /// Start a server for network chat.
    /// </summary>
    /// <param name="port">The port where connections are accepted.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task StartServer(int port)
    {
        using var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        var client = await listener.AcceptTcpClientAsync();
        await this.CommunicatingWithTheFlow(client);
    }

    /// <summary>
    /// Start a server for network chat.
    /// </summary>
    /// <param name="serverIp">Server IP.</param>
    /// <param name="serverPort">Server port.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task StartClient(string serverIp, int serverPort)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Parse(serverIp), serverPort);
        await this.CommunicatingWithTheFlow(client);
    }

    /// <summary>
    /// Stop the server.
    /// </summary>
    public void Stop()
    {
        this.cts.Cancel();
    }

    private async Task CommunicatingWithTheFlow(TcpClient client)
    {
        this.Writer(client.GetStream());
        this.Reader(client.GetStream());
        await Task.Delay(-1, this.cts.Token);
    }

    private void Writer(NetworkStream stream)
    {
        Task.Run(async () =>
        {
            var writer = new StreamWriter(stream) { AutoFlush = true };
            while (!this.cts.IsCancellationRequested)
            {
                await this.outputWriter.WriteLineAsync(">");
                var data = await this.outputReader.ReadLineAsync();
                await writer.WriteAsync(data + "\n");
            }
        });
    }

    private void Reader(NetworkStream stream)
    {
        Task.Run(async () =>
        {
            var reader = new StreamReader(stream);
            while (!this.cts.IsCancellationRequested)
            {
                var data = await reader.ReadLineAsync(this.cts.Token);
                if (data == null || this.cts.IsCancellationRequested)
                {
                    await this.outputWriter.WriteLineAsync("The connection is closed.");
                    break;
                }

                await this.outputWriter.WriteLineAsync($"[{DateTime.Now:HH:mm:ss}] Client: {data}");

                if (data.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    await this.outputWriter.WriteLineAsync("The client requested a shutdown.");
                    break;
                }
            }
        });
    }
}