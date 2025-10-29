// <copyright file="Program.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

using SimpleFTP;

var server = new Server(1506);
_ = server.Start();
var client = new Client("localhost", 1506);
/*Console.WriteLine(client.List("./").Result);*/
await client.Get("/SimpleFTP.runtimeconfig.json", "/Users/kirillbenga/Downloads/");
