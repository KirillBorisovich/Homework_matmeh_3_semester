// <copyright file="UserInterface.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace ParallelMatrixMultiplication;

public static class UserInterface
{
    public static void MultiplyMatricesFromAFile()
    {
        var path1 = GetValidInput("Enter the path to the first matrix");
        var path2 = GetValidInput("Enter the path to the second matrix");
        var pathForResult = GetValidInput("Enter the path to save the result");
        var matrix1 = Matrix.ReadFromFile(path1);
        var matrix2 = Matrix.ReadFromFile(path2);
        var result = Matrix.ParallelMultiplication(matrix1, matrix2);
        Matrix.SaveToFile(pathForResult, result);
    }

    public static void CarryingOutMeasurementsOnTestMatrices()
    {
        
    }

    private static string GetValidInput(string prompt)
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
}