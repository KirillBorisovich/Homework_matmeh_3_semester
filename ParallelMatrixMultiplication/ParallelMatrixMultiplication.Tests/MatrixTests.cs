// <copyright file="MatrixTests.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace ParallelMatrixMultiplication.Tests;

public class MatrixTests
{
    private readonly int[,] matrix1 = new int[,]
    {
        { 1, 2, 3, 4, 5 },
        { 6, 7, 8, 9, 10 },
        { 11, 12, 13, 14, 15 },
    };

    private readonly int[,] matrix2 = new[,]
    {
        { 1, 2 },
        { 3, 4 },
        { 5, 6 },
        { 7, 8 },
        { 9, 10 },
    };

    private readonly int[,] expectedResult = new[,]
    {
        { 95, 110 },
        { 220, 260 },
        { 345, 410 },
    };

    [Test]
    public void TheMatricesAreEqualTest()
    {
        var localMatrix1 = new[,]
        {
            { 1, 2, 3 },
            { 4, 5, 6 },
        };
        var localMatrix2 = new[,]
        {
            { 1, 2, 3 },
            { 4, 5, 6 },
            { 7, 8, 9 },
        };
        var localMatrix3 = new[,]
        {
            { 1, 2, 3 },
            { 4, 5, 6 },
            { 7, 8, 9 },
        };

        Assert.Multiple(() =>
        {
            Assert.That(Matrix.Equals(localMatrix1, localMatrix1), Is.True);
            Assert.That(Matrix.Equals(localMatrix1, localMatrix2), Is.False);
            Assert.That(Matrix.Equals(localMatrix2, localMatrix3), Is.True);
            Assert.That(Matrix.Equals(localMatrix3, localMatrix1), Is.False);
        });
    }

    [Test]
    public void ReadFromFileTest()
    {
        var inputMatrix1 = Matrix.ReadFromFile("../../../TestInput1.txt");
        var inputMatrix2 = Matrix.ReadFromFile("../../../TestInput2.txt");
        var sample1 = new[,]
        {
            { 1, 2, 3, 4, 5 },
            { 6, 7, 8, 9, 10 },
            { 11, 12, 13, 14, 15 },
        };
        var sample2 = new[,]
        {
            { 1, 2 },
            { 3, 4 },
            { 5, 6 },
            { 7, 8 },
            { 9, 10 },
        };

        Assert.Multiple(() =>
        {
            Assert.That(Matrix.Equals(inputMatrix1, sample1), Is.True);
            Assert.That(Matrix.Equals(inputMatrix2, sample2), Is.True);
        });
    }

    [Test]
    public void ReadFromFileShouldThrowEmptyFileExceptionIfFileIsEmpty()
    {
        Assert.Throws<EmptyFileException>(
            () => Matrix.ReadFromFile("../../../TestInputFailed1.txt"));
    }

    [Test]
    public void ReadFromFileShouldThrowInvalidDataExceptionIfIncorrectMatrixStructure()
    {
        Assert.Throws<InvalidDataException>(
            () => Matrix.ReadFromFile("../../../TestInputFailed2.txt"));
    }

    [Test]
    public void MultiplicationTest()
    {
        var result = Matrix.Multiply(this.matrix1, this.matrix2);

        Assert.That(Matrix.Equals(result, this.expectedResult), Is.True);
    }

    [Test]
    public void MultiplicationShouldThrowIncompatibleMatrixSizesExceptionIfIncompatibleMatrixSizes()
    {
        Assert.Throws<IncompatibleMatrixSizesException>(
            () => Matrix.Multiply(this.matrix1, this.expectedResult));
    }

    [Test]
    public void ParallelMultiplicationTest()
    {
        var result = Matrix.ParallelMultiplication(this.matrix1, this.matrix2);

        Assert.That(Matrix.Equals(result, this.expectedResult), Is.True);
    }

    [Test]
    public void ParallelMultiplicationShouldThrowIncompatibleMatrixSizesExceptionIfIncompatibleMatrixSizes()
    {
        var sample = new[,]
        {
            { 95, 110 },
            { 220, 260 },
            { 345, 410 },
        };
        Assert.Throws<IncompatibleMatrixSizesException>(
            () => Matrix.ParallelMultiplication(this.matrix1, sample));
    }

    [Test]
    public void SaveToFileTest()
    {
        var result = Matrix.ParallelMultiplication(this.matrix1, this.matrix2);
        Matrix.SaveToFile("../../../TestResultInput.txt", result);
        var input = Matrix.ReadFromFile("../../../TestResultInput.txt");
        Assert.That(Matrix.Equals(input, result), Is.True);
    }

    [Test]
    public void SaveToFileShouldThrowArgumentExceptionIfAnEmptyPath()
    {
        Assert.Throws<ArgumentException>(
            () => Matrix.SaveToFile(string.Empty, Matrix.ParallelMultiplication(this.matrix1, this.matrix2)));
    }

    [Test]
    public void GenerateTest()
    {
        var matrix = Matrix.Generate(10, 5);
        Assert.Multiple(() =>
        {
            Assert.That(matrix.GetLength(0), Is.EqualTo(10));
            Assert.That(matrix.GetLength(1), Is.EqualTo(5));
        });
    }
}