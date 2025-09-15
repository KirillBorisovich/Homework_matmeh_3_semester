namespace ParallelMatrixMultiplication;

public class Matrix
{
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
            throw new InvalidDataException();
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