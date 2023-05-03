namespace MemoryCore.Unitests;

public class MemoryCoreOperations
{
    [Fact]
    public void AddWithTag_GetTags_TagExists()
    {
        //Arrange
        var key = "r1";
        var value = "ok";
        var tags = new[] { "tag1" };
        var minutes = 5;
        var cache = new MemoryCoreManager();

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
        var key = "r1";
        var value = "ok";
        var tags = new[] { "tag1" };
        var minutes = 5;
        var cache = new MemoryCoreManager();

        //Act
        cache.Add(key, value, TimeSpan.FromMinutes(minutes), tags);

        //Assert
        Assert.True(cache.ExistsTag(tags[0]));
    }

    [Fact]
    public void AddWithTag_RemoveTag_Removed()
    {
        //Arrange
        var key = "r1";
        var value = "ok";
        var tags = new[] { "tag1" };
        var minutes = 5;
        var cache = new MemoryCoreManager();

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
        var key = "r1";
        var value = "ok";
        var minutes = 5;
        var cache = new MemoryCoreManager();

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
        var key = "r1";
        var value = "ok";
        var minutes = 5;
        var cache = new MemoryCoreManager();

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
        var key = "r1";
        var value = "ok";
        var minutes = 5;
        var cache = new MemoryCoreManager();

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
        var key = "r1";
        var value = "ok";
        var minutes = 5;
        var cache = new MemoryCoreManager();

        //Act
        cache.AddSliding(key, value, TimeSpan.FromMinutes(minutes));

        //Assert
        Assert.True(cache.Exists(key));
    }

    [Fact]
    public void Add_Exists()
    {
        //Arrange
        var key = "r1";
        var value = "ok";
        var minutes = 5;
        var cache = new MemoryCoreManager();

        //Act
        cache.Add(key, value, TimeSpan.FromMinutes(minutes));

        //Assert
        Assert.True(cache.Exists(key));
    }

    [Fact]
    public void TryGetOrSet_OnlyFirstExecuted_RetrunValue()
    {
        //Arrange
        var key = "r1";
        var value = "ok";
        var minutes = 5;
        var cache = new MemoryCoreManager();
        var executions = 0;

        //Act
        cache.TryGetOrSet(key, () => { executions++; return value; }, TimeSpan.FromMinutes(minutes));
        cache.TryGetOrSet(key, () => { executions++; return value; }, TimeSpan.FromMinutes(minutes));

        //Assert
        Assert.Equal(1, executions);
        Assert.True(cache.Exists(key));
    }

    [Fact]
    public void WaitForExpired_NotFound()
    {
        //Arrange
        var key = "r1";
        var value = "ok";
        var minutes = 5;
        var cache = new MemoryCoreManager();

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
        var key = "r1";
        var value = "ok";
        var minutes = 5;
        var cache = new MemoryCoreManager();

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
        var cache = new MemoryCoreManager();

        //Act
        cache.Add(prefix + 1, value, minutes);
        cache.Add(prefix + 2, value, minutes);
        cache.RemoveByPrefix(prefix);

        //Assert
        Assert.Equal(0, cache.Count());
    }

    [Fact]
    public void Add_GetValue()
    {
        //Arrange
        var key = "r1";
        var value = "ok";
        var minutes = 5;
        var cache = new MemoryCoreManager();

        //Act
        cache.Add(key, value, TimeSpan.FromMinutes(minutes));

        //Assert
        Assert.Equal(cache.Get<string>(key), value);
    }

    [Fact]
    public void AddMinutes_GetValue()
    {
        //Arrange
        var key = "r1";
        var value = "ok";
        var minutes = 5;
        var cache = new MemoryCoreManager();

        //Act
        cache.Add(key, value, minutes);

        //Assert
        Assert.Equal(cache.Get<string>(key), value);
    }

    [Fact]
    public void ClearExpired_NoItems()
    {
        //Arrange
        var key = "r1";
        var value = "ok";
        var minutes = 5;
        var cache = new MemoryCoreManager();

        //Act
        cache.Add(key, value, minutes);
        cache.dateTimeOffsetProvider = new DateTimeOffsetProvider(TimeSpan.FromMinutes(minutes + 1));
        cache.ClearExpired();

        //Assert
        Assert.Empty(cache.entries);
    }
}
