// <copyright file="Matrix.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace ParallelMatrixMultiplication;

/// <summary>
/// A class of methods for working with matrices.
/// </summary>
public static class Matrix
{
    private static readonly Random Rand = new();

    /// <summary>
    /// Reading from file.
    /// </summary>
    /// <param name="path">Path to the file.</param>
    /// <returns>Matrix as a two-dimensional array.</returns>
    /// <exception cref="EmptyFileException">Empty file exception.</exception>
    public static int[,] ReadFromFile(string path)
    {
        using var file = new StreamReader(path);

        var lines = new List<int[]>();
        var numberOfNumbersInARow = 0;
        while (file.ReadLine() is { } line)
        {
            var matrixString = Array.ConvertAll(
                line.Split([' '], StringSplitOptions.RemoveEmptyEntries),
                int.Parse);
            if (matrixString.Length != numberOfNumbersInARow &&
                numberOfNumbersInARow != 0)
            {
                throw new InvalidDataException();
            }

            if (numberOfNumbersInARow == 0)
            {
                numberOfNumbersInARow = matrixString.Length;
            }

            lines.Add(matrixString);
        }

        if (lines.Count == 0)
        {
            throw new EmptyFileException();
        }

        var matrix = new int[lines.Count,  numberOfNumbersInARow];
        for (var i = 0; i < lines.Count; i++)
        {
            for (var j = 0; j < numberOfNumbersInARow; j++)
            {
                matrix[i, j] = lines[i][j];
            }
        }

        return matrix;
    }

    /// <summary>
    /// Multiply matrices.
    /// </summary>
    /// <param name="matrix1">First matrix to multiply.</param>
    /// <param name="matrix2">Second matrix to multiply.</param>
    /// <returns>The result of multiplication.</returns>
    /// <exception cref="IncompatibleMatrixSizesException">Exception about
    /// incompatibility of matrix sizes for multiplication.</exception>
    public static int[,] Multiplication(int[,] matrix1, int[,] matrix2)
    {
        if (matrix1.GetLength(1) != matrix2.GetLength(0))
        {
            throw new IncompatibleMatrixSizesException();
        }

        var result = new int[matrix1.GetLength(0), matrix2.GetLength(1)];
        var matrix2T = Transpose(matrix2);

        MultiplicationKernel(
            matrix1,
            matrix2T,
            result,
            0,
            result.GetLength(1));

        return result;
    }

    /// <summary>
    /// Multiply matrices in parallel.
    /// </summary>
    /// <param name="matrix1">First matrix to multiply.</param>
    /// <param name="matrix2">Second matrix to multiply.</param>
    /// <returns>The result of multiplication.</returns>
    /// <exception cref="IncompatibleMatrixSizesException">Exception about
    /// incompatibility of matrix sizes for multiplication.</exception>
    public static int[,] ParallelMultiplication(int[,] matrix1, int[,] matrix2)
    {
        if (matrix1.GetLength(1) != matrix2.GetLength(0))
        {
            throw new IncompatibleMatrixSizesException();
        }

        var result = new int[matrix1.GetLength(0), matrix2.GetLength(1)];
        var processorCount = Environment.ProcessorCount;
        var threads = processorCount <= matrix1.GetLength(0)
            ? new Thread[processorCount]
            : new Thread[matrix1.GetLength(0)];
        var numberOfLinesForTheStream = matrix1.GetLength(0) / threads.Length;

        var matrix2T = Transpose(matrix2);

        for (var i = 0; i < threads.Length; i++)
        {
            var startRow = numberOfLinesForTheStream * i;
            var endRow = threads.Length - 1 == i ?
                matrix1.GetLength(0) :
                (i + 1) * numberOfLinesForTheStream;
            threads[i] = new Thread(() =>
                MultiplicationKernel(matrix1,  matrix2T, result, startRow, endRow));
        }

        foreach (var thread in threads)
        {
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        return result;
    }

    /// <summary>
    /// Save matrix to file.
    /// </summary>
    /// <param name="path">Path to file.</param>
    /// <param name="matrix">The matrix that needs to be saved.</param>
    public static void SaveToFile(string path, int[,] matrix)
    {
        ArgumentNullException.ThrowIfNull(matrix);

        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException("File path cannot be null or empty");
        }

        using var writer = new StreamWriter(path);
        for (var i = 0; i < matrix.GetLength(0); i++)
        {
            for (var j = 0; j < matrix.GetLength(1); j++)
            {
                writer.Write(matrix[i, j]);
                if (j < matrix.GetLength(1) - 1)
                {
                    writer.Write(" ");
                }
            }

            writer.WriteLine();
        }
    }

    /// <summary>
    /// Generate a random matrix with given dimensions.
    /// </summary>
    /// <param name="rows">Number of rows.</param>
    /// <param name="columns">Number columns.</param>
    /// <returns>A random matrix in the form of a two-dimensional array.</returns>
    /// <exception cref="ArgumentException">Row and Column count must be greater than zero.</exception>
    public static int[][,] Generate(int rows, int columns)
    {
        if (rows <= 0 || columns <= 0)
        {
            throw new ArgumentException("Row and Column count must " +
                                        "be greater than zero");
        }

        var matrices = new int[6][,];
        var threads = new Thread[6];
        for (var t = 0; t < threads.Length; t++)
        {
            var t1 = t;
            threads[t] = new Thread(() =>
            {
                matrices[t1] = new int[rows, columns];
                for (var i = 0; i < rows; i++)
                {
                    for (var j = 0; j < columns; j++)
                    {
                        matrices[t1][i, j] = Rand.Next(int.MinValue, int.MaxValue);
                    }
                }
            });
        }

        foreach (var thread in threads)
        {
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        return matrices;
    }

    private static void MultiplicationKernel(int[,] matrix1, int[,] matrix2T, int[,] result, int startRow, int endRow)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(startRow);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(endRow, matrix1.GetLength(0));

        for (var i = startRow; i < endRow; i++)
        {
            for (var j = 0; j < result.GetLength(1); j++)
            {
                var sum = 0;
                for (var k = 0; k < matrix1.GetLength(1); k++)
                {
                    sum += matrix1[i, k] * matrix2T[j, k];
                }

                result[i, j] = sum;
            }
        }
    }

    private static int[,] Transpose(int[,] matrix)
    {
        var rows = matrix.GetLength(0);
        var cols = matrix.GetLength(1);

        var result = new int[cols, rows];

        for (var i = 0; i < rows; i++)
        {
            for (var j = 0; j < cols; j++)
            {
                result[j, i] = matrix[i, j];
            }
        }

        return result;
    }
}