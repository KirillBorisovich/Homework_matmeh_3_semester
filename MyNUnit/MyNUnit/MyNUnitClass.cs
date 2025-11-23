namespace MyNUnit;

using System.Collections.Concurrent;
using System.Reflection;
using Attributes;

public class MyNUnitClass
{
    private readonly ConcurrentBag<int> safeBag = [];

    public static string RunAllTheTestsAlongThisPath(string path)
    {
        if (!File.Exists(path))
        {
            if (Path.GetExtension(path) != ".dll" && Path.GetExtension(path) != ".exe")
            {
                throw new ArgumentException("Incorrect file extension.");
            }

            return RunAllTheTestsInTheFile(path);
        }
        else if (!Directory.Exists(path))
        {
            return RunAllTheTestsInTheDirectory(path);
        }
        else
        {
            throw new ArgumentException("No such file or directory.");
        }
    }

    private static string RunAllTheTestsInTheFile(string path)
    {
        var assembly = Assembly.LoadFile(path);
        foreach (var type in assembly.ExportedTypes)
        {
            
        }
    }

    private static string RunAllTheTestsInTheDirectory(string path)
    {
        var assemblyNames = Directory.EnumerateFiles(path, "*.dll");
        foreach (var assemblyName in assemblyNames)
        {
            var assembly = Assembly.LoadFile(assemblyName);
            foreach (var type in assembly.ExportedTypes)
            {
                
            }
        }
    }

    private static string RunAllTheTestsInTheClass(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var methods =
            FindAllTheMethodsWithTheNecessaryAttributesForTestingInTheClass(type);

        if (methods.Count == 0)
        {
            throw new NoMethodsFoundForTestingException();
        }

        Parallel.ForEach(
            methods[nameof(BeforeClassAttribute)],
            method => method.Invoke(null, null));

        Parallel.ForEach(methods[nameof(TestAttribute)], testMethod =>
        {
            var instance = Activator.CreateInstance(type);
            foreach (var beforeMethod in methods[nameof(BeforeAttribute)])
            {
                beforeMethod.Invoke(instance, null);
            }

            testMethod.Invoke(instance, null);

            foreach (var afterMethod in methods[nameof(AfterAttribute)])
            {
                afterMethod.Invoke(instance, null);
            }
        })
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
}