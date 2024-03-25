using System.Text.RegularExpressions;

namespace MemoryCore;

public static class IMemoryCoreExtentionsTags
{
    /// <summary>
    /// Remove all items that match any tag that starts with <paramref name="prefix"/>.
    /// </summary>
    public static void RemoveTagsByPrefix(this IMemoryCore cache, string prefix)
    {
        if (prefix is null)
            throw new ArgumentNullException(nameof(prefix));

        var tags = cache.GetTags().Where(tag => tag.StartsWith(prefix));
        RemoveTags(cache, tags);
    }

    /// <summary>
    /// Remove all items that match any tag with <paramref name="pattren"/>.
    /// </summary>
    public static void RemoveTags(this IMemoryCore cache, Regex pattren)
    {
        if (pattren is null)
            throw new ArgumentNullException(nameof(pattren));

        var tags = cache.GetTags().Where(tag => pattren.IsMatch(tag));
        RemoveTags(cache, tags);
    }

    /// <summary>
    /// Remove all items that match any tag with <paramref name="tags"/>.
    /// </summary>
    public static void RemoveTags(this IMemoryCore cache, IEnumerable<string> tags)
    {
        if (tags is null)
            throw new ArgumentNullException(nameof(tags));

        foreach (string tag in tags)
            cache.RemoveTag(tag);
    }
}
