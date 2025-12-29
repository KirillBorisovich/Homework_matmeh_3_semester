// <copyright file="Program.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

using Test_29_12_2025;

Db.InitializeDatabase();

Console.WriteLine("\nHomework management\n");
Console.WriteLine("==== Menu ====");
Console.WriteLine("1. Add homework");
Console.WriteLine("2. Add a task to homework");
Console.WriteLine("3. Delete homework");
Console.WriteLine("4. Show all homeworks and tasks");
Console.WriteLine("5. Add a solution to a task");
Console.WriteLine("6. Show all solutions for a task");
Console.WriteLine("7. Change solution score");
Console.WriteLine("8. Show total score");
Console.WriteLine("0. Exit");
Console.Write("\nSelect a menu option:\n");

var input = Console.ReadLine();
Console.WriteLine();
while (input != "0")
{
    switch (input)
    {
        case "1":
            Db.AddHomework();
            break;
        case "2":
            Db.AddTask();
            break;
        case "3":
            Db.DeleteHomework();
            break;
        case "4":
            Db.ShowHomeworksAndTasks();
            break;
        case "5":
            Db.AddSolution();
            break;
        case "6":
            Db.ShowSolutionsForTask();
            break;
        case "7":
            Db.ChangeSolutionScore();
            break;
        case "8":
            Db.ShowTotalScore();
            break;
        case "0":
            break;
        default:
            Console.WriteLine("Unknown menu item.");
            break;
    }

    Console.WriteLine("\nSelect a menu option:");
    input = Console.ReadLine();
    Console.WriteLine();
}