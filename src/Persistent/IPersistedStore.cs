namespace MemoryCore.Persistent;

public interface IPersistedStore
{
    IEnumerable<PersistedEntry> GetNotExpired(string name);
    void Save(string name, IEnumerable<PersistedEntry> entries);
    void Clear(string name);
}