// <copyright file="Program.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

using ParallelMatrixMultiplication;

Console.WriteLine("Parallel matrix multiplication/n");
var path1 = GetValidInput("Enter the path to the first matrix");
var path2 = GetValidInput("Enter the path to the second matrix");
var pathForResult = GetValidInput("Enter the path to save the result");

try
{
    var matrix1 = Matrix.ReadFromFile(path1);
    var matrix2 = Matrix.ReadFromFile(path2);
    var result = Matrix.ParallelMultiplication(matrix1, matrix2);
    Matrix.SaveToFile(pathForResult, result);
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


return;

string GetValidInput(string prompt)
{
    string? input;
    do
    {
        Console.Write(prompt);
        input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("Path cannot be empty. Please try again.");
        }
    }
    while (string.IsNullOrWhiteSpace(input));

    return input.Trim();
}