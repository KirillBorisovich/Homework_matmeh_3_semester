namespace ParallelMatrixMultiplication;

/// <summary>
/// A class of methods for working with matrices.
/// </summary>
public static class Matrix
{
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
    public static int[,] MatrixMultiplication(int[,] matrix1, int[,] matrix2)
    {
        if (matrix1.GetLength(1) != matrix2.GetLength(0))
        {
            throw new IncompatibleMatrixSizesException();
        }

        var result = new int[matrix1.GetLength(0), matrix2.GetLength(1)];
        var matrix2T = Transpose(matrix2);

        MultiplicationKernel(matrix1, matrix2T, result,
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
    public static int[,] ParallelMatrixMultiplication(int[,] matrix1, int[,] matrix2)
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