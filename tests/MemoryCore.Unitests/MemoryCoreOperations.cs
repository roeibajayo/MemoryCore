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
    public void ComplexSenario()
    {
        var cache = new MemoryCoreManager();

        cache.Add("r1", "ok", 5);
        Assert.True(cache.Exists("r1"));
        Assert.True(!cache.Exists("r2"));
        Assert.True(cache.GetKeys().Count() == 1);
        Assert.True(cache.GetKeys().First() == "r1");

        var sec = cache.AddSecured("ok2", 5);
        Assert.True(cache.TryGetSecured<string>(sec, out _));
        Assert.True(!cache.TryGetSecured<string>(Guid.NewGuid(), out _));

        cache.Remove("r1");
        Assert.True(!cache.Exists("r1"));

        cache.RemoveSecured(sec);
        Assert.True(cache.GetKeys().Count() == 0);

        cache.Add("r1", "ok", 5);
        cache.Add("r2", "ok", 5);
        Assert.True(cache.GetKeys().Count() == 2);

        cache.RemoveByPrefix("r");
        Assert.True(cache.GetKeys().Count() == 0);
    }
}
