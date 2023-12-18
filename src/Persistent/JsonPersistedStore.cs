using System.Text.Encodings.Web;
using System.Text.Json;

namespace MemoryCore.Persistent;

internal class JsonPersistedStore : IPersistedStore
{
    private static readonly object LOCKER = new();

    private string GetPath(string name) =>
        Path.Combine(AppContext.BaseDirectory, $"{name}.json");

    public IEnumerable<PersistedEntry> GetAll(string name)
    {
        lock (LOCKER)
        {
            var path = GetPath(name);

            if (!File.Exists(path))
                yield break;

            var json = File.ReadAllText(path);

            if (string.IsNullOrWhiteSpace(json))
            {
                File.Delete(path);
                yield break;
            }

            JsonPersistedEntry[]? persistedEntries = null;
            try
            {
                persistedEntries = JsonSerializer.Deserialize<JsonPersistedEntry[]>(json)!;

                if (persistedEntries is null || persistedEntries.Length == 0)
                    throw new JsonException();
            }
            catch
            {
                File.Delete(path);
                yield break;
            }

            if (persistedEntries is null)
                yield break;

            foreach (var entry in persistedEntries)
            {
                if (entry is null)
                    continue;

                var jsonValue = (JsonElement)entry.Value!;
                var value = jsonValue.Deserialize(Type.GetType(entry.ValueType)!, new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                });
                entry.Value = value!;

                yield return entry;
            }
        }
    }
    public void Clear(string name)
    {
        lock (LOCKER)
        {
            var path = GetPath(name);

            if (!File.Exists(path))
                return;

            File.Delete(path);
        }
    }
    public void InsertOrReplace(string name, string key, StringComparison comparer, PersistedEntry entry)
    {
        lock (LOCKER)
        {
            var entries = GetAll(name).ToList();
            var currentEntry = entries.FindIndex(x => x.Key.Equals(key, comparer));
            if (currentEntry != -1)
                entries.RemoveAt(currentEntry);

            entries.Add(entry);
            Save(name, entries);
        }
    }
    public void Delete(string name, StringComparison comparer, IEnumerable<string> keys)
    {
        lock (LOCKER)
        {
            var entries = GetAll(name).ToList();

            foreach (var key in keys)
            {
                var currentEntry = entries.FindIndex(x => x.Key.Equals(key, comparer));
                if (currentEntry == -1)
                    continue;

                entries.RemoveAt(currentEntry);
            }

            Save(name, entries);
        }
    }

    private void Save(string name, IList<PersistedEntry> entries)
    {
        var path = GetPath(name);
        if (entries.Count == 0)
        {
            Clear(name);
            return;
        }

        var json = JsonSerializer.Serialize(entries.Select(entry => new JsonPersistedEntry
        {
            Key = entry.Key,
            Value = entry.Value,
            Tags = entry.Tags,
            AbsoluteExpiration = entry.AbsoluteExpiration,
            SlidingExpiration = entry.SlidingExpiration,
            ValueType = entry.Value.GetType().AssemblyQualifiedName!
        }));
        File.WriteAllText(path, json);
    }
}
