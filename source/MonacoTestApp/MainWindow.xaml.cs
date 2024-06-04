using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Monaco;
using System.Linq;
using System;
using Windows.Storage.Pickers;
using Windows.Storage;
using WinRT.Interop;
using Monaco.MonacoHandler;
using System.IO;

namespace MonacoTestApp;

public sealed partial class MainWindow : Window
{
    private readonly Dictionary<string, EditorThemes> _themes = new()
    {
        { "Visual Studio Dark", EditorThemes.VisualStudioDark },
        { "Visual Studio Light", EditorThemes.VisualStudioLight },
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
        EditorThemes theme = _themes.First(x => x.Key == themeName).Value;
        _ = this.MonacoEditor.SetThemeAsync(theme);
        LogMessage("Theme set to " + themeName);
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
        EditorLanguageComboBox.IsEnabled = (selectionLanguages.Count > 0);
        LogMessage("Coding languages loaded.");
    }

    private void EditorLanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        string lang = (e.AddedItems.FirstOrDefault() as string);
        _ = this.MonacoEditor.SetLanguageAsync(lang);
        LogMessage($"Set language to {lang}");
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

    private void ContentIsReadOnlyCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        MonacoEditor.SetReadOnlyMessage(ReadOnlyMessageTextBox.Text);
        MonacoEditor.ReadOnly(true);
    }

    private void ContentIsReadOnlyCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        MonacoEditor.ReadOnly(false);
    }

    private void MinimapVisibleCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        MonacoEditor.IsMiniMapVisible(true);
    }

    private void MinimapVisibleCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        MonacoEditor.IsMiniMapVisible(false);
    }

    private async void OpenFileButton_Click(object sender, RoutedEventArgs e)
    {
        FileOpenPicker fileOpenPicker = new()
        {
            ViewMode = PickerViewMode.Thumbnail,
            FileTypeFilter = { "*" },
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        };

        nint windowHandle = WindowNative.GetWindowHandle(App.Window);
        InitializeWithWindow.Initialize(fileOpenPicker, windowHandle);

        StorageFile file = await fileOpenPicker.PickSingleFileAsync();
        if (file != null)
        {
            bool autodetect = this.AutoCodeTypeDetectionCheckBox.IsChecked ?? false;
            string fileContent = await FileIO.ReadTextAsync(file);

            if (autodetect)
            {
                MonacoFileRecognitionHandler handler = this.MonacoEditor.GetHandler<MonacoFileRecognitionHandler>();

                string fileCodeLanguage = handler.RecognizeLanguageByFileType(Path.GetExtension(file.Path));
                await this.MonacoEditor.SetLanguageAsync(fileCodeLanguage);
            }

            await MonacoEditor.LoadContentAsync(fileContent);

            /// Remarks: LoadFromFileAsync method relies on LoadContentAsync but it
            /// helps to make easier loading a file and it tries to guess what is
            /// the correct coding language to be set for a proper visualization.
            /// In next commits, I will implement also LoadFromStreamAsync method
            /// in order to give multiple option to end user.
            CodingLanguageTextBlock.Text = "Recognized as: " + MonacoEditor.CurrentCodeLanguage;
            LogMessage("Loaded content into editor from " + file.Path);
        }
    }

    private void StickyScrollCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        MonacoEditor.StickyScroll(true);
    }

    private void StickyScrollCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        MonacoEditor.StickyScroll(false);
    }

    private void ReadOnlyMessageTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        MonacoEditor.SetReadOnlyMessage(ReadOnlyMessageTextBox.Text);
    }

    private void AutoCodeTypeDetection_Checked(object sender, RoutedEventArgs e)
    {

    }
}