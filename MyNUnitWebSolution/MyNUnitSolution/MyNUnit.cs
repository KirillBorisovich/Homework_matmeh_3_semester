// <copyright file="MyNUnitSolution.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace MyNUnitSolution;

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using Core;

/// <summary>
/// The type for running test methods.
/// </summary>
public class MyNUnit
{
    private readonly ConcurrentBag<string> safeBag = [];

    /// <summary>
    /// Run all the tests along path.
    /// </summary>
    /// <param name="path">The path to the assemblies.</param>
    /// <returns>The result of the test methods execution.</returns>
    /// <exception cref="ArgumentException">The exception is that the file
    /// or directory is not found.</exception>
    public async Task<ConcurrentBag<string>> RunAllTheTestsAlongThisPath(string path)
    {
        if (File.Exists(path))
        {
            if (Path.GetExtension(path) != ".dll" && Path.GetExtension(path) != ".exe")
            {
                throw new ArgumentException("Incorrect file extension.");
            }

            return await this.RunAllTheTestsInTheFile(path);
        }
        else if (Directory.Exists(path))
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

    private async Task<ConcurrentBag<string>> RunAllTheTestsInTheDirectory(string path)
    {
        var assemblyFiles = Directory.EnumerateFiles(path, "*.dll")
            .Concat(Directory.EnumerateFiles(path, "*.exe"))
            .ToArray();

        if (assemblyFiles.Length == 0)
        {
            throw new ArgumentException("The directory does not contain any assemblies.");
        }

        await Parallel.ForEachAsync(assemblyFiles, async (file, _) =>
        {
            var fullPath = Path.GetFullPath(file);
            var assembly = Assembly.LoadFrom(fullPath);
            await this.RunAllTheTestsInTheFile(assembly);
        });

        return new ConcurrentBag<string>(this.safeBag);
    }

    private async Task<ConcurrentBag<string>> RunAllTheTestsInTheFile(string path)
    {
        var assembly = Assembly.LoadFile(path);
        return await this.RunAllTheTestsInTheFile(assembly);
    }

    private async Task<ConcurrentBag<string>> RunAllTheTestsInTheFile(Assembly assembly)
    {
        await Parallel.ForEachAsync(
            assembly.ExportedTypes,
            async (type, _) => await this.RunAllTheTestsInTheClass(type));

        return new ConcurrentBag<string>(this.safeBag);
    }

    private async ValueTask RunAllTheTestsInTheClass(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        ConcurrentBag<string> temporaryBag = [];

        var methods =
            FindAllTheMethodsWithTheNecessaryAttributesForTestingInTheClass(type);

        if (methods.Count == 0)
        {
            return;
        }

        try
        {
            this.RunAuxiliaryMethods(methods, null, nameof(BeforeClassAttribute));

            await Parallel.ForEachAsync(
                methods[nameof(TestAttribute)],
                (testMethod, _) =>
                {
                    var testAttribute = testMethod.GetCustomAttribute<TestAttribute>();
                    if (testAttribute?.Ignore != null)
                    {
                        temporaryBag.Add($"\nTest Ignored: {testMethod.Name}\n" +
                                         $"    Reason: {testAttribute.Ignore}\n");

                        return ValueTask.CompletedTask;
                    }

                    var instance = Activator.CreateInstance(type);
                    this.RunAuxiliaryMethods(methods, instance, nameof(BeforeAttribute));

                    var stopwatch = Stopwatch.StartNew();
                    try
                    {
                        testMethod.Invoke(instance, null);
                        stopwatch.Stop();
                        temporaryBag.Add($"\nTest Passed: {testMethod.Name}\n" +
                                         $"    Time: {stopwatch.ElapsedMilliseconds} ms\n");
                    }
                    catch (TargetInvocationException ex) when (ex.InnerException is AssertFailedException)
                    {
                        stopwatch.Stop();
                        temporaryBag.Add($"\nTest Failed: {testMethod.Name}\n" +
                                         $"    Time: {stopwatch.ElapsedMilliseconds} ms\n"
                                         + ex.InnerException.Message + "\n");
                    }
                    catch (TargetInvocationException ex)
                    {
                        stopwatch.Stop();
                        if (testAttribute?.Expected != null &&
                            ex.InnerException != null &&
                            ex.InnerException.GetType() == testAttribute.Expected)
                        {
                            temporaryBag.Add($"\nTest Passed: {testMethod.Name}\n" +
                                             $"    Time: {stopwatch.ElapsedMilliseconds} ms\n");
                        }
                        else
                        {
                            temporaryBag.Add(
                                $"\nTest Failed: {testMethod.Name}\n" +
                                $"    Time: {stopwatch.ElapsedMilliseconds} ms\n" +
                                $"    Unexpected exception: {ex.InnerException?.GetType().Name}\n" +
                                $"    Message: {ex.InnerException?.Message}\n");
                        }
                    }

                    this.RunAuxiliaryMethods(methods, instance, nameof(AfterAttribute));

                    return ValueTask.CompletedTask;
                });

            this.RunAuxiliaryMethods(methods, null, nameof(AfterClassAttribute));

            foreach (var str in temporaryBag)
            {
                this.safeBag.Add(str);
            }
        }
        catch (TheAuxiliaryMethodDroppedTheException)
        {
        }
        catch (AggregateException ex)
            when (ex.InnerException is TheAuxiliaryMethodDroppedTheException)
        {
        }
        catch (Exception ex)
        {
            this.safeBag.Add("\n" + ex.Message + "\n");
        }
    }

    private void RunAuxiliaryMethods(
        Dictionary<string, List<MethodInfo>> methods,
        object? instance,
        string attributeName)
    {
        if (!methods.TryGetValue(attributeName, out var attributeMethods))
        {
            return;
        }

        foreach (var attributeMethod in attributeMethods)
        {
            try
            {
                attributeMethod.Invoke(instance, null);
            }
            catch (TargetInvocationException ex)
            {
                this.safeBag.Add(
                    $"\nAn exception is raised in: {attributeMethod.Name}\n" +
                    $"    Exception: {ex.InnerException?.GetType().Name}\n" +
                    $"    Message: {ex.InnerException?.Message}\n");
                throw new TheAuxiliaryMethodDroppedTheException();
            }
        }
    }
}