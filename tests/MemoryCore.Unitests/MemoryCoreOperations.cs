using System.Threading.Tasks;

namespace MemoryCore.Unitests;

public class MemoryCoreOperations
{
    [Fact]
    public void Persist_Delete_KeyNotExists()
    {
        //Arrange
        var key = "key";
        var value = "ok";
        var minutes = 5;
        var cache = new MemoryCoreManager();

        //Act
        cache.Add(key, value, TimeSpan.FromMinutes(minutes), persist: true);
        cache.Remove(key);
        var exists = cache.Exists(key);

        cache.Dispose();
        cache = new MemoryCoreManager();

        //Assert
        Assert.False(exists);
        Assert.False(cache.Exists(key));

        cache.Clear();
    }

    [Fact]
    public void Persist_DeleteByTag_Reload_KeyNotExists()
    {
        //Arrange
        var key = "key";
        var value = "ok";
        var tag = "tag";
        var minutes = 5;
        var cache = new MemoryCoreManager();

        //Act
        cache.Add(key, value, TimeSpan.FromMinutes(minutes), tags: [tag], persist: true);
        cache.RemoveTag(tag);
        var exists = cache.Exists(key);

        cache.Dispose();
        cache = new MemoryCoreManager();

        //Assert
        Assert.False(exists);
        Assert.False(cache.Exists(key));

        cache.Clear();
    }

    [Fact]
    public void Persist_Expired_ResetInstance_KeyNotExists()
    {
        //Arrange
        var key = "key";
        var value = "ok";
        var minutes = 5;
        var cache = new MemoryCoreManager();

        //Act
        cache.Add(key, value, TimeSpan.FromMinutes(minutes), persist: true);
        cache.Dispose();
        cache = new()
        {
            dateTimeOffsetProvider = new DateTimeOffsetProvider(TimeSpan.FromMinutes(minutes + 1))
        };

        //Assert
        Assert.False(cache.Exists(key));

        cache.Clear();
    }

    [Fact]
    public void Persist_Expired_KeyNotExists()
    {
        //Arrange
        var key = "key";
        var value = "ok";
        var minutes = 5;
        using var cache = new MemoryCoreManager();

        //Act
        cache.Add(key, value, TimeSpan.FromMinutes(minutes), persist: true);
        cache.dateTimeOffsetProvider = new DateTimeOffsetProvider(TimeSpan.FromMinutes(minutes + 1));

        //Assert
        Assert.False(cache.Exists(key));

        cache.Clear();
    }

    [Fact]
    public void Persist_ResetInstance_KeyExists()
    {
        //Arrange
        var key = "key";
        var value = "ok";
        var minutes = 5;
        var cache = new MemoryCoreManager();

        //Act
        cache.Add(key, value, TimeSpan.FromMinutes(minutes), persist: true);
        cache.Dispose();
        cache = new MemoryCoreManager();

        //Assert
        Assert.True(cache.Exists(key));

        cache.Clear();
    }

    [Fact]
    public void Persist_IEnumerable_ResetInstance_KeyExists()
    {
        //Arrange
        var key = "key";
        var value = Enumerable.Range(0, 5);
        var minutes = 5;
        var cache = new MemoryCoreManager();

        //Act
        cache.Add(key, value, TimeSpan.FromMinutes(minutes), persist: true);
        cache.Dispose();
        cache = new MemoryCoreManager();
        var actualValue = cache.Get<IEnumerable<int>>(key);

        //Assert
        Assert.True(cache.Exists(key));
        Assert.Equal(value, actualValue);

        cache.Clear();
    }

    [Fact]
    public void Persist_SecondIEnumerable_ResetInstance_KeyExists()
    {
        //Arrange
        var key = "key";
        var value = Enumerable.Range(0, 5).OrderBy(x => x);
        var minutes = 5;
        var cache = new MemoryCoreManager();

        //Act
        cache.Add(key, value, TimeSpan.FromMinutes(minutes), persist: true);
        cache.Dispose();
        cache = new MemoryCoreManager();
        var actualValue = cache.Get<IEnumerable<int>>(key);

        //Assert
        Assert.True(cache.Exists(key));
        Assert.Equal(value, actualValue);

        cache.Clear();
    }

