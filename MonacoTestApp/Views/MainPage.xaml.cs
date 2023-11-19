using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MonacoTestApp.ViewModels;

namespace MonacoTestApp.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();

        this.Loaded += MainPage_Loaded;
    }

    private void MainPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        this.ViewModel.Initialize(this.MonacoEditor);
    }
}
