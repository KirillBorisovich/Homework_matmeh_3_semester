// <copyright file="MyNUnitClass.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

using Core;

namespace MyNUnit;

using System.Collections.Concurrent;
using System.Reflection;

/// <summary>
/// The type for running test methods.
/// </summary>
public class MyNUnitClass
{
    private readonly ConcurrentBag<string> safeBag = [];

    /// <summary>
    /// Run all the tests along path.
    /// </summary>
    /// <param name="path">The path to the assemblies.</param>
    /// <returns>The result of the test methods execution.</returns>
    /// <exception cref="ArgumentException">The exception is that the file
    /// or directory is not found.</exception>
    public async Task<string> RunAllTheTestsAlongThisPath(string path)
    {
        if (!File.Exists(path))
        {
            if (Path.GetExtension(path) != ".dll" && Path.GetExtension(path) != ".exe")
            {
                throw new ArgumentException("Incorrect file extension.");
            }

            return await this.RunAllTheTestsInTheFile(path);
        }
        else if (!Directory.Exists(path))
        {
            return await this.RunAllTheTestsInTheDirectory(path);
        }
        else
        {
            throw new ArgumentException("No such file or directory.");
        }
    }

    private static Dictionary<string, List<MethodInfo>>
        FindAllTheMethodsWithTheNecessaryAttributesForTestingInTheClass(Type type)
    {
        Dictionary<string, List<MethodInfo>> methods = new();
        foreach (var method in type.GetMethods())
        {
            foreach (var attribute in method.GetCustomAttributes())
            {
                switch (attribute)
                {
                    case BeforeClassAttribute when method.IsStatic:
                    case AfterClassAttribute when method.IsStatic:
                    case BeforeAttribute:
                    case AfterAttribute:
                    case TestAttribute:
                        var key = attribute.GetType().Name;
                        if (!methods.TryGetValue(key, out var value))
                        {
                            value = [];
                            methods[key] = value;
                        }

                        value.Add(method);
                        continue;
                }
            }
        }

        return methods;
    }

    private async Task<string> RunAllTheTestsInTheDirectory(string path)
    {
        var assemblyFiles = Directory.EnumerateFiles(path, "*.dll")
            .Concat(Directory.EnumerateFiles(path, "*.exe"));

        var tasks = assemblyFiles.Select(file =>
        {
            var assembly = Assembly.LoadFile(file);
            return this.RunAllTheTestsInTheFile(assembly);
        });

        var results = await Task.WhenAll(tasks);

        return string.Concat(results);
    }

    private async Task<string> RunAllTheTestsInTheFile(string path)
    {
        var assembly = Assembly.LoadFile(path);
        return await this.RunAllTheTestsInTheFile(assembly);
    }

    private async Task<string> RunAllTheTestsInTheFile(Assembly assembly)
    {
        await Parallel.ForEachAsync(
            assembly.ExportedTypes,
            async (type, _) => await this.RunAllTheTestsInTheClass(type));

        return string.Concat(this.safeBag);
    }

    private async ValueTask RunAllTheTestsInTheClass(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var methods =
            FindAllTheMethodsWithTheNecessaryAttributesForTestingInTheClass(type);

        if (methods.Count == 0)
        {
            return;
        }

        if (methods.TryGetValue(nameof(BeforeClassAttribute), out var beforeClassMethods))
        {
            foreach (var beforeClassMethod in beforeClassMethods)
            {
                beforeClassMethod.Invoke(null, null);
            }
        }

        await Parallel.ForEachAsync(
            methods[nameof(TestAttribute)],
            (testMethod, _) =>
            {
                var testAttribute = testMethod.GetCustomAttribute<TestAttribute>();
                if (testAttribute?.Ignore != null)
                {
                    this.safeBag.Add($"Test Ignored: {testMethod.Name}\n" +
                                     $"    Reason: {testAttribute.Ignore}\n \n");

                    return ValueTask.CompletedTask;
                }

                var instance = Activator.CreateInstance(type);
                if (methods.TryGetValue(nameof(BeforeAttribute), out var beforeMethods))
                {
                    foreach (var beforeMethod in beforeMethods)
                    {
                        beforeMethod.Invoke(instance, null);
                    }
                }

                try
                {
                    testMethod.Invoke(instance, null);
                    this.safeBag.Add($"Test Passed: {testMethod.Name}\n \n");
                }
                catch (TargetInvocationException ex) when (ex.InnerException is AssertFailedException)
                {
                    this.safeBag.Add($"Test Failed: {testMethod.Name}\n" + ex.InnerException.Message + "\n");
                }
                catch (TargetInvocationException ex)
                {
                    if (testAttribute?.Expected != null &&
                        ex.InnerException != null &&
                        ex.InnerException.GetType() == testAttribute.Expected)
                    {
                        this.safeBag.Add($"Test Passed: {testMethod.Name}\n \n");
                    }
                    else
                    {
                        this.safeBag.Add(
                            $"Test Failed: {testMethod.Name}\n" +
                            $"    Unexpected exception: {ex.InnerException?.GetType().Name}\n" +
                            $"    Message: {ex.InnerException?.Message}\n \n");
                    }
                }

                if (methods.TryGetValue(nameof(AfterAttribute), out var afterMethods))
                {
                    foreach (var afterMethod in afterMethods)
                    {
                        afterMethod.Invoke(instance, null);
                    }
                }

                return ValueTask.CompletedTask;
            });

        if (methods.TryGetValue(nameof(AfterClassAttribute), out var afterClassMethods))
        {
            foreach (var afterClassMethod in afterClassMethods)
            {
                afterClassMethod.Invoke(null, null);
            }
        }
    }
}