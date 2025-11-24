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
    Console.WriteLine(result);
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message + "\n");
    return;
}

/*using MyNUnit;

var qwe = await new MyNUnitClass().RunAllTheTestsAlongThisPath(
    "/Users/kirillbenga/Образование/Homework_matmeh_3_semester/MyNUnit/TestsAssemblies");*/