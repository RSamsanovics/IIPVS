using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Ienemumu_Parvaldibas_Sistema.ViewModels;
using System;

namespace Ienemumu_Parvaldibas_Sistema.Views.Pages
{
    // Iestatijumu skats
    // Satur lietotaja konta darbibas (piem., konta dzesanu)
    public sealed class SettingsView : UserControl
    {
        // Lietotajvards, kuru dzest (nodots no AppWindow)
        private readonly string _username;

        // Konstruktors sanem lietotajvardu
        public SettingsView(string username)
        {
            _username = username;
            BuildUI();
        }

        // Uzbuuve UI elementiem (bez XAML)
        private void BuildUI()
        {
            // Galvenais vertikalais konteiners
            var panel = new StackPanel
            {
                Spacing = 12,
                Margin = new Thickness(20)
            };

            // Virsraksts
            panel.Children.Add(new TextBlock
            {
                Text = "Settings",
                FontSize = 22
            });

            // ================= DELETE ACCOUNT BUTTON =================

            // Poga konta dzesanai
            var deleteBtn = new Button
            {
                Content = "Delete account",
                Background = Brushes.DarkRed,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 20, 0, 0)
            };

            // Klikska notikuma piesaiste
            deleteBtn.Click += DeleteAccount_Click;

            panel.Children.Add(deleteBtn);

            // Iestata paneli ka skata saturu
            Content = panel;
        }

        // ================= DELETE ACCOUNT =================

        // Izsaucas, kad lietotajs nospiez "Delete account"
        // Atver apstiprinasanas dialogu
        private async void DeleteAccount_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Atrod augstako logu (owner)
            var topLevel = TopLevel.GetTopLevel(this);

            // Ja nav Window, iziet
            if (topLevel is not Window ownerWindow)
                return;

            // Apstiprinasanas dialogs
            var dialog = new Window
            {
                Title = "Confirm account deletion",
                Width = 360,
                Height = 180,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            // Dialoga saturs
            var dialogPanel = new StackPanel
            {
                Margin = new Thickness(15),
                Spacing = 12
            };

            // Bridinajuma teksts
            dialogPanel.Children.Add(new TextBlock
            {
                Text = "This will permanently delete your account and all data.\nThis action cannot be undone.",
                TextWrapping = TextWrapping.Wrap
            });

            // Pogas (Cancel / Delete)
            var buttonsRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 10
            };

            // Atcelsanas poga
            var cancelBtn = new Button { Content = "Cancel" };
            cancelBtn.Click += (_, _) => dialog.Close();

            // Apstiprinasanas poga
            var confirmBtn = new Button
            {
                Content = "Delete",
                Background = Brushes.DarkRed,
                Foreground = Brushes.White
            };

            confirmBtn.Click += (_, _) =>
            {
                // DZES LIETOTAJA DATU MAPES UN IERAKSTU users.json
                MainWindowViewModel.DeleteUserAccount(_username);

                // Aizver dialogu
                dialog.Close();

                // ATGRIEZAS LOGIN LOGA
                var login = new MainWindow();
                login.Show();
            };

            // Pievieno pogas rindai
            buttonsRow.Children.Add(cancelBtn);
            buttonsRow.Children.Add(confirmBtn);

            dialogPanel.Children.Add(buttonsRow);

            // Iestata dialoga saturu
            dialog.Content = dialogPanel;

            // Parada dialogu ka modal
            await dialog.ShowDialog(ownerWindow);
        }
    }
}
