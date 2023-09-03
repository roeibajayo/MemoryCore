namespace MemoryCore.Persistent;

public interface IPersistedStore
{
    IEnumerable<PersistedEntry> GetAll(string name);
    void Insert(string name, PersistedEntry entry);
    void Delete(string name, IEnumerable<string> keys);
    void Clear(string name);
}