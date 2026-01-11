// <copyright file="Program.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

using MyNUnit;

if (args.Length != 1)
{
    Console.WriteLine("Usage: program <the path to the file or directory>\n");
    return;
}

var inputPath = args[0];

try
{
    var result = await new MyNUnitClass().RunAllTheTestsAlongThisPath(inputPath);
    Console.WriteLine(string.Concat(result));
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message + "\n");
}