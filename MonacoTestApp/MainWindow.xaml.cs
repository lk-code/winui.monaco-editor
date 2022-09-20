using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Monaco;
using System.Linq;

namespace MonacoTestApp
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            this.Activated += MainWindow_Activated;
        }

        private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            this.EditorLanguageComboBox.ItemsSource = EditorLanguages.GetLanguages();
        }

        private void LightThemeButton_Click(object sender, RoutedEventArgs e)
        {
            _ = this.MonacoEditor.SetThemeAsync(EditorThemes.VisualStudioLight);
        }

        private void DarkThemeButton_Click(object sender, RoutedEventArgs e)
        {
            _ = this.MonacoEditor.SetThemeAsync(EditorThemes.VisualStudioDark);
        }

        private void SetContentButton_Click(object sender, RoutedEventArgs e)
        {
            _ = this.MonacoEditor.LoadContentAsync(this.EditorContentTextBox.Text);
        }

        private void EditorLanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string lang = (e.AddedItems.FirstOrDefault() as string);
            _ = this.MonacoEditor.SetLanguageAsync(lang);
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            _ = this.MonacoEditor.SelectAllAsync();
        }
    }
}
