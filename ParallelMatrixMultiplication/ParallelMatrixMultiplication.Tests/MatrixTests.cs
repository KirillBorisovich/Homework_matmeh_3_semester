// <copyright file="MatrixTests.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace ParallelMatrixMultiplication.Tests;

public class MatrixTests
{
    private readonly int[,] matrix1;
    private readonly int[,] matrix2;
    private readonly int[,] resultOfParallelMultiplication;
    private readonly int[,] resultOfSequentialMultiplication;

    public MatrixTests()
    {
        this.matrix1 = Matrix.ReadFromFile("../../../TestInput1.txt");
        this.matrix2 = Matrix.ReadFromFile("../../../TestInput2.txt");
        this.resultOfParallelMultiplication = Matrix.ParallelMultiplication(this.matrix1, this.matrix2);
        this.resultOfSequentialMultiplication = Matrix.Multiplication(this.matrix1, this.matrix2);
    }

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
            Assert.That(Matrix.Equals(this.matrix1, sample1), Is.True);
            Assert.That(Matrix.Equals(this.matrix2, sample2), Is.True);
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
        var sample = new[,]
        {
            { 95, 110 },
            { 220, 260 },
            { 345, 410 },
        };
        Assert.That(Matrix.Equals(this.resultOfSequentialMultiplication, sample), Is.True);
    }

    [Test]
    public void MultiplicationShouldThrowIncompatibleMatrixSizesExceptionIfIncompatibleMatrixSizes()
    {
        var sample = new[,]
        {
            { 95, 110 },
            { 220, 260 },
            { 345, 410 },
        };
        Assert.Throws<IncompatibleMatrixSizesException>(
            () => Matrix.Multiplication(this.matrix1, sample));
    }

    [Test]
    public void ParallelMultiplicationTest()
    {
        var sample = new[,]
        {
            { 95, 110 },
            { 220, 260 },
            { 345, 410 },
        };
        Assert.That(Matrix.Equals(this.resultOfParallelMultiplication, sample), Is.True);
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
        Matrix.SaveToFile("../../../TestResultInput.txt", this.resultOfSequentialMultiplication);
        var input = Matrix.ReadFromFile("../../../TestResultInput.txt");
        Assert.That(Matrix.Equals(input, this.resultOfSequentialMultiplication), Is.True);
    }

    [Test]
    public void SaveToFileShouldThrowArgumentExceptionIfAnEmptyPath()
    {
        Assert.Throws<ArgumentException>(
            () => Matrix.SaveToFile(string.Empty, this.resultOfSequentialMultiplication));
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