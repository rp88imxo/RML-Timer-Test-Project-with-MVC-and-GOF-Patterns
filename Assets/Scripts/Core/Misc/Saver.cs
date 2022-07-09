using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace RML.Core
{
public abstract class Saver<T>
{
    private const int DEFAULT_SLOT = 0;
    private const int MAX_SAVES_COUNT = 10;

    protected abstract string DirectoryName { get; }
    protected abstract string FileName { get; }


    public void Save(T data, int slot = DEFAULT_SLOT)
    {
        var filepath = GetFilepath(slot);
        var serializedData = JsonConvert.SerializeObject(data);

        var file = new FileInfo(filepath);
        file.Directory?.Create();

        File.WriteAllText(file.FullName, serializedData);
    }

    public T Load(int slot = DEFAULT_SLOT)
    {
        if (!IsSaveExists(slot)) return default;

        var filepath = GetFilepath(slot);
        var data = File.ReadAllText(filepath);

        try
        {
            var deserializedData = JsonConvert.DeserializeObject<T>(data);
            return deserializedData;
        }
        catch (Exception e)
        {
            Debug.LogError(
                $"Unable to load {GetFullFileName(slot)}. Resetting...");
            Debug.LogException(e);
            Reset();
        }

        return default;
    }

    public bool IsSaveExists(int slot = DEFAULT_SLOT)
    {
        return File.Exists(GetFilepath(slot));
    }

    public void Reset()
    {
        for (int i = 0; i < MAX_SAVES_COUNT; i++)
        {
            var filepath = GetFilepath(i);

            if (!File.Exists(filepath)) continue;

            File.Delete(filepath);
        }
    }

    private string GetFullFileName(int slot)
        => $"{FileName}{slot}";

    private string GetFilepath(int slot)
    {
        var separator = Path.DirectorySeparatorChar;
        return
            $"{Application.persistentDataPath}{separator}Saves{separator}{DirectoryName}{separator}{GetFullFileName(slot)}";
    }
}
}