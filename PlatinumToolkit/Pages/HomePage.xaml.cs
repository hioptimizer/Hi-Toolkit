using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace PlatinumToolkit.Pages;

public sealed partial class HomePage : Page
{
    private record CategorySummaryRow(string Name, string Counts);

    public HomePage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        GreetingText.Text = $"Hello, {Environment.UserName}";

        var categories = TweaksData.All.Select(t => t.Category).Distinct().OrderBy(c => c);
        var rows = categories.Select(c =>
        {
            var (enabled, total) = TweaksData.CountsFor(c);
            return new CategorySummaryRow(c, $"{enabled} / {total} enabled");
        }).ToList();

        CategorySummaryList.ItemsSource = rows;
    }
}
