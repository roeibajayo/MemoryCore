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
        cache = new MemoryCoreManager();
        cache.dateTimeOffsetProvider = new DateTimeOffsetProvider(TimeSpan.FromMinutes(minutes + 1));

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
        cache.TryGetOrAdd(key, () => { executions++; return value; }, TimeSpan.FromMinutes(minutes));
        cache.TryGetOrAdd(key, () => { executions++; return value; }, TimeSpan.FromMinutes(minutes));

        //Assert
        Assert.Equal(1, executions);
        Assert.True(cache.Exists(key));
    }

    [Fact]
    public async Task TryGetOrAddAsyncParallel_OnlyFirstExecuted_RetrunValue()
    {
        //Arrange
        var key = "key";
        var value = "ok";
        var minutes = 5;
        using var cache = new MemoryCoreManager();
        var executions = 0;

        //Act
        var task1 = cache.TryGetOrAddAsync(key, async () => { await Task.Delay(300); executions++; return value; }, TimeSpan.FromMinutes(minutes));
        var task2 = cache.TryGetOrAddAsync(key, async () => { await Task.Delay(300); executions++; return value; }, TimeSpan.FromMinutes(minutes));
        await Task.WhenAll(task1, task2);

        //Assert
        Assert.Equal(1, executions);
        Assert.True(cache.Exists(key));
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
        Assert.True(cache.Exists(key));
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
        Assert.Equal(0, cache.Count());
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
        Assert.Equal(0, cache.Count());
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
