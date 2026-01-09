// <copyright file="ServerClientInteractionTests.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

using System.Security.Cryptography;

namespace SimpleFTP.Tests;

public class ServerClientInteractionTests
{
    private const int Port = 65000;
    private readonly CancellationTokenSource cts = new();
    private Server server;
    private Client client;

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        this.server = new Server(Port);
        _ = this.server.Start();
        this.client = new Client();
        await this.client.ConnectAsync("localhost", Port, this.cts.Token);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        this.client.Dispose();
        this.server.Stop();
        this.server.Dispose();
    }

    [Test]
    public async Task ClosedConnectionTest()
    {
        using var localServer = new Server(Port + 1);
        _ = localServer.Start();
        using var localClient = new Client();
        _ = localClient.ConnectAsync("localhost", Port + 1, this.cts.Token);
        localServer.Stop();
        await Task.Delay(50);
        await Assert.MultipleAsync(async() =>
        {
            await Assert.ThatAsync(async () => await localClient.ListAsync("./", this.cts.Token),
                Throws.InstanceOf<IOException>()
                    .Or.InstanceOf<InvalidOperationException>());
            Assert.ThrowsAsync<InvalidOperationException>(async () => await localClient.Get("./File", "./"));
        });
    }

    [Test]
    public async Task ListForAnExistingDirectoryTest()
    {
        var data = await this.client.ListAsync("./", this.cts.Token);
        Assert.That(data.Split(), Has.Length.GreaterThanOrEqualTo(3));
    }

    [Test]
    public void ListAndGetForAnNonExistingDirectoryTest()
    {
        Assert.Multiple(() =>
        {
            Assert.ThrowsAsync<FileNotFoundException>(async () =>
                await this.client.ListAsync("./weqrqwrwqer", this.cts.Token));
            Assert.ThrowsAsync<FileNotFoundException>(async () =>
                await this.client.Get("./weqrqwrwqer", "./"));
        });
    }

    [Test]
    public async Task ListForFile()
    {
        var data1 = await this.client.ListAsync("./",  this.cts.Token);
        var result = data1.Split();
        var indexForFile = 0;
        for (var i = 0; i < result.Length; i++)
        {
            if (result[i] == "false")
            {
                indexForFile = i;
                break;
            }
        }

        Assert.ThrowsAsync<FileNotFoundException>(async () =>
            await this.client.ListAsync(result[indexForFile], this.cts.Token));
    }

    [Test]
    public async Task GetForFileTest()
    {
        var array = (await this.client.ListAsync("./", this.cts.Token)).Split();
        var pathForServer = array[Array.IndexOf(array, "false") - 1];
        Directory.CreateDirectory("TestDirectory");
        await this.client.Get(pathForServer, Directory.GetCurrentDirectory() + "/TestDirectory/");

        using var sha256 = SHA256.Create();

        var stream1 = File.OpenRead(Directory.GetCurrentDirectory() + '/' + Path.GetFileName(pathForServer));
        var stream2 = File.OpenRead(Directory.GetCurrentDirectory() + "/TestDirectory/" + Path.GetFileName(pathForServer));

        var hash1 = await sha256.ComputeHashAsync(stream1);
        var hash2 = await sha256.ComputeHashAsync(stream2);

        await stream1.DisposeAsync();
        await stream2.DisposeAsync();
        Directory.Delete("TestDirectory", true);

        Assert.That(hash1.SequenceEqual(hash2), Is.True);
    }
}