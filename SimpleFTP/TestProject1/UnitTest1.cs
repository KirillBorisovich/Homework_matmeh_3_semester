// <copyright file="UnitTest1.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace TestProject1;

using SimpleFTP;

public class Tests
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

    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task ListForAnExistingDirectoryTest()
    {
        var data = await this.client.List("./");
        Assert.That(data.Split(), Has.Length.GreaterThanOrEqualTo(3));
    }

    [Test]
    public void ListForAnNonExistingDirectoryTest()
    {
        Assert.ThrowsAsync<FileNotFoundException>(async () =>
            await this.client.List("./weqrqwrwqer"));
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
}