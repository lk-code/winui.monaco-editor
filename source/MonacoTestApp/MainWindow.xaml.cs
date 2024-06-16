using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Monaco;
using System.Linq;
using System;
using Windows.Storage.Pickers;
using Windows.Storage;
using WinRT.Interop;
using Windows.ApplicationModel.DataTransfer;

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
            this.MonacoEditor.OpenDebugWebViewDeveloperTools();
        }
        catch (Exception err)
        {
            this.LogMessage("failed to open dev tools: " + err.Message);
        }
    }

    private void ContentIsReadOnlyCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        MonacoEditor.SetReadOnlyMessage(txtReadOnlyMessage.Text);
        MonacoEditor.ReadOnly(true);
    }

    private void ContentIsReadOnlyCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        MonacoEditor.ReadOnly(false);
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
            await MonacoEditor.LoadFromFileAsync(file, true);
            /// Remarks: LoadFromFileAsync method relies on LoadContentAsync but it
            /// helps to make easier loading a file and it tries to guess what is
            /// the correct coding language to be set for a proper visualization.
            /// In next commits, I will implement also LoadFromStreamAsync method
            /// in order to give multiple option to end user.
            txtCodingLang.Text = "Recognized as: " + MonacoEditor.CurrentCodeLanguage;
            LogMessage("Loaded content into editor from " + file.Path);
        }
    }

    private void StickyScrollCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        MonacoEditor.EnableStickyScroll();
    }

    private void StickyScrollCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        MonacoEditor.EnableStickyScroll(false);
    }

    private void ReadOnlyMessageTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        MonacoEditor.SetReadOnlyMessage(txtReadOnlyMessage.Text);
    }

    private void mnuCopy_Click(object sender, RoutedEventArgs e)
    {
        MonacoEditor.CopyTextToClipBoard();
    }

    private void mnuCut_Click(object sender, RoutedEventArgs e)
    {
        MonacoEditor.CutTextToClipBoard();
    }

    private async void mnuPaste_Click(object sender, RoutedEventArgs e)
    {
        MonacoEditor.PasteTextFromClipBoard(await Clipboard.GetContent().GetTextAsync());
    }

    private void MonacoEditor_CursorPositionChanged(object sender, CursorPositionArgs e)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            txtCurCol.Text = e.mColumn.ToString();
            txtCurLine.Text = e.mLine.ToString();
        });
        
    }

    private async void mnuSelectAll_Click(object sender, RoutedEventArgs e)
    {
        await MonacoEditor.SelectAllAsync();
    }

    private void cbFolding_Checked(object sender, RoutedEventArgs e)
    {
        MonacoEditor.EnableFolding();
    }

    private void cbFolding_Unchecked(object sender, RoutedEventArgs e)
    {
        MonacoEditor.EnableFolding(false);
    }

    private void LineNumbersCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        MonacoEditor.EnableLineNumbers();
    }

    private void LineNumbersCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        MonacoEditor.EnableLineNumbers(false);
    }

    private void LineHighLightCheckBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedItem = LineHighLightCheckBox.SelectedItem;
        MonacoEditor.LineHighlight((selectedItem as ComboBoxItem).Tag.ToString());
    }

    private void EnableMapCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        MonacoEditor.EnableMiniMap();
    }

    private void EnableMapCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        MonacoEditor.EnableMiniMap(false);
    }

    private void RenderCharsCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        MonacoEditor.RenderMapCharacters();
    }

    private void RenderCharsCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        MonacoEditor.RenderMapCharacters(false);
    }

    private void MapSliderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedItem = MapSliderComboBox.SelectedItem;
        MonacoEditor.ShowMapSlider((selectedItem as ComboBoxItem).Tag.ToString());
        System.Diagnostics.Debug.WriteLine((selectedItem as ComboBoxItem).Tag.ToString());
    }

    private void MapSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedItem = MapSizeComboBox.SelectedItem;
        MonacoEditor.SetMapSize((selectedItem as ComboBoxItem).Tag.ToString());
        System.Diagnostics.Debug.WriteLine((selectedItem as ComboBoxItem).Tag.ToString());
    }

    private void MapAutoHideCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        MonacoEditor.EnableMapAutoHide(true);
    }

    private void MapAutoHideCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        MonacoEditor.EnableMapAutoHide(false);
    }

    private void MapSideComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedItem = MapSideComboBox.SelectedItem;
        MonacoEditor.SetMapSide((selectedItem as ComboBoxItem).Tag.ToString());
        System.Diagnostics.Debug.WriteLine((selectedItem as ComboBoxItem).Tag.ToString());
    }

    private void MapScaleNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        MonacoEditor.SetMapScale(Convert.ToInt32(args.NewValue));
    }

    private void MapMaxColumnNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        MonacoEditor.SetMapMaxColumn(Convert.ToInt32(args.NewValue));
    }

    private void MapSectionHeaderFontSizeNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        System.Diagnostics.Debug.WriteLine(Convert.ToInt32(args.NewValue).ToString());
        MonacoEditor.SetMapSectionHeaderFontSize(Convert.ToInt32(args.NewValue));
    }

    private void MapShowMarkHeaderComboBox_Checked(object sender, RoutedEventArgs e)
    {
        MonacoEditor.SetMapShowMarkSectionHeaders();
    }

    private void MapShowMarkHeaderComboBox_Unchecked(object sender, RoutedEventArgs e)
    {
        MonacoEditor.SetMapShowMarkSectionHeaders(false);
    }

    private void MapShowRegionHeaderComboBox_Checked(object sender, RoutedEventArgs e)
    {
        MonacoEditor.SetMapShowRegionSectionHeaders();
    }

    private void MapShowRegionHeaderComboBox_Unchecked(object sender, RoutedEventArgs e)
    {
        MonacoEditor.SetMapShowRegionSectionHeaders(false);
    }
}