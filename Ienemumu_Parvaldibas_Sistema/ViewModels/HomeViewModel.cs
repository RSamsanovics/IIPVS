using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using DynamicData;
using DynamicData;
using Ienemumu_Parvaldibas_Sistema.AppModels;
using Ienemumu_Parvaldibas_Sistema.AppModels;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ienemumu_Parvaldibas_Sistema.ViewModels
{
    public class HomeViewModel : INotifyPropertyChanged
    {
        // INotifyPropertyChanged implementacija
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        //definesana mainigajiem
        public string? Username { get; }
        public decimal IncomingThisMonth { get; } = 0;
        public decimal SpentThisMonth { get; } = 0;
        //public decimal Balance => IncomingThisMonth - SpentThisMonth;
        public decimal Balance { get; }

        public string WelcomeText =>
       string.IsNullOrEmpty(Username)
           ? "Sveicināti!"
           : $"Sveicināti, {Username}!";

        // FOR 1ST CHART
        public ISeries[] SpendingByPartnerSeries { get; private set; } = Array.Empty<ISeries>();
        public Axis[] SpendingByPartnerXAxes { get; private set; } = Array.Empty<Axis>();
        public Axis[] SpendingByPartnerYAxes { get; private set; } = Array.Empty<Axis>();


        //FOR 2ND CHART
        public ISeries[] SpendingByDaySeries { get; private set; } = Array.Empty<ISeries>();
        public Axis[] SpendingByDayXAxes { get; private set; } = Array.Empty<Axis>();
        public Axis[] SpendingByDayYAxes { get; private set; } = Array.Empty<Axis>();


        // FOR 3RD CHART
        public ISeries[] IncomeUsageSeries { get; private set; } = Array.Empty<ISeries>();
        public double IncomeUsagePercent { get; private set; }

        //for 4th CHART
        public ISeries[] IncomeVsSpentSeries { get; private set; } = Array.Empty<ISeries>();
        public Axis[] IncomeVsSpentXAxes { get; private set; } = Array.Empty<Axis>();
        public Axis[] IncomeVsSpentYAxes { get; private set; } = Array.Empty<Axis>();

        // KONTROLEEM UN CHART SELECTOR
        //shart selector enum
        private ChartId _activeChart = ChartId.None;
        public ChartId ActiveChart
        {
            get => _activeChart;
            set
            {
                if (_activeChart == value) return;
                _activeChart = value;

                OnPropertyChanged(nameof(ActiveChart));
                OnPropertyChanged(nameof(CurrentMonthRange));
                OnPropertyChanged(nameof(CurrentDayRange));
                OnPropertyChanged(nameof(ShowMonthRange));
                OnPropertyChanged(nameof(ShowDayRange));

                // rebuild ONLY the selected chart
                switch (_activeChart)
                {
                    case ChartId.SpendingByCategory:
                        BuildSpendingByCategoryChart();
                        break;

                    case ChartId.DailySpending:
                        BuildDailySpendingChart();
                        break;

                    case ChartId.IncomeUsage:
                        BuildIncomeUsageChart();
                        break;

                    case ChartId.IncomeVsSpent:
                        BuildIncomeVsSpentChart();
                        break;
                }
            }
        }

        //HELPERS PRIEKS SETTINGS VISIBILITY GRAFIKIEM
        public bool ShowMonthRange =>
            ActiveChart == ChartId.SpendingByCategory
            || ActiveChart == ChartId.IncomeUsage
            || ActiveChart == ChartId.IncomeVsSpent;

        public bool ShowDayRange =>
            ActiveChart == ChartId.DailySpending;


        // --- Time ranges per chart ---
        public MonthRange SpendingByCategoryRange { get; } = new MonthRange();
        public DayRange DailySpendingRange { get; } = new DayRange();
        public MonthRange IncomeUsageRange { get; } = new MonthRange();
        public MonthRange IncomeVsSpentRange { get; } = new MonthRange();


        // JO AVALONIA NESTRADA BEZ SITA, NELIETO <COMBOBOXITEM> MANUALI
        public Array ChartOptions => Enum.GetValues(typeof(ChartId));

        private Transaction[] _transactions = Array.Empty<Transaction>();

        //lai katram chard data rada savu uzstadijumu
        public MonthRange? CurrentMonthRange =>
            ActiveChart switch
            {
                    ChartId.SpendingByCategory => SpendingByCategoryRange,
                    ChartId.IncomeUsage => IncomeUsageRange,
                    ChartId.IncomeVsSpent => IncomeVsSpentRange,
                     _ => null
            };
        public DayRange? CurrentDayRange =>
            ActiveChart == ChartId.DailySpending
                ? DailySpendingRange
                : null;

        public ObservableCollection<GroupDefinition> Groups { get; }
    = new ObservableCollection<GroupDefinition>
{
    new GroupDefinition
    {
        Name = "Groceries",
        Keywords = new() { "rimi", "maxima", "lidl" }
    },
    new GroupDefinition
    {
        Name = "Fuel",
        Keywords = new() { "circle k", "viada" }
    }
};
        public HomeViewModel(string normalizedCsvPath)
        {         

            Debug.WriteLine($"HomeViewModel path: '{normalizedCsvPath}'");

            _transactions =
    !string.IsNullOrWhiteSpace(normalizedCsvPath) &&
    File.Exists(normalizedCsvPath)
        ? LoadTransactions(normalizedCsvPath)
        : Array.Empty<Transaction>();

            // var transactions = LoadTransactions(normalizedCsvPath);
            _transactions = LoadTransactions(normalizedCsvPath);
            var transactions = _transactions; // so your old code below still works for now


            BuildDailySpendingChart();


            //parbaude
            Debug.WriteLine($"Count={transactions.Count()}");

            if (transactions.Any())
            {
                Debug.WriteLine(
                    $"Min={transactions.Min(t => t.Date):yyyy-MM-dd}  " +
                    $"Max={transactions.Max(t => t.Date):yyyy-MM-dd}");
            }
            else
            {
                Debug.WriteLine("No transactions loaded.");
            }


            var topMonths = transactions
                .GroupBy(t => new { t.Date.Year, t.Date.Month })
                .OrderByDescending(g => g.Count())
                .Take(12);

            foreach (var g in topMonths)
                Debug.WriteLine($"{g.Key.Year}-{g.Key.Month:00} : {g.Count()} rows");

            //parbaude beigas

            var now = DateTime.Now;

            var monthTransactions = transactions
           .Where(t => t.Date.Year == now.Year &&
                       t.Date.Month == now.Month)
           .ToList();

            // nosaukums uz so bridi bet nem visus datumus
            IncomingThisMonth = transactions
                .Where(t => t.Direction == TransactionDirection.In)
                 .Sum(t => t.Amount);


            // nosaukums uz so bridi bet nem visus datumus
            SpentThisMonth = transactions
                .Where(t => t.Direction == TransactionDirection.Out)
                .Sum(t => Math.Abs(t.Amount));
           
            Balance = IncomingThisMonth - SpentThisMonth;

            //===================================================
            //-----------------------------------------KONTROLES  
            //===================================================
            var today = DateTime.Today;
            // prieks chart  1, 3 un 4
            SpendingByCategoryRange = new MonthRange
            {
                FromYear = today.Year,
                FromMonth = today.Month,
                ToYear = today.Year,
                ToMonth = today.Month
            };



            //prieks chart 2
            DailySpendingRange = new DayRange
            {
                FromYear = today.AddDays(-6).Year,
                FromMonth = today.AddDays(-6).Month,
                FromDay = today.AddDays(-6).Day,
                ToYear = today.Year,
                ToMonth = today.Month,
                ToDay = today.Day
            };

            // Chart 3 – current month
            IncomeUsageRange = new MonthRange
            {
                FromYear = now.Year,
                FromMonth = now.Month,
                ToYear = now.Year,
                ToMonth = now.Month
            };

            // Chart 4 – last 6 months
            var start = new DateTime(now.Year, now.Month, 1).AddMonths(-5);
            IncomeVsSpentRange = new MonthRange
            {
                FromYear = start.Year,
                FromMonth = start.Month,
                ToYear = now.Year,
                ToMonth = now.Month
            };

            // default values for controls
            SpendingByCategoryRange.FromYear = today.Year;
            SpendingByCategoryRange.FromMonth = today.Month;


            _transactions = LoadTransactions(normalizedCsvPath);
            BuildSpendingByCategoryChart(); // initial build

            // JA SpendingByCategoryRange IZMAINAS, JA IR AKTIVAIS TAD ATJAUNO GRAFIKU
            SpendingByCategoryRange.PropertyChanged += (_, __) =>
            {
                if (ActiveChart == ChartId.SpendingByCategory)
                    BuildSpendingByCategoryChart();
            };

            // JA DailySpendingRange IZMAINAS, JA IR AKTIVAIS TAD ATJAUNO GRAFIKU
            DailySpendingRange.PropertyChanged += (_, __) =>
            {
                if (ActiveChart == ChartId.DailySpending)
                    BuildDailySpendingChart();
            };
            // JA IncomeUsageRange IZMAINAS, JA IR AKTIVAIS TAD ATJAUNO GRAFIKU
            IncomeUsageRange.PropertyChanged += (_, __) =>
            {
                if (ActiveChart == ChartId.IncomeUsage)
                    BuildIncomeUsageChart();
            };
            // JA IncomeVsSpentRange IZMAINAS, JA IR AKTIVAIS TAD ATJAUNO GRAFIKU
            IncomeVsSpentRange.PropertyChanged += (_, __) =>
            {
                if (ActiveChart == ChartId.IncomeVsSpent)
                    BuildIncomeVsSpentChart();
            };


            Groups.CollectionChanged += (_, e) =>
            {
                if (e.NewItems != null)
                    foreach (GroupDefinition g in e.NewItems)
                        g.PropertyChanged += OnGroupChanged;

                if (e.OldItems != null)
                    foreach (GroupDefinition g in e.OldItems)
                        g.PropertyChanged -= OnGroupChanged;

                RebuildActiveChart();
            };


            // Charts are built via Build*Chart() methods.
            // Old inline constructor chart logic removed intentionally.

        }
        private decimal SumForGroup(IEnumerable<Transaction> transactions, string group)
        {
            var keywords = group
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(k => k.Trim().ToLowerInvariant())
                .Where(k => k.Length > 0)
                .ToList();

            if (keywords.Count == 0) return 0;

            return transactions
                .Where(t => t.Direction == TransactionDirection.Out)
                .Where(t => keywords.Any(k => (t.Partner ?? "").ToLowerInvariant().Contains(k)))
                .Sum(t => Math.Abs(t.Amount));
        }

        private static Transaction[] LoadTransactions(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return Array.Empty<Transaction>();


            var lines = File.ReadAllLines(path);

            if(lines.Length <= 1)
            {
                return Array.Empty<Transaction>();
            }
        
            return lines
           .Skip(1) // skip header
           .Select(ParseTransaction)
           .Where(t => t != null)
           .Cast<Transaction>()
           .ToArray();          
        }
        private static Transaction? ParseTransaction(string line)
        {
            var parts = line.Split(';');
            if (parts.Length < 4)
                return null;

            // FIX: exact date parsing (dd.MM.yyyy)
            if (!DateTime.TryParseExact(
                    parts[0].Trim(),
                    "dd.MM.yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var date))
                return null;

            if (!decimal.TryParse(
                    parts[1].Trim(),
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var amount))
                return null;

            var partner = parts[2].Trim();
            var directionText = parts[3].Trim();

            var direction = directionText.Equals("In", StringComparison.OrdinalIgnoreCase)
                ? TransactionDirection.In
                : TransactionDirection.Out;

            return new Transaction
            {
                Date = date,
                Amount = amount,
                Partner = partner,
                Direction = direction
            };
        }

        //Bulding FIRST CHART
        private void BuildSpendingByCategoryChart()
        {
            var from = new DateTime(
                SpendingByCategoryRange.FromYear,
                SpendingByCategoryRange.FromMonth,
                1);

            var to = new DateTime(
                SpendingByCategoryRange.ToYear,
                SpendingByCategoryRange.ToMonth,
                DateTime.DaysInMonth(
                    SpendingByCategoryRange.ToYear,
                    SpendingByCategoryRange.ToMonth));

            var data = _transactions
        .Where(t => t.Direction == TransactionDirection.Out)
        .Where(t => t.Date >= from && t.Date <= to)
        .GroupBy(t =>
            GroupingEngine.ResolveGroup(
                t.Partner ?? "",
                Groups))
        .Select(g => new
        {
            Group = g.Key,
            Amount = g.Sum(x => Math.Abs(x.Amount))
        })
        .OrderByDescending(x => x.Amount)
        .ToList();


            SpendingByPartnerSeries = new ISeries[]
 {
    new ColumnSeries<double>
    {
        Values = data.Select(x => (double)x.Amount).ToArray(),
        Name = "Spending"
    }
 };

            SpendingByPartnerXAxes = new[]
            {
    new Axis
    {
        Labels = data.Select(x => x.Group).ToArray(),
        LabelsRotation = 45
    }
};


            SpendingByPartnerYAxes = new[]
            {
        new Axis { Name = "EUR" }
    };

            OnPropertyChanged(nameof(SpendingByPartnerSeries));
            OnPropertyChanged(nameof(SpendingByPartnerXAxes));
            OnPropertyChanged(nameof(SpendingByPartnerYAxes));
        }

        //BUILDING SECOND CHART
        private void BuildDailySpendingChart()
        {
            var from = SafeDate(
                DailySpendingRange.FromYear,
                DailySpendingRange.FromMonth,
                DailySpendingRange.FromDay);

            var to = SafeDate(
                DailySpendingRange.ToYear,
                DailySpendingRange.ToMonth,
                DailySpendingRange.ToDay);

            var outgoing = _transactions
                .Where(t => t.Direction == TransactionDirection.Out)
                .Where(t => t.Date >= from && t.Date <= to)
                .ToList();

            var days = outgoing
                .Select(t => t.Date.Date)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

         
            var groupNames = outgoing
    .Select(t => GroupingEngine.ResolveGroup(t.Partner ?? "", Groups))
    .Distinct()
    .ToList();

            var series = new List<ISeries>();

            foreach (var group in groupNames)
            {
                var values = days.Select(day =>
                    outgoing
                        .Where(t =>
                            GroupingEngine.ResolveGroup(t.Partner ?? "", Groups) == group &&
                            t.Date.Date == day)
                        .Sum(t => (double)Math.Abs(t.Amount))
                ).ToArray();

                if (values.All(v => v == 0))
                    continue;

                series.Add(new StackedStepAreaSeries<double>
                {
                    Values = values,
                    Name = group,
                    Stroke = null
                });
            }

            SpendingByDaySeries = series.ToArray();

            SpendingByDayXAxes = new[]
            {
        new Axis
        {
            Labels = days.Select(d => d.ToString("dd.MM")).ToArray(),
            LabelsRotation = 80,
            TextSize = 10
        }
    };

            SpendingByDayYAxes = new[]
            {
        new Axis { Name = "EUR", TextSize = 10 }
    };

            OnPropertyChanged(nameof(SpendingByDaySeries));
            OnPropertyChanged(nameof(SpendingByDayXAxes));
            OnPropertyChanged(nameof(SpendingByDayYAxes));
        }

        //BUILDING THIRD CHART
        private void BuildIncomeUsageChart()
        {
            var from = new DateTime(
                IncomeUsageRange.FromYear,
                IncomeUsageRange.FromMonth,
                1);

            var to = new DateTime(
                IncomeUsageRange.ToYear,
                IncomeUsageRange.ToMonth,
                DateTime.DaysInMonth(
                    IncomeUsageRange.ToYear,
                    IncomeUsageRange.ToMonth));

            var incoming = _transactions
                .Where(t => t.Direction == TransactionDirection.In)
                .Where(t => t.Date >= from && t.Date <= to)
                .Sum(t => t.Amount);
 
            var spentByGroup = _transactions
           .Where(t => t.Direction == TransactionDirection.Out)
           .Where(t => t.Date >= from && t.Date <= to)
           .GroupBy(t => GroupingEngine.ResolveGroup(t.Partner ?? "", Groups))
           .Select(g => new
           {
               Group = g.Key,
               Amount = g.Sum(x => Math.Abs(x.Amount))
           })
           .ToList();

            var totalSpent = spentByGroup.Sum(x => x.Amount);
            var pieSeries = new List<ISeries>();
            foreach (var g in spentByGroup)
            {
                pieSeries.Add(new PieSeries<double>
                {
                    Values = new[] { (double)g.Amount },
                    Name = g.Group,
                    InnerRadius = 60
                });
            }

            var remaining = Math.Max(0, incoming - totalSpent);

            pieSeries.Add(new PieSeries<double>
            {
                Values = new[] { (double)remaining },
                Name = "Remaining",
                InnerRadius = 60
            });

            IncomeUsagePercent = incoming == 0
                ? 0
                : Math.Round((double)(totalSpent / incoming) * 100, 1);

            IncomeUsageSeries = pieSeries.ToArray();

            OnPropertyChanged(nameof(IncomeUsageSeries));
            OnPropertyChanged(nameof(IncomeUsagePercent));
        }

        //BUILDING FOURTH CHART
        private void BuildIncomeVsSpentChart()
        {
            var start = new DateTime(
                IncomeVsSpentRange.FromYear,
                IncomeVsSpentRange.FromMonth,
                1);

            var end = new DateTime(
                IncomeVsSpentRange.ToYear,
                IncomeVsSpentRange.ToMonth,
                1);

            var months = new List<DateTime>();
            for (var d = start; d <= end; d = d.AddMonths(1))
                months.Add(d);

            var income = months.Select(m =>
                _transactions
                    .Where(t => t.Direction == TransactionDirection.In)
                    .Where(t => t.Date.Year == m.Year && t.Date.Month == m.Month)
                    .Sum(t => (double)t.Amount)
            ).ToArray();

            var spent = months.Select(m =>
                _transactions
                    .Where(t => t.Direction == TransactionDirection.Out)
                    .Where(t => t.Date.Year == m.Year && t.Date.Month == m.Month)
                    .Sum(t => (double)Math.Abs(t.Amount))
            ).ToArray();

            IncomeVsSpentSeries = new ISeries[]
            {
        new LineSeries<double> { Values = income, Name = "Income" },
        new LineSeries<double> { Values = spent, Name = "Spent" }
            };

            IncomeVsSpentXAxes = new[]
            {
        new Axis
        {
            Labels = months.Select(m => m.ToString("MMM yyyy")).ToArray()
        }
    };

            IncomeVsSpentYAxes = new[]
            {
        new Axis { Name = "EUR" }
    };

            OnPropertyChanged(nameof(IncomeVsSpentSeries));
            OnPropertyChanged(nameof(IncomeVsSpentXAxes));
            OnPropertyChanged(nameof(IncomeVsSpentYAxes));
        }

        //HELPER FOR DATE PARSING
        private static DateTime SafeDate(int year, int month, int day)
        {
            year = Math.Clamp(year, 1, 9999);
            month = Math.Clamp(month, 1, 12);

            var maxDay = DateTime.DaysInMonth(year, month);
            day = Math.Clamp(day, 1, maxDay);

            return new DateTime(year, month, day);
        }
        //HELPER FOR KEYWORDS
        public string GetKeywordsText(GroupDefinition g)
    => string.Join(", ", g.Keywords);

        public void SetKeywordsText(GroupDefinition g, string text)
        {
            g.Keywords = text
                .Split(',', System.StringSplitOptions.RemoveEmptyEntries)
                .Select(k => k.Trim().ToLowerInvariant())
                .ToList();

            RebuildActiveChart();
        }
        private void RebuildActiveChart()
        {
            switch (ActiveChart)
            {
                case ChartId.SpendingByCategory:
                    BuildSpendingByCategoryChart();
                    break;
                case ChartId.DailySpending:
                    BuildDailySpendingChart();
                    break;
                case ChartId.IncomeUsage:
                    BuildIncomeUsageChart();
                    break;
            }
        }

        //ADD CATEGORY GROUP
        public void AddGroup()
        {
            Groups.Add(new GroupDefinition
            {
                Name = "New Group",
                Keywords = new()
            });

            RebuildActiveChart();
        }

        // REMOVE CATEGORY GROUP
        public void RemoveGroup(GroupDefinition group)
        {
            Groups.Remove(group);
            RebuildActiveChart();
        }
        private void OnGroupChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Any group change affects grouping → rebuild
            RebuildActiveChart();
        }

    }

}

