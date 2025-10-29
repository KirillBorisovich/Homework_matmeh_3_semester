// <copyright file="ServerClientInteractionTests.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace TestProject1;

using System.Security.Cryptography;
using SimpleFTP;

public class ServerClientInteractionTests
{
    private const int Port = 65000;
    private Server server;
    private Client client;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        this.server = new Server(Port);
        _ = this.server.Start();
        this.client = new Client("localhost", Port);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        this.client.Dispose();
        this.server.Stop();
    }

    [Test]
    public void ClosedConnectionTest()
    {
        var localServer = new Server(Port + 1);
        _ = localServer.Start();
        using var localClient = new Client("localhost", Port + 1);
        localServer.Stop();
        Assert.Multiple(() =>
        {
            Assert.ThrowsAsync<IOException>(async () => await localClient.List("./"));
            Assert.ThrowsAsync<IOException>(async () => await localClient.Get("./File", "./"));
        });
    }

    [Test]
    public async Task ListForAnExistingDirectoryTest()
    {
        var data = await this.client.List("./");
        Assert.That(data.Split(), Has.Length.GreaterThanOrEqualTo(3));
    }

    [Test]
    public void ListAndGetForAnNonExistingDirectoryTest()
    {
        Assert.Multiple(() =>
        {
            Assert.ThrowsAsync<FileNotFoundException>(async () =>
                await this.client.List("./weqrqwrwqer"));
            Assert.ThrowsAsync<FileNotFoundException>(async () =>
                await this.client.Get("./weqrqwrwqer", "./"));
        });
    }

    [Test]
    public async Task ListForFile()
    {
        var data1 = await this.client.List("./");
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
            await this.client.List(result[indexForFile]));
    }

    [Test]
    public async Task GetForFileTest()
    {
        var array = (await this.client.List("./")).Split();
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