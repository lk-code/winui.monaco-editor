using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Monaco;
using System.Linq;
using System;
using MonacoTestApp.Extensions;
using Monaco.MonacoHandler;

namespace MonacoTestApp;

public sealed partial class MainWindow : Window
{
    private readonly Dictionary<string, string> _themes = new()
    {
        { "Visual Studio Light", "vs-light" },
        { "Visual Studio Dark", "vs-dark" },
        { "High Contast Dark", "hc-black" }
    };

    public MainWindow()
    {
        // register own additional monaco-editor handlers
        MonacoEditor.AdditionalMonacoHandlerTypes = new List<Type> {
            typeof(MonacoVersionHandler)
        };

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

    private void LogMessage(string message)
    {
        this.LoggingTextBox.Text += $"{DateTime.Now.ToShortTimeString()} - {message}" + Environment.NewLine;
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
        string theme = _themes.First(x => x.Key == themeName).Value;

        MonacoEditorThemeHandler handler = this.MonacoEditor.GetHandler<MonacoEditorThemeHandler>();

        _ = handler.SetThemeAsync(theme);
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
        MonacoEditorLanguageHandler handler = this.MonacoEditor.GetHandler<MonacoEditorLanguageHandler>();

        CodeLanguage[] languages = await handler.GetLanguagesAsync();
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
        this.LogMessage("Content in the editor has changed to: \n" + MonacoEditor.EditorContent);
    }

    private void MonacoEditor_MonacoEditorLoaded(object sender, EventArgs e)
    {
        this.LogMessage("Monaco Editor loaded");

    }

    private void OpenDevToolsButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            MonacoWebViewDevToolsHandler handler = this.MonacoEditor.GetHandler<MonacoWebViewDevToolsHandler>();

            handler.OpenDebugWebViewDeveloperTools();
        }
        catch (Exception err)
        {
            this.LogMessage("failed to open dev tools: " + err.Message);
        }
    }
}