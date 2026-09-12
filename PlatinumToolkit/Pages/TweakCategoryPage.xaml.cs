using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace PlatinumToolkit.Pages;

public sealed partial class TweakCategoryPage : Page
{
    public TweakCategoryPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        var category = e.Parameter as string ?? "";
        CategoryTitle.Text = category;

        var items = TweaksData.ForCategory(category).ToList();
        var (enabled, total) = TweaksData.CountsFor(category);
        CategorySubtitle.Text = total == 0
            ? "No tweaks configured for this category."
            : $"{enabled} of {total} enabled";

        TweaksList.ItemsSource = items;
        EmptyState.Visibility = total == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void Tweak_Toggled(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleSwitch toggle || toggle.DataContext is not TweakItem tweak)
        {
            return;
        }

        var ok = RegistryHelper.ApplyTweak(tweak, toggle.IsOn);
        if (!ok)
        {
            // Most likely cause: process isn't elevated - revert the visual
            // toggle state if the write silently failed.
            toggle.IsOn = !toggle.IsOn;
            return;
        }

        // Refresh the subtitle count and ask MainWindow to refresh nav badges.
        var (enabled, total) = TweaksData.CountsFor(tweak.Category);
        CategorySubtitle.Text = $"{enabled} of {total} enabled";
        MainWindow.Current?.RefreshNavBadges();
    }
}
