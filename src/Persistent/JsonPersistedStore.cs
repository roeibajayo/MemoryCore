using System.Text.Encodings.Web;
using System.Text.Json;

namespace MemoryCore.Persistent;

internal class JsonPersistedStore : IPersistedStore
{
    private readonly IDateTimeOffsetProvider dateTimeOffsetProvider;

    public JsonPersistedStore(IDateTimeOffsetProvider dateTimeOffsetProvider)
    {
        this.dateTimeOffsetProvider = dateTimeOffsetProvider;
    }

    private string GetPath(string name) =>
        Path.Combine(AppContext.BaseDirectory, $"{name}.json");

    public IEnumerable<PersistedEntry> GetNotExpired(string name)
    {
        var path = GetPath(name);

        if (!File.Exists(path))
            yield break;

        var json = File.ReadAllText(path);
        var persistedEntries = JsonSerializer.Deserialize<JsonPersistedEntry[]>(json);
        if (persistedEntries is null)
            yield break;

        var notExpired = new List<PersistedEntry>();
        var nowOffset = dateTimeOffsetProvider.NowOffset;
        foreach (var entry in persistedEntries)
        {
            if (entry is null)
                continue;

            if (entry.AbsoluteExpiration is null || entry.AbsoluteExpiration.Value < nowOffset)
                continue;

            var jsonValue = (JsonElement)entry.Value!;
            var value = jsonValue.Deserialize(Type.GetType(entry.ValueType)!, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            });
            entry.Value = value!;

            notExpired.Add(entry);
            yield return entry;
        }

        if (notExpired.Count != persistedEntries.Length)
            Save(name, notExpired);
    }
    public void Save(string name, IEnumerable<PersistedEntry> entries)
    {
        var path = GetPath(name);
        if (!entries.Any())
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
    public void Clear(string name)
    {
        var path = GetPath(name);

        if (!File.Exists(path))
            return;

        File.Delete(path);
    }
}
