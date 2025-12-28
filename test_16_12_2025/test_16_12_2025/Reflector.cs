// <copyright file="Reflector.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace Test_16_12_2025;

using System.Reflection;
using System.Text;

/// <summary>
/// A set of methods for reflection.
/// </summary>
public class Reflector
{
    /// <summary>
    /// Print all fields and methods that differ in the two classes.
    /// </summary>
    /// <param name="a">First class.</param>
    /// <param name="b">Second class.</param>
    /// <exception cref="ArgumentNullException">The exception about null arguments.</exception>
    public static void DiffClasses(Type a, Type b)
    {
        if (a == null || b == null)
        {
            throw new ArgumentNullException(a == null ? nameof(a) : nameof(b));
        }

        Console.WriteLine($"Сравнение классов:");
        Console.WriteLine($"A: {a.FullName}");
        Console.WriteLine($"B: {b.FullName}");
        Console.WriteLine();

        var fieldsA = a.GetFields(BindingFlags.Public | BindingFlags.NonPublic |
                                 BindingFlags.Static | BindingFlags.Instance);
        var fieldsB = b.GetFields(BindingFlags.Public | BindingFlags.NonPublic |
                                 BindingFlags.Static | BindingFlags.Instance);
        var methodsA = a.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                                    BindingFlags.Static | BindingFlags.Instance)
                       .Where(m => !m.IsSpecialName);
        var methodsB = b.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                                   BindingFlags.Static | BindingFlags.Instance)
                       .Where(m => !m.IsSpecialName);

        Console.WriteLine("1. Поля только в классе A:");
        var onlyInAFields = fieldsA.Where(fa => !fieldsB.Any(fb =>
            fb.Name == fa.Name &&
            fb.FieldType == fa.FieldType &&
            fb.IsStatic == fa.IsStatic &&
            GetAccessModifier(fb) == GetAccessModifier(fa)));

        foreach (var field in onlyInAFields)
        {
            Console.WriteLine($"   {GetFieldSignature(field)}");
        }

        Console.WriteLine();
        Console.WriteLine("2. Поля только в классе B:");
        var onlyInBFields = fieldsB.Where(fb => !fieldsA.Any(fa =>
            fb.Name == fa.Name &&
            fb.FieldType == fa.FieldType &&
            fb.IsStatic == fa.IsStatic &&
            GetAccessModifier(fb) == GetAccessModifier(fa)));
        foreach (var field in onlyInBFields)
        {
            Console.WriteLine($"   {GetFieldSignature(field)}");
        }

        Console.WriteLine();

        Console.WriteLine("3. Методы только в классе A:");
        var methodInfos = methodsA as MethodInfo[] ?? methodsA.ToArray();
        var onlyInAMethods = methodInfos.Where(ma => methodsB.All(mb => GetMethodSignature(ma) != GetMethodSignature(mb)));

        foreach (var method in onlyInAMethods)
        {
            Console.WriteLine($"   {GetMethodSignature(method)}");
        }

        Console.WriteLine();

        Console.WriteLine("4. Методы только в классе B:");
        var enumerable = methodsB as MethodInfo[] ?? methodsB.ToArray();
        var onlyInBMethods = enumerable.Where(mb => methodInfos.All(ma => GetMethodSignature(ma) != GetMethodSignature(mb)));

        foreach (var method in onlyInBMethods)
        {
            Console.WriteLine($"   {GetMethodSignature(method)}");
        }

        Console.WriteLine();

        Console.WriteLine("5. Поля, присутствующие в обоих классах, но отличающиеся:");
        var commonFields = fieldsA.Where(fa => fieldsB.Any(fb => fb.Name == fa.Name));

        foreach (var fieldA in commonFields)
        {
            var fieldB = fieldsB.First(fb => fb.Name == fieldA.Name);

            var differ = false;
            var differences = new List<string>();

            if (fieldA.FieldType != fieldB.FieldType)
            {
                differences.Add($"тип: {fieldA.FieldType.Name} vs {fieldB.FieldType.Name}");
                differ = true;
            }

            if (fieldA.IsStatic != fieldB.IsStatic)
            {
                differences.Add($"static: {fieldA.IsStatic} vs {fieldB.IsStatic}");
                differ = true;
            }

            if (GetAccessModifier(fieldA) != GetAccessModifier(fieldB))
            {
                differences.Add($"доступ: {GetAccessModifier(fieldA)} vs {GetAccessModifier(fieldB)}");
                differ = true;
            }

            if (fieldA.IsInitOnly != fieldB.IsInitOnly)
            {
                differences.Add($"readonly: {fieldA.IsInitOnly} vs {fieldB.IsInitOnly}");
                differ = true;
            }

            if (!differ)
            {
                continue;
            }

            Console.WriteLine($"   {fieldA.Name}:");
            foreach (var diff in differences)
            {
                Console.WriteLine($"     - {diff}");
            }
        }

        Console.WriteLine();

        Console.WriteLine("6. Методы, присутствующие в обоих классах, но отличающиеся:");
        var commonMethods = methodInfos.Where(ma => enumerable.Any(mb =>
            mb.Name == ma.Name &&
            GetParameterTypes(ma).SequenceEqual(GetParameterTypes(mb))));

        foreach (var methodA in commonMethods)
        {
            var methodB = enumerable.First(mb =>
                mb.Name == methodA.Name &&
                GetParameterTypes(mb).SequenceEqual(GetParameterTypes(methodA)));

            var differ = false;
            var differences = new List<string>();

            if (methodA.ReturnType != methodB.ReturnType)
            {
                differences.Add($"возвращаемый тип: {methodA.ReturnType.Name} vs {methodB.ReturnType.Name}");
                differ = true;
            }

            if (GetAccessModifier(methodA) != GetAccessModifier(methodB))
            {
                differences.Add($"доступ: {GetAccessModifier(methodA)} vs {GetAccessModifier(methodB)}");
                differ = true;
            }

            if (methodA.IsStatic != methodB.IsStatic)
            {
                differences.Add($"static: {methodA.IsStatic} vs {methodB.IsStatic}");
                differ = true;
            }

            if (methodA.IsVirtual != methodB.IsVirtual)
            {
                differences.Add($"virtual: {methodA.IsVirtual} vs {methodB.IsVirtual}");
                differ = true;
            }

            if (methodA.IsAbstract != methodB.IsAbstract)
            {
                differences.Add($"abstract: {methodA.IsAbstract} vs {methodB.IsAbstract}");
                differ = true;
            }

            if (methodA.IsGenericMethod != methodB.IsGenericMethod)
            {
                differences.Add($"generic: {methodA.IsGenericMethod} vs {methodB.IsGenericMethod}");
                differ = true;
            }
            else if (methodA.IsGenericMethod)
            {
                var genA = methodA.GetGenericArguments();
                var genB = methodB.GetGenericArguments();
                if (genA.Length != genB.Length)
                {
                    differences.Add($"кол-во generic параметров: {genA.Length} vs {genB.Length}");
                    differ = true;
                }
            }

            if (!differ)
            {
                continue;
            }

            Console.WriteLine($"   {methodA.Name}:");
            foreach (var diff in differences)
            {
                Console.WriteLine($"     - {diff}");
            }
        }
    }

    /// <summary>
    /// create a file with the generated class using reflection.
    /// </summary>
    /// <param name="someClass">Type for playback.</param>
    public static void PrintStructure(Type someClass)
    {
        ArgumentNullException.ThrowIfNull(someClass);

        var fileName = $"{someClass.Name}.cs";

        using var writer = new StreamWriter(fileName, false, Encoding.UTF8);
        writer.WriteLine("using System;");
        writer.WriteLine("using System.Collections.Generic;");
        writer.WriteLine();

        if (!string.IsNullOrEmpty(someClass.Namespace))
        {
            writer.WriteLine($"namespace {someClass.Namespace}");
            writer.WriteLine("{");
        }

        WriteTypeDeclaration(writer, someClass, 0);

        if (string.IsNullOrEmpty(someClass.Namespace))
        {
            return;
        }

        writer.WriteLine("}");
    }

    private static void WriteTypeDeclaration(StreamWriter writer, Type type, int indentLevel)
    {
        var indent = new string(' ', indentLevel * 4);

        if (type.IsPublic)
        {
            writer.Write(indent + "public ");
        }
        else if (type.IsNotPublic)
        {
            writer.Write(indent + "internal ");
        }

        switch (type)
        {
            case { IsAbstract: true, IsSealed: true }:
                writer.Write("static ");
                break;
            case { IsAbstract: true, IsInterface: false }:
                writer.Write("abstract ");
                break;
            case { IsSealed: true, IsInterface: false }:
                writer.Write("sealed ");
                break;
        }

        if (type is { IsClass: true, IsInterface: false })
        {
            writer.Write("class ");
        }
        else if (type.IsInterface)
        {
            writer.Write("interface ");
        }
        else if (type is { IsValueType: true, IsEnum: false })
        {
            writer.Write("struct ");
        }

        writer.Write(GetTypeName(type, false));

        if (type.BaseType != null && type.BaseType != typeof(object) && !type.IsInterface)
        {
            writer.Write(" : " + GetTypeName(type.BaseType, true));
        }

        var interfaces = type.GetInterfaces();
        if (interfaces.Length > 0)
        {
            var interfaceNames = interfaces.Select(i => GetTypeName(i, true)).ToArray();
            if (type.BaseType != null && type.BaseType != typeof(object) && !type.IsInterface)
            {
                writer.Write(", " + string.Join(", ", interfaceNames));
            }
            else if (!type.IsInterface)
            {
                writer.Write(" : " + string.Join(", ", interfaceNames));
            }
        }

        writer.WriteLine();
        writer.WriteLine(indent + "{");

        var nestedTypes = type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic |
                                             BindingFlags.Static | BindingFlags.Instance);
        foreach (var nestedType in nestedTypes)
        {
            WriteTypeDeclaration(writer, nestedType, indentLevel + 1);
        }

        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic |
                                   BindingFlags.Static | BindingFlags.Instance |
                                   BindingFlags.DeclaredOnly);
        foreach (var field in fields)
        {
            WriteField(writer, field, indentLevel + 1);
        }

        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                                     BindingFlags.Static | BindingFlags.Instance |
                                     BindingFlags.DeclaredOnly)
                         .Where(m => !m.IsSpecialName); // Исключаем property getters/setters

        foreach (var method in methods)
        {
            WriteMethod(writer, method, indentLevel + 1);
        }

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic |
                                          BindingFlags.Static | BindingFlags.Instance |
                                          BindingFlags.DeclaredOnly);
        foreach (var property in properties)
        {
            WriteProperty(writer, property, indentLevel + 1);
        }

        writer.WriteLine(indent + "}");
    }

    private static void WriteField(StreamWriter writer, FieldInfo field, int indentLevel)
    {
        var indent = new string(' ', indentLevel * 4);

        if (field.IsPublic)
        {
            writer.Write(indent + "public ");
        }
        else if (field.IsPrivate)
        {
            writer.Write(indent + "private ");
        }
        else if (field.IsFamily)
        {
            writer.Write(indent + "protected ");
        }
        else if (field.IsAssembly)
        {
            writer.Write(indent + "internal ");
        }
        else if (field.IsFamilyOrAssembly)
        {
            writer.Write(indent + "protected internal ");
        }
        else if (field.IsFamilyAndAssembly)
        {
            writer.Write(indent + "private protected ");
        }

        if (field.IsStatic)
        {
            writer.Write("static ");
        }

        if (field.IsInitOnly)
        {
            writer.Write("readonly ");
        }

        if (field is { IsLiteral: true, IsInitOnly: false })
        {
            writer.Write("const ");
        }

        writer.WriteLine($"{GetTypeName(field.FieldType, true)} {field.Name};");
    }

    private static void WriteMethod(StreamWriter writer, MethodInfo method, int indentLevel)
    {
        var indent = new string(' ', indentLevel * 4);
        if (method.IsConstructor || method.Name == "Finalize")
        {
            return;
        }

        // Модификаторы доступа
        if (method.IsPublic)
        {
            writer.Write(indent + "public ");
        }
        else if (method.IsPrivate)
        {
            writer.Write(indent + "private ");
        }
        else if (method.IsFamily)
        {
            writer.Write(indent + "protected ");
        }
        else if (method.IsAssembly)
        {
            writer.Write(indent + "internal ");
        }
        else if (method.IsFamilyOrAssembly)
        {
            writer.Write(indent + "protected internal ");
        }
        else if (method.IsFamilyAndAssembly)
        {
            writer.Write(indent + "private protected ");
        }

        if (method.IsStatic)
        {
            writer.Write("static ");
        }

        if (method.IsAbstract)
        {
            writer.Write("abstract ");
        }
        else if (method is { IsVirtual: true, IsFinal: false })
        {
            writer.Write("virtual ");
        }
        else if (method is { IsFinal: true, IsVirtual: true })
        {
            writer.Write("sealed override ");
        }

        // Generic параметры метода
        var genericParams = string.Empty;
        if (method.IsGenericMethod)
        {
            var typeParams = method.GetGenericArguments();
            genericParams = $"<{string.Join(", ", typeParams.Select(t => t.Name))}>";
        }

        writer.Write($"{GetTypeName(method.ReturnType, true)} {method.Name}{genericParams}(");

        var parameters = method.GetParameters();
        var paramStrings = parameters.Select(p =>
            $"{GetTypeName(p.ParameterType, true)} {p.Name}");

        writer.Write(string.Join(", ", paramStrings));
        writer.Write(")");

        writer.WriteLine();
        writer.Write(indent + "{");

        if (!method.IsAbstract)
        {
            writer.WriteLine();
            writer.Write(indent + "    ");

            if (method.ReturnType != typeof(void))
            {
                writer.WriteLine($"return default({GetTypeName(method.ReturnType, true)});");
            }
            else
            {
                writer.WriteLine();
            }

            writer.Write(indent);
        }
        else
        {
            writer.Write(" ");
        }

        writer.WriteLine("}");
    }

    private static void WriteProperty(StreamWriter writer, PropertyInfo property, int indentLevel)
    {
        var indent = new string(' ', indentLevel * 4);

        var getMethod = property.GetGetMethod(true);
        var setMethod = property.GetSetMethod(true);

        var accessModifier = "public";
        if (getMethod != null)
        {
            if (getMethod.IsPrivate)
            {
                accessModifier = "private";
            }
            else if (getMethod.IsFamily)
            {
                accessModifier = "protected";
            }
            else if (getMethod.IsAssembly)
            {
                accessModifier = "internal";
            }
        }

        var isStatic = getMethod?.IsStatic ?? setMethod?.IsStatic ?? false;

        var isVirtual = getMethod?.IsVirtual ?? false;
        var isAbstract = getMethod?.IsAbstract ?? false;

        writer.Write(indent + accessModifier + " ");
        if (isStatic)
        {
            writer.Write("static ");
        }

        if (isAbstract)
        {
            writer.Write("abstract ");
        }
        else if (isVirtual)
        {
            writer.Write("virtual ");
        }

        writer.WriteLine($"{GetTypeName(property.PropertyType, true)} {property.Name} {{ get; set; }}");
    }

    private static string GetTypeName(Type? type, bool withNamespace)
    {
        if (type == typeof(void))
        {
            return "void";
        }

        if (type == typeof(int))
        {
            return "int";
        }

        if (type == typeof(long))
        {
            return "long";
        }

        if (type == typeof(short))
        {
            return "short";
        }

        if (type == typeof(byte))
        {
            return "byte";
        }

        if (type == typeof(bool))
        {
            return "bool";
        }

        if (type == typeof(float))
        {
            return "float";
        }

        if (type == typeof(double))
        {
            return "double";
        }

        if (type == typeof(decimal))
        {
            return "decimal";
        }

        if (type == typeof(char))
        {
            return "char";
        }

        if (type == typeof(string))
        {
            return "string";
        }

        if (type == typeof(object))
        {
            return "object";
        }

        var name = withNamespace ? type?.Namespace + "." + type?.Name : type?.Name;
        if (type is { IsGenericType: true })
        {
            name = name?.Split('`')[0];

            var genericArgs = type.GetGenericArguments();
            return $"{name}<{string.Join(", ", genericArgs.Select(t => GetTypeName(t, withNamespace)))}>";
        }

        if (type is { IsGenericParameter: true })
        {
            return type.Name;
        }

        if (type is { IsArray: true })
        {
            return GetTypeName(type.GetElementType(), withNamespace) + "[]";
        }

        return withNamespace && !string.IsNullOrEmpty(type?.Namespace)
            ? type.Namespace + "." + type.Name
            : type!.Name;
    }

    private static string GetAccessModifier(FieldInfo field)
    {
        if (field.IsPublic)
        {
            return "public";
        }

        if (field.IsPrivate)
        {
            return "private";
        }

        if (field.IsFamily)
        {
            return "protected";
        }

        if (field.IsAssembly)
        {
            return "internal";
        }

        if (field.IsFamilyOrAssembly)
        {
            return "protected internal";
        }

        if (field.IsFamilyAndAssembly)
        {
            return "private protected";
        }

        return "private";
    }

    private static string GetAccessModifier(MethodInfo method)
    {
        if (method.IsPublic)
        {
            return "public";
        }

        if (method.IsPrivate)
        {
            return "private";
        }

        if (method.IsFamily)
        {
            return "protected";
        }

        if (method.IsAssembly)
        {
            return "internal";
        }

        if (method.IsFamilyOrAssembly)
        {
            return "protected internal";
        }

        if (method.IsFamilyAndAssembly)
        {
            return "private protected";
        }

        return "private";
    }

    private static string GetFieldSignature(FieldInfo field)
    {
        return $"{GetAccessModifier(field)} {(field.IsStatic ? "static " : string.Empty)}{field.FieldType.Name} {field.Name}";
    }

    private static string GetMethodSignature(MethodInfo method)
    {
        var parameters = method.GetParameters();
        var paramString = string.Join(", ", parameters.Select(p => $"{p.ParameterType.Name} {p.Name}"));

        return $"{GetAccessModifier(method)} {(method.IsStatic ? "static " : string.Empty)}{method.ReturnType.Name} {method.Name}({paramString})";
    }

    private static IEnumerable<Type> GetParameterTypes(MethodInfo method)
    {
        return method.GetParameters().Select(p => p.ParameterType);
    }
}