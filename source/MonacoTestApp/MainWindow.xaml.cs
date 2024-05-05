/***
 * IMPORTANT NOTES FROM MATTEO RISO (MR or ZipGenius on GitHub).
 * 
 * 2024-05-04
 * Successfully added "CopyTextToClipboard", "CutTextToClipboard" and "PasteTextFromClipboard" methods.
 * These methods can be called in code directly or attached to a context menu. Monaco's own context menu
 * is somewhat tricky and hard to translate to different languages for an app that is designed for an
 * international audience. I successfully added a native WinUI ContextFlyout to the editor control (see
 * MainWindow.xaml of this TestApp). That's the easiest way to have a native context menu that can be localized
 * as you would do for other controls in your application. The best discover is that the ContextFlyout
 * completely overtakes Monaco's own context menu, so when you right click in editor control, there
 * you will get Copy, Cut and Paste commands.
 * 
 * 2024-04-04
 * StickyScroll, ReadOnly, IsMiniMapVisible, LineNumbers properties were successfully added and they are
 * working as designed. Unfortunately CodeLens, AriaRequire, AriaLabel, AutoIndentStrategy are not working
 * and they must be reviewed.
 * 
 * 
 ***/

using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Monaco;
using System.Linq;
using System;
using Microsoft.UI.Xaml.Controls.Primitives;
using Windows.Storage.Pickers;
using Windows.Storage;
using WinRT.Interop;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using System.Runtime.InteropServices.ComTypes;

namespace MonacoTestApp;

public sealed partial class MainWindow : Window
{
    #region Internal use: retrieve version number of this test app
    /// Added 20240504 - MR
    public static string GetAppVersion()
    {
        Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        string displayableVersion = $"{version}";
        if (displayableVersion.Length < 15)
        {
            int c = 15 - displayableVersion.Length;
            for (int h = 0; h < c; h++)
                displayableVersion += " ";
        }
        return displayableVersion;
    }
    #endregion

    private readonly Dictionary<string, EditorThemes> _themes = new()
    {
        { "Visual Studio Light", EditorThemes.VisualStudioLight },
        { "Visual Studio Dark", EditorThemes.VisualStudioDark },        
        { "High Contast Dark", EditorThemes.HighContrastDark }
    };

    public MainWindow()
    {
        this.InitializeComponent();
        this.Title = "WinUI.Monaco Test App - version: " + GetAppVersion();
        this.Activated += MainWindow_Activated;
        
    }

    private async void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        

        // set languages
        // this.EditorLanguageComboBox.ItemsSource = (await MonacoEditor.GetLanguagesAsync()).Select(x => x.Id).ToList();

        // set theme
        this.ThemeSelectionComboBox.ItemsSource = _themes.Select(x => x.Key);

        /// Added 20240504 - MR
        /// The following lines were written to set Monaco theme at app startup
        /// because WinUI.Monaco control loads by default using the dark theme.
        /// Maybe in next commits there will be a property to set starting theme
        /// at control creation.
        while (!MonacoEditor.LoadCompleted)
        {
            await Task.Delay(25);
        }
        await MonacoEditor.SetThemeAsync(EditorThemes.VisualStudioLight);
        
