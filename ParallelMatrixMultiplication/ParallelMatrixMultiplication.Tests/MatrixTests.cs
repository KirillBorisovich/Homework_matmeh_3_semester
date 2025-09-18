namespace ParallelMatrixMultiplication.Tests;

public class MatrixTests
{
    private readonly int[,] matrix1 = Matrix.ReadFromFile("../../../TestInput1.txt");
    private readonly int[,] matrix2 = Matrix.ReadFromFile("../../../TestInput2.txt");
    private int[,] resultOfParallelMultiplication = Matrix.ParallelMultiplication(matrix1, matrix2);

    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TheMatricesAreEqualTest()
    {
        var matrix1 = new int[,]
        {
            { 1, 2, 3 },
            { 4, 5, 6 },
        };
        var matrix2 = new int[,]
        {
            { 1, 2, 3 },
            { 4, 5, 6 },
            { 7, 8, 9 },
        };
        var matrix3 = new int[,]
        {
            { 1, 2, 3 },
            { 4, 5, 6 },
            { 7, 8, 9 },
        };

        Assert.That(Matrix.Equals(matrix1, matrix1), Is.True);
        Assert.That(Matrix.Equals(matrix1, matrix2), Is.False);
        Assert.That(Matrix.Equals(matrix2, matrix3), Is.True);
        Assert.That(Matrix.Equals(matrix3, matrix1), Is.False);
    }

    [Test]
    public void ReadFromFileTest()
    {
        var sample1 = new int[,]
        {
            { 1, 2, 3, 4, 5 },
            { 6, 7, 8, 9, 10 },
            { 11, 12, 13, 14, 15 },
        };
        var sample2 = new int[,]
        {
            { 1, 2 },
            { 3, 4 },
            { 5, 6 },
            { 7, 8 },
            { 9, 10 },
        };
        var testMatrix1 = Matrix.ReadFromFile("../../../TestInput1.txt");
        var testMatrix2 = Matrix.ReadFromFile("../../../TestInput2.txt");
        Assert.Multiple(() =>
        {
            Assert.That(Matrix.Equals(testMatrix1, sample1), Is.True);
            Assert.That(Matrix.Equals(testMatrix2, sample2), Is.True);
        });
    }

    [Test]
    public void ReadFromFileShouldThrowEmptyFileExceptionIfFileIsEmpty()
    {
        Assert.Throws<EmptyFileException>(
            () => Matrix.ReadFromFile("../../../TestInputFailed1.txt"));
    }

    [Test]
    public void ReadFromFileShouldThrowInvalidDataExceptionIfIncorrectMatrixstructure()
    {
        Assert.Throws<InvalidDataException>(
            () => Matrix.ReadFromFile("../../../TestInputFailed2.txt"));
    }
}