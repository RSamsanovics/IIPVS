using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Ienemumu_Parvaldibas_Sistema.Views.Pages;


// - CSV failu ielase
// - kolonnu kartesanu
// - datu priekssskatijumu
// - normalizeta CSV saglabasanu
public sealed class ImportView : UserControl
{
    // Lietotajvards (izmanto AppData cela veidosanai)
    private readonly string _username;

    // CSV datu priekskatijuma tabula
    private DataGrid _csvGrid = null!;

    // Teksts statusa un kludu paziņojumiem
    private TextBlock _infoText = null!;

    // Saraksts ar importetajiem CSV failiem
    private ListBox _filesList = null!;

    // Header rindas indeksa ievade (1-based)
    private TextBox _headerRowBox = null!;

    // ComboBox kolonnu piesaistei
    private ComboBox _dateCol = null!;
    private ComboBox _amountCol = null!;
    private ComboBox _partnerCol = null!;
    private ComboBox _directionCol = null!;

    // Lietotaja izveidota kolonnu kartesana
    private readonly CsvMapping _mapping = new();

    // Paslaik izveletais CSV fails
    private CsvFile? _selectedFile;

    // CSV dati ka rindas (dictionary forma)
    private List<Dictionary<string, string>> _rows = new();

    // Norada, vai tiek radits normalizetais priekskatijums
    private bool _showingNormalizedPreview = false;

    // Atslegvardu saraksti direction noteiksanai
    private string _incomingKeywords = "";
    private string _outgoingKeywords = "";

    // Maksimalais rindu skaits priekskatijumam
    private const int PreviewLimit = 100;

    // ======================= MODELS =======================

    // Ieksejs modelis CSV failu sarakstam
    private sealed class CsvFile
    {
        public string Name { get; init; } = "";
        public string Path { get; init; } = "";

        // Parada tikai faila nosaukumu ListBox
        public override string ToString() => Name;
    }

    // Kolonnu piesaistes konfiguracija
    private sealed class CsvMapping
    {
        public int HeaderRowIndex { get; set; } = 0;
        public int? DateCol { get; set; }
        public int? AmountCol { get; set; }
        public int? PartnerCol { get; set; }
        public int? DirectionCol { get; set; }
    }

    // Viena kolonnu izveles opcija ComboBox
    private sealed class ColumnOption
    {
        public string Name { get; init; } = "";
        public int? Index { get; init; }
    }

    // ======================= CONSTRUCTOR =======================

    // Konstruktors sanem lietotajvardu no augstaka limena
    public ImportView(string username)
    {
        _username = username;
        BuildUI();
    }

    // ======================= UI =======================

    // Programmiski izveido visu UI
    private void BuildUI()
    {
        // Saknes grid ar 2 kolonam
        var root = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,320"),
            RowDefinitions = new RowDefinitions("Auto,*")
        };

        // Statusa teksts
        _infoText = new TextBlock { Text = "No file selected." };
        root.Children.Add(_infoText);
        Grid.SetColumnSpan(_infoText, 2);

