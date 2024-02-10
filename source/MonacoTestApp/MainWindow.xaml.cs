using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Monaco;
using System.Linq;
using System.Diagnostics;

namespace MonacoTestApp;

public sealed partial class MainWindow : Window
{
    private readonly Dictionary<string, EditorThemes> _themes = new()
    {
        { "Visual Studio Light", EditorThemes.VisualStudioLight },
        { "Visual Studio Dark", EditorThemes.VisualStudioDark },
        { "High Contast Dark", EditorThemes.HighContrastDark }
    };

    public MainWindow()
    {
        this.InitializeComponent();

        this.Activated += MainWindow_Activated;
    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        // set languages
        // this.EditorLanguageComboBox.ItemsSource = (await MonacoEditor.GetLanguagesAsync()).Select(x => x.Id).ToList();

        // set theme
        this.ThemeSelectionComboBox.ItemsSource = _themes.Select(x => x.Key);
    }

    private void ThemeSelectionComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        string themeName = (e.AddedItems.FirstOrDefault() as string);
        if (string.IsNullOrWhiteSpace(themeName))
        {
            return;
        }

        this.SetEditorTheme(themeName);
    }

    private void SetEditorTheme(string themeName)
    {
        EditorThemes theme = _themes.First(x => x.Key == themeName).Value;
        _ = this.MonacoEditor.SetThemeAsync(theme);
    }

    private void SetContentButton_Click(object sender, RoutedEventArgs e)
    {
        this.MonacoEditor.EditorContent = this.EditorContentTextBox.Text;
    }

    private void GetContentButton_Click(object sender, RoutedEventArgs e)
    {
        this.EditorContentTextBox.Text = MonacoEditor.EditorContent;
    }

    private async void LoadLanguagesButton_Click(object sender, RoutedEventArgs e)
    {
        CodeLanguage[] languages = await MonacoEditor.GetLanguagesAsync();
        List<string> selectionLanguages = languages.Select(x => x.Id).ToList();
        this.EditorLanguageComboBox.ItemsSource = selectionLanguages;
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

    private void MonacoEditor_EditorContentChanged(object sender, System.EventArgs e)
    {
        Debug.WriteLine("Content in the editor has changed to: \n" + MonacoEditor.EditorContent);
    }
}