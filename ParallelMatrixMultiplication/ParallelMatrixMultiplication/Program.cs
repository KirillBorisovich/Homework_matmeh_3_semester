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
            break;
        case "2":
            UserInterface.MakeOutMeasurementsOnTestMatrices();
            break;
        case "0":
            break;
        default:
            Console.WriteLine("Unknown number of operations");
            break;
    }
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"File '{ex.FileName}' not found");
}
catch (DirectoryNotFoundException ex)
{
    Console.WriteLine($"Directory not found: {ex.Message}");
}
catch (UnauthorizedAccessException)
{
    Console.WriteLine("No permission to read file");
}
catch (InvalidDataException)
{
    Console.WriteLine("Incorrect data format");
}
catch (EmptyFileException)
{
    Console.WriteLine("The file is empty");
}
catch (IncompatibleMatrixSizesException)
{
    Console.WriteLine("Matrix multiplication cannot be performed:" +
                      "dimensionality mismatch.");
}
catch (ArgumentException)
{
    Console.WriteLine("File path cannot be null or empty");
}