using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Ienemumu_Parvaldibas_Sistema.AppModels;
using Ienemumu_Parvaldibas_Sistema.ViewModels;
using Ienemumu_Parvaldibas_Sistema.Views.Pages;

namespace Ienemumu_Parvaldibas_Sistema.Views.Pages;


// Visa reala logika atrodas HomeViewModel
public partial class HomeView : UserControl
{

    public HomeView()
    {
        InitializeComponent();
    }

    // Izsaucas, kad tiek nospiesta poga "Add Group"
    private void OnAddGroupClicked(object? sender, RoutedEventArgs e)
    {
        // Parbauda, vai DataContext ir HomeViewModel
        // Un izsauc AddGroup metodi
        if (DataContext is HomeViewModel vm)
            vm.AddGroup();
    }

    // Izsaucas, kad TextBox (keywords) zaude fokusu
    private void OnKeywordsLostFocus(object? sender, RoutedEventArgs e)
    {
        // sender  -> TextBox
        // DataContext TextBox limeni -> GroupDefinition
        // DataContext visa skata limeni -> HomeViewModel
        if (sender is TextBox tb &&
            tb.DataContext is GroupDefinition g &&
            DataContext is HomeViewModel vm)
        {
            // Atjauno keywords grupai un parbuuve grafiku
            vm.SetKeywordsText(g, tb.Text ?? "");
        }
    }


    // Dzesh konkretu grupu no saraksta
    private void OnRemoveGroupClicked(object? sender, RoutedEventArgs e)
    {
        // sender -> Button
        // Button.DataContext -> GroupDefinition
        // View.DataContext -> HomeViewModel
        if (sender is Button btn &&
            btn.DataContext is GroupDefinition g &&
            DataContext is HomeViewModel vm)
        {
            vm.RemoveGroup(g);
        }
    }
}
