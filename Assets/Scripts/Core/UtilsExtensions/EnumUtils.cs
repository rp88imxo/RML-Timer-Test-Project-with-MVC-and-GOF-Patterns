using System;
using System.Collections.Generic;
using System.Linq;

namespace RML.Core
{
public static class EnumUtils
{
    public static int GetNamesCount<T>() 
        where T : Enum
    {
        return Enum.GetNames(typeof(T)).Length;
    }
    
    public static T[] GetAllValues<T>() where T : Enum
    {
        return (T[])Enum.GetValues(typeof(T));
    }

    public static T GetRandomValue<T>() where T : Enum
    {
        var values = Enum.GetValues(typeof(T));
        int index = UnityEngine.Random.Range(0, values.Length);
        return (T)values.GetValue(index);
    }

    public static T GetRandomValue<T>(T exclude) where T : Enum
    {
        var values = GetAllValues<T>().ToList();
        values.Remove(exclude);
        int index = UnityEngine.Random.Range(0, values.Count);
        return values[index];
    }
    
    public static T GetRandomValue<T>(params T[] excludes) where T : Enum
    {
        var values = GetAllValues<T>().ToList();
        foreach (var exclude in excludes)
            values.Remove(exclude);

        int index = UnityEngine.Random.Range(0, values.Count);
        return values[index];
    }

    public static int GetValuesCount<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T)).Length;
    }
    
    /// <typeparam name="TKey"> Any Enum </typeparam>
    /// <typeparam name="TValue"> Any type </typeparam>
    /// <returns> Dictionary filled with all enum values as a key, and
    /// default as a value</returns>
    public static Dictionary<TKey, TValue> GetFullDictionary<TKey,
        TValue>() where TKey : Enum
    {
        return GetAllValues<TKey>()
            .ToDictionary<TKey, TKey, TValue>(enumValue => enumValue,
                _ => default);
    }
    
    /// <typeparam name="TKey"> Any Enum </typeparam>
    /// <typeparam name="TValue"> Any object that can be created with new </typeparam>
    /// <returns> Dictionary filled with all enum values as a key, and
    /// new object as a value</returns>
    public static Dictionary<TKey, TValue> GetFullDictionaryWithNew<TKey,
        TValue>() where TKey : Enum where TValue : new()
    {
        return GetAllValues<TKey>()
            .ToDictionary(enumValue => enumValue,
                _ => new TValue());
    }
}
}


