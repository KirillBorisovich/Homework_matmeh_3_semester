namespace ParallelMatrixMultiplication;

/// <summary>
/// A set of methods for working with matrices.
/// </summary>
public static class Matrix
{
    /// <summary>
    /// Reading from file.
    /// </summary>
    /// <param name="path">Path to the file.</param>
    /// <returns>Two-dimensional array.</returns>
    /// <exception cref="EmptyFileException">Empty file exception.</exception>
    public static int[,] ReadFromFile(string path)
    {
        using var file = new StreamReader(path);

        var lines = new List<int[]>();
        var numberOfNumbersInARow = 0;
        while (file.ReadLine() is { } line)
        {
            var matrixString = Array.ConvertAll(line.Split(), int.Parse);
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
}