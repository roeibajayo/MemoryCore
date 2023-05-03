using System.Text.RegularExpressions;

namespace MemoryCore;

public static class IMemoryCoreExtentionsTags
{
    public static void RemoveTags(this IMemoryCore cache, Regex pattren)
    {
        ArgumentNullException.ThrowIfNull(nameof(pattren));

        var keys = cache.GetTags().Where(tag => pattren.IsMatch(tag)).ToArray();
        foreach (var key in keys)
            cache.RemoveTag(key);
    }
    public static void RemoveTags(this IMemoryCore cache, IEnumerable<string> tags)
    {
        ArgumentNullException.ThrowIfNull(nameof(tags));

        foreach (string tag in tags)
            cache.RemoveTag(tag);
    }
    public static void RemoveTagsByPrefix(this IMemoryCore cache, string prefix)
    {
        ArgumentNullException.ThrowIfNull(nameof(prefix));

        var tags = cache.GetTags().Where(tag => tag.StartsWith(prefix)).ToArray();
        foreach (var tag in tags)
            cache.RemoveTag(tag);
    }
}
