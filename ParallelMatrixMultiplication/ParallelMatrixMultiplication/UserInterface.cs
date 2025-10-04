// <copyright file="UserInterface.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace ParallelMatrixMultiplication;

using System.Diagnostics;

/// <summary>
/// User interface.
/// </summary>
public static class UserInterface
{
    /// <summary>
    /// Multiplying matrices from files.
    /// </summary>
    public static void MultiplyMatricesFromAFile()
    {
        var path1 = GetValidInput("Enter the path to the first matrix");
        var path2 = GetValidInput("Enter the path to the second matrix");
        var pathForResult = GetValidInput("Enter the path to save the result");
        var matrix1 = Matrix.ReadFromFile(path1);
        var matrix2 = Matrix.ReadFromFile(path2);
        var result = Matrix.ParallelMultiplication(matrix1, matrix2);
        Matrix.SaveToFile(pathForResult, result);
        Console.WriteLine("\nSuccessfully!");
    }

    /// <summary>
    /// Make measurements using test matrices.
    /// </summary>
    public static void MakeOutMeasurementsOnTestMatrices()
    {
        Console.WriteLine("\nStart of measurements");
        var sizes = new[] { 100, 500, 1000 };
        foreach (var size in sizes)
        {
            var matrix1 = Matrix.Generate(size, size);
            var matrix2 = Matrix.Generate(size, size);

            var time1 = MeasureTime(
                () => { Matrix.Multiplication(matrix1, matrix2); });
            var time2 = MeasureTime(
                () => { Matrix.ParallelMultiplication(matrix1, matrix2); });
            Console.WriteLine($"---------------\n" +
                              $"Size of the first matrix: {size}x{size}\n" +
                              $"Size of the second matrix: {size}x{size}\n" +
                              $"Time of successive multiplication: {time1.AverageTime,8:F2} ± {time1.StandardDeviation:F2} ms\n" +
                              $"Parallel multiplication time: {time2.AverageTime,8:F2} ± {time2.StandardDeviation:F2} ms\n" +
                              $"Acceleration: {time1.AverageTime / time2.AverageTime,8:F2}");
        }

        return;

        (double AverageTime, double StandardDeviation) MeasureTime(Action action)
        {
            action();

            const int iterations = 10;
            var totalTime = new List<double>();

            for (var i = 0; i < iterations; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                var watch = Stopwatch.StartNew();
                action();
                watch.Stop();

                totalTime.Add(watch.ElapsedMilliseconds);
            }

            var averageTime = totalTime.Sum() / iterations;
            var sumOfSquaredDeviations = totalTime.Sum(item => Math.Pow(item - averageTime, 2));
            var standardDeviation = Math.Sqrt(sumOfSquaredDeviations / totalTime.Count);

            return (averageTime, standardDeviation);
        }
    }

    private static string GetValidInput(string prompt)
    {
        string? input;
        do
        {
            Console.WriteLine("\n" + prompt);
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