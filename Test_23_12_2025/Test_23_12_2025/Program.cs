// <copyright file="Program.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

using Test_23_12_2025;

Console.WriteLine("Online chat");

try
{
    using var consoleStream = Console.OpenStandardInput();
    var chat = new OnlineChat(consoleStream);
    switch (args.Length)
    {
        case 1:
            _ = chat.StartServer(int.Parse(args[0]));
            break;
        case 2:
            _ = chat.StartClient(args[0], int.Parse(args[1]));
            break;
        default:
            Console.WriteLine("Incorrect input of arguments");
            break;
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message + "\n");
}