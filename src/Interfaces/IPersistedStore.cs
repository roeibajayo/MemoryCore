﻿using MemoryCore.Persistent;

namespace MemoryCore;

public interface IPersistedStore
{
    IList<PersistedEntry> GetAll(string name);
    void InsertOrReplace(string name, string key, StringComparison comparer, PersistedEntry entry);
    void Delete(string name, StringComparison comparer, IEnumerable<string> keys);
    void Clear(string name);
}