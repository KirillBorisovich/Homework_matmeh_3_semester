// <copyright file="Db.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace Test_29_12_2025;

using System.Globalization;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Test_29_12_2025.Models;

/// <summary>
/// Database for homework management.
/// </summary>
public static class Db
{
    /// <summary>
    /// Initialize Database.
    /// </summary>
    public static void InitializeDatabase()
    {
        using var context = new HomeworkContext();
        context.Database.EnsureCreated();
    }

    /// <summary>
    /// Add homework.
    /// </summary>
    public static void AddHomework()
        {
            Console.Write("Enter the name of your homework: ");
            var title = Console.ReadLine() ?? string.Empty;

            Console.Write("Enter the deadline (формат: yyyy-MM-dd HH:mm): ");
            var deadlineStr = Console.ReadLine();

            if (!DateTime.TryParseExact(
                deadlineStr ?? string.Empty,
                "yyyy-MM-dd HH:mm",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var deadline))
            {
                Console.WriteLine("Incorrect date format.");
                return;
            }

            using var context = new HomeworkContext();
            var hw = new Homework
            {
                Title = title,
                Deadline = deadline,
            };
            context.Homeworks.Add(hw);
            context.SaveChanges();

            Console.WriteLine("Homework added.");
        }

    /// <summary>
    /// Add task.
    /// </summary>
    public static void AddTask()
        {
            using var context = new HomeworkContext();
            try
            {
                ShowHomeworksShort(context);
            }
            catch (InvalidOperationException)
            {
                return;
            }

            Console.Write("Enter the ID of the homework to add the task to: ");
            if (!int.TryParse(Console.ReadLine(), out var hwId))
            {
                Console.WriteLine("Invalid ID.");
                return;
            }

            var homework = context.Homeworks.FirstOrDefault(h => h.Id == hwId);
            if (homework == null)
            {
                Console.WriteLine("No homework with this ID was found..");
                return;
            }

            Console.Write("Enter the text of the task condition: ");
            var description = Console.ReadLine() ?? string.Empty;

            Console.Write("Enter the maximum score for the task: ");
            if (!int.TryParse(Console.ReadLine(), out var maxScore))
            {
                Console.WriteLine("Incorrect number.");
                return;
            }

            var task = new HomeworkTask
            {
                HomeworkId = hwId,
                Description = description,
                MaxScore = maxScore,
            };

            context.Tasks.Add(task);
            context.SaveChanges();

            Console.WriteLine("Task added.");
        }

    /// <summary>
    /// Delete homework.
    /// </summary>
    public static void DeleteHomework()
        {
            using var context = new HomeworkContext();
            try
            {
                ShowHomeworksShort(context);
            }
            catch (InvalidOperationException)
            {
                return;
            }

            Console.Write("Enter the ID of the homework to delete: ");
            if (!int.TryParse(Console.ReadLine(), out var hwId))
            {
                Console.WriteLine("Invalid ID.");
                return;
            }

            var homework = context.Homeworks
                .Include(h => h.Tasks)
                .ThenInclude(t => t.Solutions)
                .FirstOrDefault(h => h.Id == hwId);

            if (homework == null)
            {
                Console.WriteLine("No homework with this ID was found.");
                return;
            }

            context.Homeworks.Remove(homework);
            context.SaveChanges();

            Console.WriteLine("Homework has been deleted.");
        }

    /// <summary>
    /// Show homeworks and tasks.
    /// </summary>
    public static void ShowHomeworksAndTasks()
        {
            using var context = new HomeworkContext();
            var homeworks = context.Homeworks
                .Include(h => h.Tasks)
                .OrderBy(h => h.Id)
                .ToList();

            if (homeworks.Count == 0)
            {
                Console.WriteLine("No homework yet.");
                return;
            }

            foreach (var hw in homeworks)
            {
                Console.WriteLine();
                Console.WriteLine($"Homework ID={hw.Id}, title: {hw.Title}");
                Console.WriteLine($"  Deadline: {hw.Deadline:yyyy-MM-dd HH:mm}");

                if (hw.Tasks == null || hw.Tasks.Count == 0)
                {
                    Console.WriteLine("  Tasks: none");
                }
                else
                {
                    Console.WriteLine("  Tasks:");
                    foreach (var task in hw.Tasks.OrderBy(t => t.Id))
                    {
                        Console.WriteLine($"    Task ID={task.Id}, max. score={task.MaxScore}");
                        Console.WriteLine($"      Condition: {task.Description}");
                    }
                }
            }
        }

    /// <summary>
    /// Add solution.
    /// </summary>
    public static void AddSolution()
        {
            using var context = new HomeworkContext();
            try
            {
                ShowTasksShort(context);
            }
            catch (InvalidOperationException)
            {
                return;
            }

            Console.Write("Enter the ID of the task for which the solution is being added: ");
            if (!int.TryParse(Console.ReadLine(), out var taskId))
            {
                Console.WriteLine("Invalid ID.");
                return;
            }

            var task = context.Tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null)
            {
                Console.WriteLine("A task with this ID was not found.");
                return;
            }

