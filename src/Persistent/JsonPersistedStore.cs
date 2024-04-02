using System.Text.Encodings.Web;
using System.Text.Json;

namespace MemoryCore.Persistent;

internal class JsonPersistedStore : IPersistedStore
{
    private readonly object LOCKER = new();

    private string GetPath(string name) =>
        Path.Combine(AppContext.BaseDirectory, $"{name}.json");

    public IList<PersistedEntry> GetAll(string name)
    {
        lock (LOCKER)
        {
            var path = GetPath(name);

            if (!File.Exists(path))
                return [];

            var json = File.ReadAllText(path);

            if (string.IsNullOrWhiteSpace(json))
            {
                File.Delete(path);
                return [];
            }

            JsonPersistedEntry[]? persistedEntries = null;
            try
            {
                persistedEntries = JsonSerializer.Deserialize<JsonPersistedEntry[]>(json)!;
            }
            catch
            {
                File.Delete(path);
                return [];
            }

            if (persistedEntries is null or { Length: 0 })
                return [];

            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            var result = new List<PersistedEntry>(persistedEntries.Length);
            foreach (var entry in persistedEntries)
            {
                try
                {
                    var jsonValue = (JsonElement)entry.Value!;
                    var value = jsonValue.Deserialize(Type.GetType(entry.ValueType)!, options);
                    entry.Value = value!;
                }
                catch
                {
                    // Ignore invalid entries like corrupted or old or unsupported types
                    continue;
                }

                result.Add(entry);
            }

            return result;
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
            var entries = GetAll(name) as List<PersistedEntry>;
            var currentEntry = entries!.FindIndex(x => x.Key.Equals(key, comparer));
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
            var entries = GetAll(name) as List<PersistedEntry>;
            var save = false;

            foreach (var key in keys)
            {
                var currentEntry = entries!.FindIndex(x => x.Key.Equals(key, comparer));
                if (currentEntry == -1)
                    continue;

                save = true;
                entries.RemoveAt(currentEntry);
            }

            if (!save)
                return;

            Save(name, entries!);
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
            ValueType = GetSerializableType(entry.Value.GetType()).AssemblyQualifiedName!
        }));
        File.WriteAllText(path, json);
    }

    private static Type GetSerializableType(Type type)
    {
        if (type.IsArray || type.IsPrimitive || type.IsValueType || type.IsSerializable)
            return type;

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            return GetSerializableType(type.GetGenericArguments()[0]);

        var enumerableType = type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        if (enumerableType is not null)
        {
            var elementType = enumerableType.GetGenericArguments()[0];
            return elementType.MakeArrayType();
        }

        return type;
    }
}