        // ===== PREVIEW GRID =====
        // CSV datu tabula
        _csvGrid = new DataGrid
        {
            IsReadOnly = true,
            AutoGenerateColumns = false,
            GridLinesVisibility = DataGridGridLinesVisibility.All,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        var previewHost = new Border
        {
            Child = _csvGrid,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        root.Children.Add(previewHost);
        Grid.SetRow(previewHost, 1);
        Grid.SetColumn(previewHost, 0);

        // ===== RIGHT PANEL =====
        // Vadibas panelis importesanai
        var panel = new StackPanel { Spacing = 8 };

        // Import un delete pogas
        var btnRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        var importBtn = new Button { Content = "Import CSV" };
        importBtn.Click += async (_, _) => await ImportCsvAndLoadPreview();
        btnRow.Children.Add(importBtn);

        var deleteBtn = new Button { Content = "Delete CSV" };
        deleteBtn.Click += (_, _) => DeleteSelectedCsv();
        btnRow.Children.Add(deleteBtn);

        panel.Children.Add(btnRow);

        // CSV failu saraksts
        _filesList = new ListBox { ItemsSource = LoadFiles(), Height = 150 };
        _filesList.SelectionChanged += (_, _) =>
        {
            _selectedFile = _filesList.SelectedItem as CsvFile;
            if (_selectedFile != null)
                LoadPreview(_selectedFile.Path);
        };
        panel.Children.Add(_filesList);

        panel.Children.Add(new Separator());

        // Header rindas izvele
        panel.Children.Add(new TextBlock { Text = "Header row (1-based):" });
        _headerRowBox = new TextBox { Text = "1" };
        _headerRowBox.LostFocus += (_, _) => ApplyHeaderRow();
        panel.Children.Add(_headerRowBox);

        // Kolonnu ComboBox inicializacija
        _dateCol = new ComboBox();
        _amountCol = new ComboBox();
        _partnerCol = new ComboBox();
        _directionCol = new ComboBox();

        panel.Children.Add(MakeCombo("Date", _dateCol));
        panel.Children.Add(MakeCombo("Amount", _amountCol));
        panel.Children.Add(MakeCombo("Partner", _partnerCol));
        panel.Children.Add(MakeCombo("Direction", _directionCol));

        // Atslegvardu ievade ienakumiem
        panel.Children.Add(new TextBlock { Text = "Incoming keywords" });
        var inBox = new TextBox();
        inBox.TextChanged += (_, _) =>
        {
            _incomingKeywords = inBox.Text ?? "";
            RebuildNormalizedPreview();
        };
        panel.Children.Add(inBox);

        // Atslegvardu ievade izdevumiem
        panel.Children.Add(new TextBlock { Text = "Outgoing keywords" });
        var outBox = new TextBox();
        outBox.TextChanged += (_, _) =>
        {
            _outgoingKeywords = outBox.Text ?? "";
            RebuildNormalizedPreview();
        };
        panel.Children.Add(outBox);

        // Normalizesanas poga
        var normalizeBtn = new Button { Content = "Normalize & Save" };
        normalizeBtn.Click += (_, _) => NormalizeAndSave();
        panel.Children.Add(normalizeBtn);

        var rightPanel = new ScrollViewer { Content = panel };
        root.Children.Add(rightPanel);
        Grid.SetRow(rightPanel, 1);
        Grid.SetColumn(rightPanel, 1);

        Content = root;
    }

    // Izveido ComboBox ar kolonnu piesaisti
    private Control MakeCombo(string label, ComboBox box)
    {
        var stack = new StackPanel();
        stack.Children.Add(new TextBlock { Text = label });

        // Kolonnas izveles apstrade
        box.SelectionChanged += (_, _) =>
        {
            if (box.SelectedItem is ColumnOption o)
            {
                if (label == "Date") _mapping.DateCol = o.Index;
                if (label == "Amount") _mapping.AmountCol = o.Index;
                if (label == "Partner") _mapping.PartnerCol = o.Index;
                if (label == "Direction") _mapping.DirectionCol = o.Index;

                // Pec katras izmainas parbuuve priekskatijumu
                RebuildNormalizedPreview();
            }
        };

        stack.Children.Add(box);
        return stack;
    }

    // ======================= FILE SYSTEM =======================

    // Pamatdirektorija lietotaja CSV failiem
    private string BaseDir =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "IenemumuParvaldibasSistema",
            _username,
            "imports");

    // Neapstradatie CSV faili
    private string RawDir => Directory.CreateDirectory(Path.Combine(BaseDir, "raw")).FullName;

    // Normalizetie CSV faili
    private string NormDir => Directory.CreateDirectory(Path.Combine(BaseDir, "normalized")).FullName;

    // Ielade visus raw CSV failus
    private List<CsvFile> LoadFiles() =>
        Directory.Exists(RawDir)
            ? Directory.GetFiles(RawDir, "*.csv")
                .Select(p => new CsvFile { Name = Path.GetFileName(p), Path = p })
                .ToList()
            : new();

    // ======================= IMPORT =======================

