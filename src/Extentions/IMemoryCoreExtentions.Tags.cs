using System.Text.RegularExpressions;

namespace MemoryCore;

public static class IMemoryCoreExtentionsTags
{
    /// <summary>
    /// Remove all items that match any tag with <paramref name="pattren"/>.
    /// </summary>
    public static void RemoveTags(this IMemoryCore cache, Regex pattren)
    {
        ArgumentNullException.ThrowIfNull(nameof(pattren));

        var keys = cache.GetTags().Where(tag => pattren.IsMatch(tag));
        foreach (var key in keys)
            cache.RemoveTag(key);
    }

    /// <summary>
    /// Remove all items that match any tag with <paramref name="tags"/>.
    /// </summary>
    public static void RemoveTags(this IMemoryCore cache, IEnumerable<string> tags)
    {
        ArgumentNullException.ThrowIfNull(nameof(tags));

        foreach (string tag in tags)
            cache.RemoveTag(tag);
    }

    /// <summary>
    /// Remove all items that match any tag that starts with <paramref name="prefix"/>.
    /// </summary>
    public static void RemoveTagsByPrefix(this IMemoryCore cache, string prefix)
    {
        ArgumentNullException.ThrowIfNull(nameof(prefix));

        var tags = cache.GetTags().Where(tag => tag.StartsWith(prefix));
        foreach (var tag in tags)
            cache.RemoveTag(tag);
    }
}
