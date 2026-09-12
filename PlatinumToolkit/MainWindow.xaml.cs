using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PlatinumToolkit.Pages;

namespace PlatinumToolkit;

public sealed partial class MainWindow : Window
{
    /// <summary>
    /// Set in the constructor so pages (e.g. TweakCategoryPage after a toggle)
    /// can call back into MainWindow.Current.RefreshNavBadges() without needing
    /// a reference passed down through navigation.
    /// </summary>
    public static MainWindow? Current { get; private set; }

    public MainWindow()
    {
        InitializeComponent();
        Current = this;
        Title = "Platinum Toolkit";

        TweaksData.LoadCurrentState();
    }

    private void NavView_Loaded(object sender, RoutedEventArgs e)
    {
        RefreshNavBadges();

        // Select Home by default.
        if (NavView.MenuItems.Count > 0)
        {
            NavView.SelectedItem = NavView.MenuItems[0];
        }
    }

    /// <summary>
    /// Recomputes the "X/Y enabled" badges shown next to nav items that are
    /// backed by TweakCategoryPage. Call this after any tweak is toggled.
    /// </summary>
    public void RefreshNavBadges()
    {
        SetBadge(GeneralConfigBadge, "General Configuration");
        SetBadge(InterfaceTweaksBadge, "Interface Tweaks");
        SetBadge(AdvancedConfigBadge, "Advanced Configuration");
    }

    private static void SetBadge(InfoBadge badge, string category)
    {
        var (enabled, total) = TweaksData.CountsFor(category);
        if (total == 0)
        {
            badge.Visibility = Visibility.Collapsed;
            return;
        }
        badge.Visibility = Visibility.Visible;
        badge.Value = enabled;
        // InfoBadge.Value alone just shows a number; if you'd rather show
        // "enabled/total" like the mock-up, swap InfoBadge's built-in Value
        // for a custom template with a TextBlock bound to a formatted string.
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is not NavigationViewItem item || item.Tag is not string tag)
        {
            return;
        }

        if (tag.StartsWith("Category:"))
        {
            var category = tag["Category:".Length..];
            ContentFrame.Navigate(typeof(TweakCategoryPage), category);
            return;
        }

        Type pageType = tag switch
        {
            "Home" => typeof(HomePage),
            "AppFetch" => typeof(AppFetchPage),
            "PowerPlans" => typeof(PowerPlansPage),
            "UserAdjustments" => typeof(UserAdjustmentsPage),
            "Specs" => typeof(SpecsPage),
            "DiskCleanup" => typeof(DiskCleanupPage),
            "Troubleshooting" => typeof(TroubleshootingPage),
            "Settings" => typeof(SettingsPage),
            _ => typeof(HomePage)
        };

        ContentFrame.Navigate(pageType);
    }
}