    // Importe CSV un ielade priekskatijumu
    private async System.Threading.Tasks.Task ImportCsvAndLoadPreview()
    {
        var top = TopLevel.GetTopLevel(this);
        if (top?.StorageProvider == null) return;

        // Atver faila izveles dialogu
        var result = await top.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            FileTypeFilter = new[] { new FilePickerFileType("CSV") { Patterns = new[] { "*.csv" } } }
        });

        if (result.Count == 0) return;

        // Nokope failu uz raw direktoriju
        var src = result[0].Path.LocalPath;
        var dst = Path.Combine(RawDir, Path.GetFileName(src));
        File.Copy(src, dst, true);

        // Atjauno failu sarakstu un priekskatijumu
        _filesList.ItemsSource = LoadFiles();
        _selectedFile = new CsvFile { Name = Path.GetFileName(dst), Path = dst };
        LoadPreview(dst);
    }

    // Dzesh izveleto CSV failu
    private void DeleteSelectedCsv()
    {
        if (_selectedFile == null) return;
        if (File.Exists(_selectedFile.Path)) File.Delete(_selectedFile.Path);

        // Notira UI
        _rows.Clear();
        _csvGrid.ItemsSource = null;
        _filesList.ItemsSource = LoadFiles();
    }

    // ======================= PREVIEW =======================

    // Ielade CSV priekskatijuma datus
    private void LoadPreview(string path)
    {
        _rows.Clear();

        using var sr = new StreamReader(path, Encoding.UTF8);
        var lines = new List<string>();

        // Nolasa tikai pirmas PreviewLimit rindas
        while (!sr.EndOfStream && lines.Count < PreviewLimit)
        {
            var l = sr.ReadLine();
            if (!string.IsNullOrWhiteSpace(l)) lines.Add(l);
        }

        if (lines.Count == 0) return;

        // Nosaka delimiteri
        char delim = lines[0].Contains(';') ? ';' : ',';

        var parsed = lines.Select(l => l.Split(delim)).ToList();
        int cols = parsed.Max(r => r.Length);

        // Parsed CSV -> dictionary forma
        _rows = parsed.Select(r =>
        {
            var d = new Dictionary<string, string>();
            for (int i = 0; i < cols; i++)
                d[$"Col{i}"] = i < r.Length ? r[i] : "";
            return d;
        }).ToList();

        ShowRawPreview();
        PopulateCombos();
        RebuildNormalizedPreview();
    }

    // Parada neapstradato CSV priekskatijumu
    private void ShowRawPreview()
    {
        if (_rows.Count == 0) return;

        _csvGrid.Columns.Clear();

        // Dinamiski izveido kolonnas
        for (int i = 0; i < _rows[0].Count; i++)
            _csvGrid.Columns.Add(new DataGridTextColumn
            {
                Header = $"Col {i + 1}",
                Binding = new Binding($"[Col{i}]")
            });

        _csvGrid.ItemsSource = null;
        _csvGrid.ItemsSource = _rows;
        _showingNormalizedPreview = false;
    }

    // Aizpilda ComboBox ar kolonnu nosaukumiem
    private void PopulateCombos()
    {
        var header = _rows[_mapping.HeaderRowIndex];
        var opts = header.Keys
            .Select((k, i) => new ColumnOption { Name = header[k], Index = i })
            .ToList();

        Bind(_dateCol, opts, false);
        Bind(_amountCol, opts, false);
        Bind(_partnerCol, opts, false);
        Bind(_directionCol, opts, true);
    }

    // Piesaista ComboBox ItemsSource
    private static void Bind(ComboBox box, List<ColumnOption> opts, bool allowNone)
    {
        box.ItemsSource = allowNone
            ? new[] { new ColumnOption { Name = "(None)", Index = null } }.Concat(opts).ToList()
            : opts;

        box.DisplayMemberBinding = new Binding(nameof(ColumnOption.Name));
    }

    // Pielieto header rindas izmainas
    private void ApplyHeaderRow()
    {
        if (!int.TryParse(_headerRowBox.Text, out var oneBased)) return;

        _mapping.HeaderRowIndex = Math.Max(0, oneBased - 1);
        PopulateCombos();
        RebuildNormalizedPreview();
    }

    // ======================= NORMALIZE =======================

    // Pargenerē normalizeto priekskatijumu
    private void RebuildNormalizedPreview()
    {
        if (_rows.Count == 0 ||
            _mapping.DateCol == null ||
            _mapping.AmountCol == null ||
            _mapping.PartnerCol == null)
        {
            ShowRawPreview();
            return;
        }

        var preview = new List<Dictionary<string, string>>();

        // Parskata katru datu rindu pec header
        for (int i = _mapping.HeaderRowIndex + 1; i < _rows.Count; i++)
        {
            var r = _rows[i];
            string Get(int c) => r.TryGetValue($"Col{c}", out var v) ? v : "";

            // Summa
            if (!decimal.TryParse(Get(_mapping.AmountCol.Value).Replace(",", "."),
                NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
                continue;

            // Direction teksts
            var dirText = _mapping.DirectionCol != null
                ? Get(_mapping.DirectionCol.Value).ToLowerInvariant()
                : "";

            // Nosaka izdevumus pec atslgvardiem
            if (_outgoingKeywords.Split(',').Any(k => dirText.Contains(k.Trim())))
                amount = -Math.Abs(amount);

            // Normalizeta rinda
            preview.Add(new Dictionary<string, string>
            {
                ["Date"] = Get(_mapping.DateCol.Value),
                ["Amount"] = amount.ToString(CultureInfo.InvariantCulture),
                ["Partner"] = Get(_mapping.PartnerCol.Value),
                ["Direction"] = amount < 0 ? "Out" : "In"
            });
        }

        ShowNormalizedPreview(preview);
    }

    // Parada normalizeto CSV priekskatijumu
    private void ShowNormalizedPreview(List<Dictionary<string, string>> rows)
    {
        _csvGrid.Columns.Clear();

        foreach (var h in new[] { "Date", "Amount", "Partner", "Direction" })
            _csvGrid.Columns.Add(new DataGridTextColumn
            {
                Header = h,
                Binding = new Binding($"[{h}]")
            });

        _csvGrid.ItemsSource = null;
        _csvGrid.ItemsSource = rows;
        _showingNormalizedPreview = true;
    }

    // Saglaba normalizeto CSV failu
    private void NormalizeAndSave()
    {
        if (_selectedFile == null || _rows.Count == 0)
        {
            _infoText.Text = "Nothing to normalize.";
            return;
        }

        var outFile = Path.Combine(NormDir, "UserNormalizedCSVData.csv");
        using var w = new StreamWriter(outFile, false, Encoding.UTF8);

        // CSV header
        w.WriteLine("Date;Amount;Partner;Direction");

        // Ieraksta visas rindas no normalizeta priekskatijuma
        foreach (var r in _csvGrid.ItemsSource as IEnumerable<Dictionary<string, string>> ?? [])
            w.WriteLine($"{r["Date"]};{r["Amount"]};{r["Partner"]};{r["Direction"]}");

        _infoText.Text = "CSV normalized successfully.";
    }
}