            Console.Write("Enter the text of the decision: ");
            var text = Console.ReadLine() ?? string.Empty;

            var solution = new Solution
            {
                TaskId = taskId,
                SubmissionDate = DateTime.Now,
                Text = text,
                Score = 0,
            };

            context.Solutions.Add(solution);
            context.SaveChanges();

            Console.WriteLine("Solution added.");
        }

    /// <summary>
    /// Show solutions for task.
    /// </summary>
    public static void ShowSolutionsForTask()
        {
            using var context = new HomeworkContext();
            try
            {
                ShowTasksShort(context);
            }
            catch (InvalidOperationException)
            {
                return;
            }

            Console.Write("Enter the ID of the task to show solutions for: ");
            if (!int.TryParse(Console.ReadLine(), out var taskId))
            {
                Console.WriteLine("Invalid ID.");
                return;
            }

            var solutions = context.Solutions
                .Where(s => s.TaskId == taskId)
                .Include(s => s.Task)
                    .ThenInclude(t => t!.Homework)
                .OrderBy(s => s.Id)
                .ToList();

            if (solutions.Count == 0)
            {
                Console.WriteLine("There are no solutions for this task yet.");
                return;
            }

            foreach (var sol in solutions)
            {
                var hw = sol.Task?.Homework;
                var onTime = hw != null && sol.SubmissionDate <= hw.Deadline;
                var status = onTime ? "Completed before the deadline" : "Completed after the deadline";

                Console.WriteLine();
                Console.WriteLine($"Decision ID={sol.Id}");
                Console.WriteLine($"  Date of delivery: {sol.SubmissionDate:yyyy-MM-dd HH:mm:ss} ({status})");
                Console.WriteLine($"  Score: {sol.Score}");
                Console.WriteLine($"  Text: {sol.Text}");
            }
        }

    /// <summary>
    /// Change solution score.
    /// </summary>
    public static void ChangeSolutionScore()
        {
            using var context = new HomeworkContext();
            try
            {
                ShowTasksShort(context);
            }
            catch (InvalidOperationException)
            {
                ShowSolutionsShort(context);
            }

            Console.Write("Enter the ID of the solution for which you want to change the score.: ");
            if (!int.TryParse(Console.ReadLine(), out var solId))
            {
                Console.WriteLine("Invalid ID.");
                return;
            }

            var solution = context.Solutions.FirstOrDefault(s => s.Id == solId);
            if (solution == null)
            {
                Console.WriteLine("No solution has been found with this ID..");
                return;
            }

            Console.Write("Enter a new score: ");
            if (!int.TryParse(Console.ReadLine(), out var newScore))
            {
                Console.WriteLine("Incorrect number.");
                return;
            }

            solution.Score = newScore;
            context.SaveChanges();

            Console.WriteLine("Score updated.");
        }

    /// <summary>
    /// Show total score.
    /// </summary>
    public static void ShowTotalScore()
        {
            using var context = new HomeworkContext();
            var total = context.Solutions.Sum(s => (int?)s.Score) ?? 0;
            Console.WriteLine($"The total number of points for all solutions: {total}");
        }

    private static void ShowHomeworksShort(HomeworkContext context)
        {
            var homeworks = context.Homeworks
                .OrderBy(h => h.Id)
                .ToList();

            Console.WriteLine("List of homework chores:");
            if (homeworks.Count == 0)
            {
                Console.WriteLine("  There are no homeworks");
                throw new InvalidOperationException();
            }

            foreach (var hw in homeworks)
            {
                Console.WriteLine(
                    $"  ID={hw.Id}: {hw.Title}, deadline: {hw.Deadline:yyyy-MM-dd HH:mm}");
            }
        }

    private static void ShowTasksShort(HomeworkContext context)
        {
            var tasks = context.Tasks
                .Include(t => t.Homework)
                .OrderBy(t => t.Id)
                .ToList();

            Console.WriteLine("Task list:");
            if (tasks.Count == 0)
            {
                Console.WriteLine("  There are no homeworks");
                throw new InvalidOperationException();
            }

            foreach (var t in tasks)
            {
                Console.WriteLine(
                    $"  ID={t.Id}, HW: {t.Homework?.Title}, max. score={t.MaxScore}");
                Console.WriteLine($"     Condition: {t.Description}");
            }
        }

    private static void ShowSolutionsShort(HomeworkContext context)
        {
            var solutions = context.Solutions
                .Include(s => s.Task)
                    .ThenInclude(t => t!.Homework)
                .OrderBy(s => s.Id)
                .ToList();

            Console.WriteLine("List of solutions:");
            if (solutions.Count == 0)
            {
                Console.WriteLine("  There are no solutions");
                throw new InvalidOperationException();
            }

            foreach (var s in solutions)
            {
                Console.WriteLine(
                    $"  Decision ID={s.Id}, task ID={s.TaskId} (HW: {s.Task?.Homework?.Title}), " +
                    $"score={s.Score}, date of delivery={s.SubmissionDate:yyyy-MM-dd HH:mm:ss}");
            }
        }
}
