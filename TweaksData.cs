namespace PlatinumToolkit;

/// <summary>
/// Single shared instance of the tweak list, loaded once at startup.
/// Pages (TweakCategoryPage) and the nav-badge counters both read from this
/// so a toggle flipped on one page is reflected everywhere immediately.
/// </summary>
public static class TweaksData
{
    public static List<TweakItem> All { get; } = RegistryHelper.BuiltInTweaks();

    public static void LoadCurrentState()
    {
        foreach (var tweak in All)
        {
            tweak.IsEnabled = RegistryHelper.IsTweakCurrentlyEnabled(tweak);
        }
    }

    public static IEnumerable<TweakItem> ForCategory(string category) =>
        All.Where(t => t.Category == category);

    public static (int enabled, int total) CountsFor(string category)
    {
        var items = ForCategory(category).ToList();
        return (items.Count(t => t.IsEnabled), items.Count);
    }
}
