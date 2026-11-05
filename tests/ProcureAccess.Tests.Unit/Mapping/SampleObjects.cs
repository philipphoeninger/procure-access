// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Collections;
using System.Reflection;

namespace ProcureAccess.Tests.Unit.Mapping;

/// <summary>
/// Builds objects in which every settable public property holds a value that is not its type's
/// default: strings name their property, numbers count up, enums take their last value, collections
/// hold one such element and nested objects of the project are filled the same way (three levels).
/// Used by ARC-06 to see whether a map leaves a target member unset.
/// </summary>
internal static class SampleObjects
{
    private const int MaxDepth = 3;

    public static T Filled<T>() where T : new() => (T)Fill(typeof(T), depth: 0, counter: new Counter());

    /// <summary>Public, settable, non-indexer properties: the members a map has to set.</summary>
    public static IEnumerable<PropertyInfo> SettableProperties(Type type) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.CanWrite && property.SetMethod!.IsPublic && property.GetIndexParameters().Length == 0);

    /// <summary>True if the value is the default of its type, an empty string or a collection without a non-null element.</summary>
    public static bool IsUnset(object? value) => value switch
    {
        null => true,
        string text => text.Length == 0,
        IEnumerable sequence => !sequence.Cast<object?>().Any(element => element is not null),
        _ => value.GetType().IsValueType && value.Equals(Activator.CreateInstance(value.GetType())),
    };

    private static object Fill(Type type, int depth, Counter counter)
    {
        var instance = Activator.CreateInstance(type)!;
        foreach (var property in SettableProperties(type))
        {
            var value = SampleValue(property.PropertyType, property.Name, depth, counter);
            if (value is not null)
            {
                property.SetValue(instance, value);
            }
        }

        return instance;
    }

    private static object? SampleValue(Type type, string name, int depth, Counter counter)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        if (underlying == typeof(string)) return $"{name}-{counter.Next()}";
        if (underlying == typeof(bool)) return true;
        if (underlying == typeof(int)) return counter.Next();
        if (underlying == typeof(uint)) return (uint)counter.Next();
        if (underlying == typeof(long)) return (long)counter.Next();
        if (underlying == typeof(DateTime)) return new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(counter.Next());
        if (underlying == typeof(DateTimeOffset)) return new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero).AddDays(counter.Next());
        if (underlying.IsEnum) return Enum.GetValues(underlying).Cast<object>().Last();
        if (depth >= MaxDepth) return null;

        var elementType = CollectionElementType(underlying);
        if (elementType is not null)
        {
            var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType))!;
            var element = SampleValue(elementType, name, depth + 1, counter);
            if (element is not null)
            {
                list.Add(element);
            }

            return list;
        }

        return IsProjectType(underlying) && underlying.GetConstructor(Type.EmptyTypes) is not null
            ? Fill(underlying, depth + 1, counter)
            : null;
    }

    private static Type? CollectionElementType(Type type)
    {
        if (type == typeof(string) || !type.IsGenericType) return null;
        var definition = type.GetGenericTypeDefinition();
        return definition == typeof(List<>) || definition == typeof(ICollection<>) || definition == typeof(IEnumerable<>)
            ? type.GetGenericArguments()[0]
            : null;
    }

    private static bool IsProjectType(Type type) =>
        type.Namespace?.StartsWith("MODELS.ProcureAccess", StringComparison.Ordinal) == true && !type.IsAbstract;

    private sealed class Counter
    {
        private int _value;

        public int Next() => ++_value;
    }
}
