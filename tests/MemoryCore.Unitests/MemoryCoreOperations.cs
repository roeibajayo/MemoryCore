namespace MemoryCore.Unitests;

public class MemoryCoreOperations
{
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
        Assert.True(!cache.Exists(key));
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
    public void Add_Stored()
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
    public void AddMinutes_Stored()
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