    [Fact]
    public async Task PersistAsync_ResetInstance_KeyExists()
    {
        //Arrange
        var key = "key";
        var value = "ok";
        var minutes = 5;
        var cache = new MemoryCoreManager();
        var execution = 0;

        //Act
        await cache.TryGetOrAddAsync(key, async () => { execution++; await Task.Delay(100); return value; }, TimeSpan.FromMinutes(minutes), persist: true);
        cache = new MemoryCoreManager();
        var actualValue = await cache.TryGetOrAddAsync(key, async () => { execution++; await Task.Delay(100); return value; }, TimeSpan.FromMinutes(minutes), persist: true);

        //Assert
        Assert.True(cache.Exists(key));
        Assert.Equal(value, actualValue);
        Assert.Equal(1, execution);

        cache.Clear();
    }

    [Fact]
    public void AddWithTag_GetTags_TagExists()
    {
        //Arrange
        var key = "key";
        var value = "ok";
        var tags = new[] { "tag1" };
        var minutes = 5;
        using var cache = new MemoryCoreManager();

        //Act
        cache.Add(key, value, TimeSpan.FromMinutes(minutes), tags);

        //Assert
        Assert.Contains(tags[0], cache.GetTags());
        Assert.Single(cache.GetTags());
    }

    [Fact]
    public void AddWithTag_TagExists()
    {
        //Arrange
        var key = "key";
        var value = "ok";
        var tags = new[] { "tag1" };
        var minutes = 5;
        using var cache = new MemoryCoreManager();

        //Act
        cache.Add(key, value, TimeSpan.FromMinutes(minutes), tags);

        //Assert
        Assert.True(cache.ExistsTag(tags[0]));
    }

    [Fact]
    public void AddWithTag_RemoveTag_Removed()
    {
        //Arrange
        var key = "key";
        var value = "ok";
        var tags = new[] { "tag1" };
        var minutes = 5;
        using var cache = new MemoryCoreManager();

        //Act
        cache.Add(key, value, TimeSpan.FromMinutes(minutes), tags);
        cache.RemoveTag(tags[0]);

        //Assert
        Assert.False(cache.Exists(key));
    }

    [Fact]
    public void AddSliding_Touch_Expired_NotExists()
    {
        //Arrange
        var key = "key";
        var value = "ok";
        var minutes = 5;
        using var cache = new MemoryCoreManager();

        //Act
        cache.AddSliding(key, value, TimeSpan.FromMinutes(minutes));
        cache.dateTimeOffsetProvider = new DateTimeOffsetProvider(TimeSpan.FromMinutes(minutes / 2));
        cache.Get<string>(key);
        cache.dateTimeOffsetProvider = new DateTimeOffsetProvider(TimeSpan.FromMinutes((minutes / 2) + minutes + 1));

        //Assert
        Assert.False(cache.Exists(key));
    }

    [Fact]
    public void AddSliding_Touch_Exists()
    {
        //Arrange
        var key = "key";
        var value = "ok";
        var minutes = 5;
        using var cache = new MemoryCoreManager();

        //Act
        cache.AddSliding(key, value, TimeSpan.FromMinutes(minutes));
        cache.dateTimeOffsetProvider = new DateTimeOffsetProvider(TimeSpan.FromMinutes(minutes / 2));
        cache.Get<string>(key);
        cache.dateTimeOffsetProvider = new DateTimeOffsetProvider(TimeSpan.FromMinutes((minutes / 2) + 1));

        //Assert
        Assert.True(cache.Exists(key));
    }

    [Fact]
    public void AddSliding_Expired_NotExists()
    {
        //Arrange
        var key = "key";
        var value = "ok";
        var minutes = 5;
        using var cache = new MemoryCoreManager();

        //Act
        cache.AddSliding(key, value, TimeSpan.FromMinutes(minutes));
        cache.dateTimeOffsetProvider = new DateTimeOffsetProvider(TimeSpan.FromMinutes(minutes + 1));

        //Assert
        Assert.False(cache.Exists(key));
    }

    [Fact]
    public void AddSliding_Exists()
    {
        //Arrange
        var key = "key";
        var value = "ok";
        var minutes = 5;
        using var cache = new MemoryCoreManager();

        //Act
        cache.AddSliding(key, value, TimeSpan.FromMinutes(minutes));

        //Assert
        Assert.True(cache.Exists(key));
    }

