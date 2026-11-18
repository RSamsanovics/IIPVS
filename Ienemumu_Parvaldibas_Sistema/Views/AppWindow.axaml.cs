using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using DynamicData;
using DynamicData.Binding;
//using Ienemumu_Parvaldibas_Sistema.Pages;
using Ienemumu_Parvaldibas_Sistema.ViewModels;
using Ienemumu_Parvaldibas_Sistema.Views.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Ienemumu_Parvaldibas_Sistema.Views
{

    // - navigacija
    // - skatu ieladi MainContent zonai
    public partial class AppWindow : Window
    {
        // Visas navigacijas pogas
        // Izmantots, lai mainitu selected stilu
        private readonly List<Button> _navButtons;

        // Aktivais lietotajvards
        // Tiek padots uz apaksskatiem (Import, Manual, Settings)
        private readonly string _username;

        // Konstruktors sanem lietotajvardu no login loga
        public AppWindow(string Username)
        {
            InitializeComponent();

            // Atver logu pilnekrana rezima
            WindowState = WindowState.Maximized;

            // Saglaba lietotajvardu
            _username = Username;

            // Savac visas navigacijas pogas saraksta
            _navButtons = new()
            {
                HomeButton,
                ImportButton,
                ManuallyButton,
                ReportsButton,
                SettingsButton
            };

            // Mekle normalizeto CSV failu
            var normalizedCsvPath = GetNormalizedCsvPath();

            // Inicialais skats - Home
            var homeView = new HomeView();

            // Piesaista HomeViewModel ar CSV celu
            homeView.DataContext = new HomeViewModel(normalizedCsvPath);

            // Ielade HomeView galvenaja satura zona
            MainContent.Content = homeView;
        }

        // -------- Page selection --------
        // Paligmetode navigacijai ar selected stilu
        private void SelectPage(Button selected, Control content)
        {
            // Noòem selected klasi visam pogam
            foreach (var button in _navButtons)
            {
                button.Classes.Remove("selected");
            }

            // Pievieno selected klasi aktivajai pogai
            selected.Classes.Add("selected");

            // Ielade jauno skatu
            MainContent.Content = content;
        }

        // Zemak - testa / pagaidu skati (TextBlock)
        // Paðlaik nav izmantoti galvenaja plûsma

        private Control CreateManuallyView()
        {
            return new TextBlock
            {
                Text = $"Sveicinati manual {_username}!",
                FontSize = 24
            };
        }

        private Control CreateReportsView()
        {
            return new TextBlock
            {
                Text = $"Sveicinati reports {_username}!",
                FontSize = 24
            };
        }

        private Control CreateSettingsView()
        {
            return new TextBlock
            {
                Text = $"Sveicinati settings {_username}!",
                FontSize = 24
            };
        }

        // =======================
        // NAVIGATION EVENTS
        // =======================

        // Home poga
        private void Home_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Atjauno CSV celu (ja bija jauni dati)
            var normalizedCsvPath = GetNormalizedCsvPath();

            // Izveido jaunu HomeView instanci
            var homeView = new HomeView();

            // Piesaista HomeViewModel
            homeView.DataContext = new HomeViewModel(normalizedCsvPath);

            // Ielade Home skatu
            MainContent.Content = homeView;
        }

        // Import CSV poga
        private void Import_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Ielade ImportView ar lietotajvardu
            MainContent.Content = new ImportView(_username);
        }

        // Manual entry poga
        private void Manually_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Ielade AddManualView ar lietotajvardu
            MainContent.Content = new AddManualView(_username);
        }

        // Reports poga
        private void Reports_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Izmanto testa skatu
            SelectPage(ReportsButton, CreateReportsView());
        }

        // Settings poga
        private void Settings_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Ielade SettingsView ar lietotajvardu
            MainContent.Content = new SettingsView(_username);
        }

        // =======================
        // CSV PATH HELPERS
        // =======================

        // Atrod normalizeta CSV faila celu lietotajam
        private string GetNormalizedCsvPath()
        {
            var baseDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "IenemumuParvaldibasSistema",
                _username,
                "imports",
                "normalized"
            );

            var file = Path.Combine(baseDir, "UserNormalizedCSVData.csv");

            // Atgriez celu tikai, ja fails eksiste
            return File.Exists(file) ? file : string.Empty;
        }
    }
}
