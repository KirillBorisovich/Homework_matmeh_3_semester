// <copyright file="Program.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

using ParallelMatrixMultiplication;

Console.WriteLine("Parallel matrix multiplication\n");
Console.WriteLine(
    "Enter a number for further action:\n" +
    "1. Multiply matrices from file\n" +
    "2. Generate test matrices and take measurements\n" +
    "0. Exit\n");
Console.WriteLine("Enter number:");
try
{
    switch (Console.ReadLine())
    {
        case "1":
            UserInterface.MultiplyMatricesFromAFile();
            return 0;
        case "2":
            UserInterface.MakeOutMeasurementsOnTestMatrices();
            return 0;
        case "0":
            return 0;
        default:
            Console.WriteLine("Unknown number of operations");
            return 0;
    }
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"File '{ex.FileName}' not found");
    return 0;
}
catch (DirectoryNotFoundException ex)
{
    Console.WriteLine($"Directory not found: {ex.Message}");
    return 0;
}
catch (UnauthorizedAccessException)
{
    Console.WriteLine("No permission to read file");
    return 0;
}
catch (InvalidDataException)
{
    Console.WriteLine("Incorrect data format");
    return 0;
}
catch (EmptyFileException)
{
    Console.WriteLine("The file is empty");
    return 0;
}
catch (IncompatibleMatrixSizesException)
{
    Console.WriteLine("Matrix multiplication cannot be performed:" +
                      "dimensionality mismatch.");
    return 0;
}
catch (ArgumentException)
{
    Console.WriteLine("File path cannot be null or empty");
    return 0;
}