        Clipboard.Clear(); // ONLY FOR DEBUGGING PURPOSES!
    }

    private void MonacoEditor_EditorTextSelected(object sender, TextSelectionArgs e) 
    {
        /// added 20240504 - MR
        /// this code tells when text is selected into editor. 
        /// Write here any logic that should be attached 
        /// to text selection.
        string selText = e.SelectedText;
        mnuCopy.IsEnabled = (!string.IsNullOrEmpty(selText));
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
            LogMessage("DevTools showing.");
        }
        catch (Exception err)
        {
            this.LogMessage("failed to open dev tools: " + err.Message);
        }
    }

    private void CheckBox_Checked(object sender, RoutedEventArgs e)
    {
        MonacoEditor.SetReadOnlyMessage(txtReadOnlyMessage.Text);
        MonacoEditor.ReadOnly(true);
        LogMessage("Editor set as read only.");
    }

    private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        MonacoEditor.ReadOnly(false);
    }

    private void cbMinimapVisible_Checked(object sender, RoutedEventArgs e)
    {
        MonacoEditor.IsMiniMapVisible(true);
    }

    private void cbMinimapVisible_Unchecked(object sender, RoutedEventArgs e)
    {
        MonacoEditor.IsMiniMapVisible(false);
    }

    private async void btnOpenFromFile_Click(object sender, RoutedEventArgs e)
    {
        FileOpenPicker fileOpenPicker = new()
        {
            ViewMode = PickerViewMode.Thumbnail,
            FileTypeFilter = { "*" },
        };

        nint windowHandle = WindowNative.GetWindowHandle(App.Window);
        InitializeWithWindow.Initialize(fileOpenPicker, windowHandle);

        StorageFile file = await fileOpenPicker.PickSingleFileAsync();
        if (file != null)
        {
            MonacoEditor.EditorTextSelected -= MonacoEditor_EditorTextSelected;
            await MonacoEditor.LoadFromFileAsync(file, true);
           
            /// Remarks: LoadFromFileAsync method relies on LoadContentAsync but it
            /// helps to make easier loading a file and it tries to guess what is
            /// the correct coding language to be set for a proper visualization.
            /// In next commits, I will implement also LoadFromStreamAsync method
            /// in order to give multiple option to end user.
            txtCodingLang.Text = "Recognized as: " + MonacoEditor.CurrentCodeLanguage;
            LogMessage("Loaded content into editor from " + file.Path);
            MonacoEditor.EditorTextSelected += MonacoEditor_EditorTextSelected;
        }
    }

    private void cbStickyScroll_Checked(object sender, RoutedEventArgs e)
    {
        // Added 20240404 - MR
        MonacoEditor.StickyScroll(true);
    }

    private void cbStickyScroll_Unchecked(object sender, RoutedEventArgs e)
    {
        // Added 20240404 - MR
        MonacoEditor.StickyScroll(false);
    }

    private void txtReadOnlyMessage_TextChanged(object sender, TextChangedEventArgs e)
    {
        string roDefaultMsg = "Can't edit in read only mode!";
        if (string.IsNullOrEmpty(txtReadOnlyMessage.Text))
            MonacoEditor.SetReadOnlyMessage(roDefaultMsg);
        else
            MonacoEditor.SetReadOnlyMessage(txtReadOnlyMessage.Text);
        LogMessage("ReadOnly message changed.");
    }

    #region Code not working
    //private void cbAriaLabelRequire_Checked(object sender, RoutedEventArgs e)
    //{
    //    // Added 20240404 - MR ## ! ## Not working, as of now.
    //    MonacoEditor.AriaRequired(true);
    //    MonacoEditor.AriaLabel(txtAriaLabel.Text);
    //}

    //private void cbAriaLabelRequire_Unchecked(object sender, RoutedEventArgs e)
    //{
    //    // Added 20240404 - MR ## ! ## Not working, as of now.
    //    MonacoEditor.AriaRequired(false);
    //}

    //private void txtAriaLabel_TextChanged(object sender, TextChangedEventArgs e)
    //{
    //    // Added 20240404 - MR ## ! ## Not working, as of now.
    //    MonacoEditor.AriaLabel(txtAriaLabel.Text);
    //}

    //private void cbAutoIndentStrategy_SelectionChanged(object sender, SelectionChangedEventArgs e)
    //{
    //    // Added 20240404 - MR ## ! ## Not working, as of now.
    //    MonacoEditor.AutoIndentStrategy(cbAutoIndentStrategy.SelectedItem.ToString());
    //    Debug.WriteLine(cbAutoIndentStrategy.SelectedItem.ToString());
    //}

    //private void cbCodeLens_Checked(object sender, RoutedEventArgs e)
    //{
    //    // Added 20240404 - MR ## ! ## Not working, as of now.
    //    MonacoEditor.CodeLens(true);
    //}

    //private void cbCodeLens_Unchecked(object sender, RoutedEventArgs e)
    //{
    //    // Added 20240404 - MR ## ! ## Not working, as of now.
    //    MonacoEditor.CodeLens(false);
    //} 
    #endregion

    private void cbFolding_Checked(object sender, RoutedEventArgs e)
    {
        // Added 20240404 - MR
        MonacoEditor.Folding(true);
    }

    private void cbFolding_Unchecked(object sender, RoutedEventArgs e)
    {
        // Added 20240404 - MR
        MonacoEditor.Folding(false);
    }

    private void cbLineNumbers_Checked(object sender, RoutedEventArgs e)
    {
        // Added 20240404 - MR
        MonacoEditor.LineNumbers(true);
    }

    private void cbLineNumbers_Unchecked(object sender, RoutedEventArgs e)
    {
        // Added 20240404 - MR
        MonacoEditor.LineNumbers(false);
    }

    private void cbWordWrapMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Added 20240404 - MR ## ! ## Not working, as of now.
        //switch (cbWordWrapMode.SelectedIndex)
        //{
        //    case 0:
        //        {
        //            MonacoEditor.WordWrap("on");
        //            break;
        //        }
        //    case 1:
        //        {
        //            MonacoEditor.WordWrap("off");
        //            break;
        //        }
        //    case 2:
        //        {
        //            MonacoEditor.WordWrap("wordWrapColumn");
        //            break;
        //        }
        //    case 3:
        //        {
        //            MonacoEditor.WordWrap("bounded");
        //            break;
        //        }
        //}
    }

    private void mnuCopy_Click(object sender, RoutedEventArgs e)
    {
        // Added 20240504 - MR 
        MonacoEditor.CopyTextToClipBoard();
        LogMessage("Text copied to clipboard.");
    }

    private void mnuCut_Click(object sender, RoutedEventArgs e)
    {
        // Added 20240504 - MR
        MonacoEditor.CutTextToClipBoard();
        LogMessage("Text cut to clipboard.");
    }

    private async void mnuSelctAll_Click(object sender, RoutedEventArgs e)
    {
        await MonacoEditor.SelectAllAsync();
    }

    private async void mnuPaste_Click(object sender, RoutedEventArgs e)
    {
        DataPackageView dataPackageView = Clipboard.GetContent();
        if (dataPackageView.Contains(StandardDataFormats.Text) is true)
        {
            string cpbText = await dataPackageView.GetTextAsync();
            MonacoEditor.PasteTextFromClipBoard(cpbText);
            LogMessage("Text pasted: " + cpbText);
        }
    }

    private void ctxMenu_Opening(object sender, object e)
    {
        mnuPaste.IsEnabled = ClipboardHasText();
    }

    private bool ClipboardHasText()
    {
        DataPackageView iData = Clipboard.GetContent();
        if (iData.Contains(StandardDataFormats.Html) ||
            iData.Contains(StandardDataFormats.Text) ||
            iData.Contains(StandardDataFormats.ApplicationLink) ||
            iData.Contains(StandardDataFormats.WebLink) ||
            iData.Contains(StandardDataFormats.Uri) ||
            iData.Contains(StandardDataFormats.Rtf) ||
            iData.Contains(StandardDataFormats.UserActivityJsonArray))
            return true;
        else
            return false;        
    }
}