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

        var colsMatrix1 = matrix1.GetLength(1);
        var rowsMatrix2 = matrix2.GetLength(0);
        var result = new int[matrix1.GetLength(0), matrix2.GetLength(1)];

        for (var i = 0; i < result.GetLength(0); i++)
        {
            for (var j = 0; j < result.GetLength(1); j++)
            {
                var sum = 0;
                for (var k = 0; k < colsMatrix1; k++)
                {
                    sum += matrix1[i, k] * matrix2[k, j];
                }

                result[i, j] = sum;
            }
        }

        return result;
    }
}