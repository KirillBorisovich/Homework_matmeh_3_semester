using ParallelMatrixMultiplication;

Matrix.ReadFromFile("../../../input.txt");
int[,] matrixA =
{
    { 1, 2, 3 },
    { 4, 5, 6 },
    { 7, 8, 9 },
};

int[,] matrixB =
{
    { 1, 2, 3 },
    { 4, 5, 6 },
    { 7, 8, 9 },
};

var result = Matrix.ParallelMatrixMultiplication(matrixA, matrixB);