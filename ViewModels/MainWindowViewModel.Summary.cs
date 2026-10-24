using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using TimeTracker.Helpers;

namespace TimeTracker.ViewModels;

// Activities, summary cards and chart part of MainWindowViewModel (see MainWindowViewModel.cs)
public partial class MainWindowViewModel
{
    // Minimum and target percentages: below the minimum shows red, at the target green.
    // These become user settings in the Settings window phase.
    private const double BillableMinimum = 60;
    private const double BillableTarget = 80;
    private const double ProductiveMinimum = 70;
    private const double ProductiveTarget = 90;

    public ObservableCollection<ActivityRow> Activities { get; } = [];
    public ObservableCollection<SummaryCard> SummaryCards { get; } = [];
    public ObservableCollection<ChartBar> ChartBars { get; } = [];

    public IReadOnlyList<string> ChartKinds { get; } =
        ["Hours per project", "Amount per project", "Hours per task", "Hours per day"];

    [ObservableProperty]
    private ActivityRow? _selectedActivity;

    [ObservableProperty]
    private int _selectedChartIndex;

    [ObservableProperty]
    private string _chartTitle = string.Empty;

    [ObservableProperty]
    private bool _hasChartData;

    public string SelectedPeriodName => Periods.Names[SelectedPeriodIndex];

    partial void OnSelectedChartIndexChanged(int value) => UpdateChart();

    // Activities for the selected period, project and task; then cards and chart
    private void LoadActivities()
    {
        var selectedId = SelectedActivity?.Id ?? 0;
        var (from, to) = Periods.GetRange(SelectedPeriodIndex);  // "deconstructs" the tuple

        // The database stores UTC, so convert the local period boundaries
        var entries = _timeEntryRepository.GetCompleted(
            from.ToUniversalTime(), to.ToUniversalTime(),
            SelectedProject?.Id ?? 0, SelectedTask?.Id ?? 0);

        Activities.Clear();
        foreach (var entry in entries)
            Activities.Add(new ActivityRow(entry));
        SelectedActivity = Activities.FirstOrDefault(row => row.Id == selectedId);

        UpdateSummary();
        UpdateChart();
    }

    private void UpdateSummary()
    {
        var totalHours      = Activities.Sum(row => row.Hours);
        var billableHours   = Activities.Where(row => row.IsBillable).Sum(row => row.Hours);
        var productiveHours = Activities.Where(row => row.IsProductive).Sum(row => row.Hours);
        var amount          = Activities.Sum(row => row.Amount);
        var aiSavedHours    = Activities.Sum(row => row.AiTimeSavedHours);
        var count           = Activities.Count;

        SummaryCards.Clear();
        SummaryCards.Add(new SummaryCard(
            "Total time",
            $"{DisplayFormat.Hours(totalHours)} h",
            count == 1 ? "1 activity" : $"{count} activities"));
        SummaryCards.Add(PercentageCard("Billable time", billableHours, totalHours, BillableMinimum, BillableTarget));
        SummaryCards.Add(new SummaryCard(
            "Billable amount",
            DisplayFormat.Money(amount),
            billableHours > 0 ? $"{DisplayFormat.Money(amount / (decimal)billableHours)} per hour" : "No billable time"));
        SummaryCards.Add(PercentageCard("Productive time", productiveHours, totalHours, ProductiveMinimum, ProductiveTarget));
        SummaryCards.Add(new SummaryCard(
            "AI time saved",
            $"{DisplayFormat.Hours((double)aiSavedHours)} h",
            "For value-based billing"));
    }

    private static SummaryCard PercentageCard(string caption, double hours, double totalHours,
                                              double minimum, double target)
    {
        var percentage = totalHours > 0 ? hours / totalHours * 100 : 0;
        return new SummaryCard(
            caption,
            $"{DisplayFormat.Hours(hours)} h",
            $"{percentage:0}% of total",
            IsBelowMinimum: totalHours > 0 && percentage < minimum,
            IsOnTarget: totalHours > 0 && percentage >= target);
    }

    // Groups the current activities for the selected chart and builds one bar per group
    private void UpdateChart()
    {
        var isMoney = SelectedChartIndex == 1;

        // Each chart is a LINQ GroupBy; every branch yields (Label, Value) pairs
        IEnumerable<(string Label, double Value)> groups = SelectedChartIndex switch
        {
            1 => Activities.GroupBy(row => row.ProjectName)
                           .Select(group => (group.Key, (double)group.Sum(row => row.Amount))),
            2 => Activities.GroupBy(row => $"{row.ProjectName} · {row.TaskName}")
                           .Select(group => (group.Key, group.Sum(row => row.Hours))),
            3 => Activities.GroupBy(row => row.StartedAtLocal.Date)
                           .OrderBy(group => group.Key)  // chronological
                           .Select(group => (group.Key.ToString("ddd dd-MM"), group.Sum(row => row.Hours))),
            _ => Activities.GroupBy(row => row.ProjectName)
                           .Select(group => (group.Key, group.Sum(row => row.Hours)))
        };

        // Days stay in date order; the other charts show the largest first
        if (SelectedChartIndex != 3)
            groups = groups.OrderByDescending(group => group.Value);

        var bars = groups.ToList();
        var maximum = bars.Count > 0 ? bars.Max(bar => bar.Value) : 0;
        if (maximum <= 0) maximum = 1;  // avoid an empty 0..0 bar range

        ChartBars.Clear();
        foreach (var (label, value) in bars)
        {
            var valueText = isMoney ? DisplayFormat.Money((decimal)value) : $"{DisplayFormat.Hours(value)} h";
            ChartBars.Add(new ChartBar(label, value, maximum, valueText));
        }

        HasChartData = ChartBars.Count > 0;
        ChartTitle = $"{ChartKinds[SelectedChartIndex]} — {SelectedPeriodName}";
    }
}