    [Fact]
    public void Add_Exists()
    {
        //Arrange
        var key = "key";
        var value = "ok";
        var minutes = 5;
        using var cache = new MemoryCoreManager();

        //Act
        cache.Add(key, value, TimeSpan.FromMinutes(minutes));

        //Assert
        Assert.True(cache.Exists(key));
    }

    [Fact]
    public void TryGetOrAdd_OnlyFirstExecuted_RetrunValue()
    {
        //Arrange
        var key = "key";
        var value = "ok";
        var minutes = 5;
        using var cache = new MemoryCoreManager();
        var executions = 0;

        //Act
        var value1 = cache.TryGetOrAdd(key, () => { executions++; return value; }, TimeSpan.FromMinutes(minutes));
        var value2 = cache.TryGetOrAdd(key, () => { executions++; return value; }, TimeSpan.FromMinutes(minutes));

        //Assert
        Assert.Equal(1, executions);
        Assert.Equal(value, cache.Get<string>(key));
        Assert.Equal(value, value1);
        Assert.Equal(value, value2);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task TryGetOrAddAsyncParallel_OnlyFirstExecuted_RetrunValue(bool spread)
    {
        //Arrange
        var key = "key";
        var value = "ok";
        var minutes = 5;
        using var cache = new MemoryCoreManager();
        var tries = 30;
        var executions = 0;
        var random = new Random();

        //Act
        var tasks = Enumerable.Range(0, tries)
            .Select(async x =>
            {
                if (spread) await Task.Delay(random.Next(500));
                return await cache.TryGetOrAddAsync(key, async () =>
                {
                    await Task.Delay(random.Next(500));
                    executions++;
                    return value;
                }, TimeSpan.FromMinutes(minutes));
            })
            .ToArray();
        await Task.WhenAll(tasks);

        //Assert
        Assert.Equal(1, executions);
        Assert.Equal(value, cache.Get<string>(key));
        foreach (var task in tasks)
        {
            Assert.Equal(value, task.Result);
        }
    }

    [Fact]
    public async Task TryGetOrAddAsync_OnlyFirstExecuted_RetrunValue()
    {
        //Arrange
        var key = "key";
        var value = "ok";
        var minutes = 5;
        using var cache = new MemoryCoreManager();
        var executions = 0;

        //Act
        var value1 = await cache.TryGetOrAddAsync(key, async () => { await Task.Delay(300); executions++; return value; }, TimeSpan.FromMinutes(minutes));
        var value2 = await cache.TryGetOrAddAsync(key, async () => { await Task.Delay(300); executions++; return value; }, TimeSpan.FromMinutes(minutes));

        //Assert
        Assert.Equal(1, executions);
        Assert.Equal(value, cache.Get<string>(key));
        Assert.Equal(value, value1);
        Assert.Equal(value, value2);
    }

    [Fact]
    public void WaitForExpired_NotFound()
    {
        //Arrange
        var key = "key";
        var value = "ok";
        var minutes = 5;
        using var cache = new MemoryCoreManager();

        //Act
        cache.Add(key, value, TimeSpan.FromMinutes(minutes));
        cache.dateTimeOffsetProvider = new DateTimeOffsetProvider(TimeSpan.FromMinutes(minutes + 1));

        //Assert
        Assert.False(cache.Exists(key));
    }

    [Fact]
    public void Remove_Removed()
    {
        //Arrange
        var key = "key";
        var value = "ok";
        var minutes = 5;
        using var cache = new MemoryCoreManager();

        //Act
        cache.Add(key, value, TimeSpan.FromMinutes(minutes));
        cache.Remove(key);

        //Assert
        Assert.False(cache.Exists(key));
    }

    [Fact]
    public void RemoveByPrefix_Removed()
    {
        //Arrange
        var prefix = "r";
        var value = "ok";
        var minutes = 5;
        using var cache = new MemoryCoreManager();

        //Act
        cache.Add(prefix + 1, value, minutes);
        cache.Add(prefix + 2, value, minutes);
        cache.RemoveByPrefix(prefix);

        //Assert
        Assert.Equal(0, cache.Count);
    }

    [Fact]
    public void RemoveNotExists_NoError()
    {
        //Arrange
        var key = "not-exists";
        using var cache = new MemoryCoreManager();

        //Act
        cache.Remove(key);

        //Assert
        Assert.Equal(0, cache.Count);
    }

    [Fact]
    public void RemoveTag_NotExists_NoError()
    {
        //Arrange
        var tag = "not-exists";
        using var cache = new MemoryCoreManager();
        cache.Add("key", "value", 5);

        //Act
        cache.RemoveTag(tag);

        //Assert
        Assert.Equal(1, cache.Count);
    }

    [Fact]
    public async Task RemoveTag_Exists_NoError()
    {
        //Arrange
        var tag = "exists";
        using var cache = new MemoryCoreManager();
        cache.Add("key", "value", 5, tag); //will be deleted
        cache.TryGetOrAdd("key2", () => "value2", TimeSpan.FromSeconds(50), tags: [tag]); //will be deleted
        await cache.TryGetOrAddAsync("key3", () => Task.FromResult("value2"), TimeSpan.FromSeconds(50), tags: [tag]); //will be deleted
        cache.TryGetOrAdd<string?>("key4", () => null, TimeSpan.FromSeconds(50), tags: null); //will NOT be deleted
        cache.Add("key4", "value3", 5); //will NOT be deleted but key is already exists

        //Act
        cache.RemoveTag(tag);

        //Assert
        Assert.Equal(1, cache.Count);
    }

    [Fact]
    public void EmptyTag_Remove_NoError()
    {
        //Arrange
        using var cache = new MemoryCoreManager();
        cache.Add("key1", "value", 5, null!);
        cache.Add("key2", "value", 5, []);
        cache.Add("key3", "value", 5, [null!]);
        cache.Add("key4", "value", 5, [null!, string.Empty]);
        cache.Add("key5", string.Empty, 5);

        //Act
        cache.RemoveTag("fake-tag");

        //Assert
        Assert.Equal(5, cache.Count);
    }

    [Fact]
    public void NoKeys_Remove_NoError()
    {
        //Arrange
        using var cache = new MemoryCoreManager();

        //Act
        cache.Remove("fake-tag");

        //Assert
        Assert.Equal(0, cache.Count);
    }

    [Fact]
    public void NoKeys_RemoveTag_NoError()
    {
        //Arrange
        using var cache = new MemoryCoreManager();

        //Act
        cache.RemoveTag("fake-tag");

        //Assert
        Assert.Equal(0, cache.Count);
    }

    [Fact]
    public async Task Exception_RemoveTage_NoError()
    {
        //Arrange
        var tag = "exists";
        using var cache = new MemoryCoreManager();
        try
        {
            await cache.TryGetOrAddAsync<int>("key", () =>
            {
                throw new Exception();
            }, TimeSpan.FromSeconds(50), tags: [tag]);
        }
        catch { }

        //Act
        cache.RemoveTag(tag);

        //Assert
        Assert.Equal(0, cache.Count);
    }

    [Fact]
    public void Add_GetValue()
    {
        //Arrange
        var key = "key";
        var value = "ok";
        var minutes = 5;
        using var cache = new MemoryCoreManager();

        //Act
        cache.Add(key, value, TimeSpan.FromMinutes(minutes));

        //Assert
        Assert.Equal(cache.Get<string>(key), value);
    }

    [Fact]
    public void AddMinutes_GetValue()
    {
        //Arrange
        var key = "key";
        var value = "ok";
        var minutes = 5;
        using var cache = new MemoryCoreManager();

        //Act
        cache.Add(key, value, minutes);

        //Assert
        Assert.Equal(cache.Get<string>(key), value);
    }

    [Fact]
    public void ClearExpired_NoItems()
    {
        //Arrange
        var key = "key";
        var value = "ok";
        var minutes = 5;
        using var cache = new MemoryCoreManager();

        //Act
        cache.Add(key, value, minutes);
        cache.dateTimeOffsetProvider = new DateTimeOffsetProvider(TimeSpan.FromMinutes(minutes + 1));
        cache.ClearExpired();

        //Assert
        Assert.Empty(cache.entries);
    }
}
