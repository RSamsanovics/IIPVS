using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Ienemumu_Parvaldibas_Sistema.Views.Pages;

// Skats (View), kas ļauj manuāli pievienot darījumu
// Raksta datus lietotāja normalizētajā CSV failā
public sealed class AddManualView : UserControl
{
    // Pašreizējais lietotājs (izmanto ceļa izveidei AppData)
    private readonly string _username;

    // Teksta lauks statusa / kļūdu paziņojumiem
    private TextBlock _infoText = null!;

    // Konstruktors saņem lietotājvārdu no ārpuses
    public AddManualView(string username)
    {
        _username = username;
        BuildUI();
    }

    // Uzbūvē visu UI programmatiski (bez XAML)
    private void BuildUI()
    {
        // Galvenais vertikālais konteiners
        var panel = new StackPanel
        {
            Spacing = 10,
            Margin = new Thickness(20)
        };

        // Virsraksts
        panel.Children.Add(new TextBlock
        {
            Text = "Add transaction manually",
            FontSize = 20
        });

        // =======================
        // DATE
        // =======================
        panel.Children.Add(new TextBlock { Text = "Date" });

        // Datuma izvēle (noklusēti – šodiena)
        var datePicker = new DatePicker
        {
            SelectedDate = DateTime.Today
        };
        panel.Children.Add(datePicker);

        // =======================
        // AMOUNT
        // =======================
        panel.Children.Add(new TextBlock { Text = "Amount" });

        // Summas ievades lauks
        var amountBox = new TextBox
        {
            Watermark = "e.g. 25.50"
        };
        panel.Children.Add(amountBox);

        // =======================
        // PARTNER
        // =======================
        panel.Children.Add(new TextBlock { Text = "Partner name" });

        // Partnera / veikala nosaukums
        var partnerBox = new TextBox
        {
            Watermark = "e.g. Rimi"
        };
        panel.Children.Add(partnerBox);

        // =======================
        // DIRECTION
        // =======================
        panel.Children.Add(new TextBlock { Text = "Direction" });

        // Darījuma virziens (In / Out)
        var directionBox = new ComboBox
        {
            ItemsSource = new[] { "In", "Out" },
            SelectedIndex = 0
        };
        panel.Children.Add(directionBox);

        // =======================
        // BUTTON
        // =======================
        var addBtn = new Button
        {
            Content = "Add transaction",
            Margin = new Thickness(0, 10, 0, 0)
        };
        panel.Children.Add(addBtn);

        // Statusa teksts (kļūdas vai apstiprinājums)
        _infoText = new TextBlock();
        panel.Children.Add(_infoText);

        // Pogas klikšķa apstrāde
        // Nolasa vērtības no UI elementiem un nodod loģikai
        addBtn.Click += (_, _) =>
        {
            AddTransaction(
                datePicker.SelectedDate?.DateTime,
                amountBox.Text,
                partnerBox.Text,
                directionBox.SelectedItem as string
            );
        };

        // Iestata šo paneli kā skata saturu
        Content = panel;
    }

    // ===================== LOGIC =====================

    // Pievieno darījumu CSV failam
    private void AddTransaction(
        DateTime? date,
        string? amountText,
        string? partner,
        string? direction)
    {
        // Pārbauda, vai visi lauki ir aizpildīti
        if (date == null ||
            string.IsNullOrWhiteSpace(amountText) ||
            string.IsNullOrWhiteSpace(partner) ||
            string.IsNullOrWhiteSpace(direction))
        {
            _infoText.Text = "All fields are required.";
            return;
        }

        // Mēģina parsēt summu (atbalsta , un .)
        if (!decimal.TryParse(
                amountText.Replace(",", "."),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var amount))
        {
            _infoText.Text = "Invalid amount.";
            return;
        }

        // Normalizē summas zīmi atkarībā no virziena
        if (direction == "Out")
            amount = -Math.Abs(amount);
        else
            amount = Math.Abs(amount);

        // Ceļš uz lietotāja normalizēto CSV failu AppData mapē
        var baseDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "IenemumuParvaldibasSistema",
            _username,
            "imports",
            "normalized");

        // Nodrošina, ka mape eksistē
        Directory.CreateDirectory(baseDir);

        var filePath = Path.Combine(baseDir, "UserNormalizedCSVData.csv");

        // Nosaka, vai jāraksta CSV header
        bool writeHeader = !File.Exists(filePath);

        // Atver failu papildināšanai
        using var w = new StreamWriter(filePath, append: true, Encoding.UTF8);

        // Ja fails ir jauns – uzraksta kolonnu nosaukumus
        if (writeHeader)
            w.WriteLine("Date;Amount;Partner;Direction");

        // Pievieno jaunu rindu CSV formātā
        w.WriteLine(
            $"{date.Value:dd.MM.yyyy};" +
            $"{amount.ToString(CultureInfo.InvariantCulture)};" +
            $"{partner.Trim()};" +
            $"{(amount < 0 ? "Out" : "In")}");

        // Apstiprinājuma paziņojums lietotājam
        _infoText.Text = "Transaction added successfully.";

        // Optional reset
        // amountBox.Text = "";
        // partnerBox.Text = "";
    }
}
