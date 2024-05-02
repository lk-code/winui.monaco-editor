using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace Monaco;

public sealed partial class MonacoEditor : UserControl, IMonacoEditor
{
    private const string HTML_LAUNCH_FILE = @"monaco-editor\index.html";

    private string _content = "";
    private bool _hideminimap = false;
    private bool _readonly = false;
    public bool LoadCompleted { get; set; } = false;

    public event EventHandler? MonacoEditorLoaded = null;


    #region PropertyChanged Event

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    #region ContentChanged Event

    public event EventHandler? EditorContentChanged;

    private async void OnContentChanged()
    {
        _content = await GetEditorContentAsync();
        EditorContentChanged?.Invoke(this, new EventArgs());
    }

    #endregion

    #region EditorLanguage Property

    public static readonly DependencyProperty EditorLanguageProperty = DependencyProperty.Register("EditorLanguage",
        typeof(string),
        typeof(MonacoEditor),
        new PropertyMetadata(null));

    public string EditorLanguage
    {
        get
        {
            object editorLanguageProperty = GetValue(EditorLanguageProperty);
            return editorLanguageProperty == null ? "javascript" : (string)editorLanguageProperty;
        }
        set
        {
            SetValue(EditorLanguageProperty, value);
            OnPropertyChanged();

            _ = this.SetLanguageAsync(value);
        }
    }

    #endregion

    #region EditorContent Property

    public static readonly DependencyProperty EditorContentProperty = DependencyProperty.Register("EditorContent",
        typeof(string),
        typeof(MonacoEditor),
        new PropertyMetadata(null));

    /// <summary>
    /// Get and set the content of the editor.
    /// </summary>
    public string EditorContent
    {
        get
        {
            return _content;
        }
        set
        {
            SetValue(EditorContentProperty, value);
            OnPropertyChanged();

            _ = this.LoadContentAsync(value);
        }
    }

    #endregion

    #region HideMiniMap Property

    public static readonly DependencyProperty HideMiniMapProperty = DependencyProperty.Register("HideMiniMap",
        typeof(bool),
        typeof(MonacoEditor),
        new PropertyMetadata(null));

    /// <summary>
    /// Hides/shows the mini code map
    /// </summary>
    public bool EditorHideMiniMap
    {
        get
        {
            return _hideminimap;
        }
        set
        {
            SetValue(HideMiniMapProperty, value);
            OnPropertyChanged();

            this.HideMiniMap(value);
        }
    }

    #endregion

    #region ReadOnly Property

    public static readonly DependencyProperty ReadOnlyProperty = DependencyProperty.Register("ReadOnly",
        typeof(bool),
        typeof(MonacoEditor),
        new PropertyMetadata(null));

    /// <summary>
    /// Set the editor to readonly or not.
    /// </summary>
    public bool EditorReadOnly
    {
        get
        {
            return _readonly;
        }
        set
        {
            SetValue(ReadOnlyProperty, value);
            OnPropertyChanged();

            this.ReadOnly(value);
        }
    }

    #endregion

    #region SetReadOnlyMessage Property

    public static readonly DependencyProperty SetReadOnlyMessageProperty = DependencyProperty.Register("SetReadOnlyMessage",
        typeof(string),
        typeof(MonacoEditor),
        new PropertyMetadata(null));

    /// <summary>
    /// Set a custom message to tell user that the editor is in read only mode.
    /// </summary>
    public string EditorReadOnlyMessage
    {
        get
        {
            return _content;
        }
        set
        {
            SetValue(SetReadOnlyMessageProperty, value);
            OnPropertyChanged();

            this.SetReadOnlyMessage(value);
        }
    }

    #endregion

    #region Theme Property

    public static readonly DependencyProperty EditorThemeProperty = DependencyProperty.Register("EditorTheme",
        typeof(EditorThemes),
        typeof(MonacoEditor),
        new PropertyMetadata(null));

    public EditorThemes EditorTheme
    {
        get
        {
            object editorThemeProperty = GetValue(EditorThemeProperty);
            return editorThemeProperty == null ? EditorThemes.VisualStudioLight : (EditorThemes)GetValue(EditorThemeProperty);
        }
        set
        {
            SetValue(EditorThemeProperty, value);
            OnPropertyChanged();

            _ = this.SetThemeAsync(value);
        }
    }

    #endregion

    public MonacoEditor()
    {
        this.InitializeComponent();

        this.Loaded += MonacoEditorParentView_Loaded;

        MonacoEditorWebView.NavigationCompleted += WebView_NavigationCompleted;
    }

    private async void MonacoEditorParentView_Loaded(object sender, RoutedEventArgs e)
    {
        await MonacoEditorWebView.EnsureCoreWebView2Async();

        MonacoEditorWebView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

        // load launch html file
        string monacoHtmlFile = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, HTML_LAUNCH_FILE);
        this.MonacoEditorWebView.Source = new Uri(monacoHtmlFile);
    }

    private void CoreWebView2_WebMessageReceived(CoreWebView2 sender, CoreWebView2WebMessageReceivedEventArgs args)
    {
        string message = args.TryGetWebMessageAsString();

        this.ProcessMonacoEvents(message);
    }

    private void ProcessMonacoEvents(string monacoEvent)
    {
        switch (monacoEvent)
        {
            // own events
            case "EVENT_EDITOR_LOADED":
                {
                    MonacoEditorLoaded?.Invoke(this, EventArgs.Empty);
                }
                break;
            case "EVENT_EDITOR_CONTENT_CHANGED":
                {
                    OnContentChanged();
                }
                break;

            // monaco events
        }
    }

    private async void WebView_NavigationCompleted(object sender, object e)
    {
        LoadCompleted = true;
        _ = this.SetThemeAsync(this.EditorTheme);
        _ = this.SetLanguageAsync(this.EditorLanguage);

        string javaScriptContentChangedEventHandlerWebMessage = "window.editor.getModel().onDidChangeContent((event) => { handleWebViewMessage(\"EVENT_EDITOR_CONTENT_CHANGED\"); });";
        _ = await MonacoEditorWebView.ExecuteScriptAsync(javaScriptContentChangedEventHandlerWebMessage);

    }

    public void HideMiniMap(bool status=false)
    {
        string command = "";
        if (status)
            command = $"editor.updateOptions({{ minimap: {{ enabled: false }} }});";
        else
            command = $"editor.updateOptions({{ minimap: {{ enabled: true }} }});";
        this.MonacoEditorWebView.ExecuteScriptAsync(command);
    }

    public void ReadOnly(bool status = false)
    {
        string command = "";
        if (status)
            command = $"editor.updateOptions({{readOnly: true}});";
        else
            command = $"editor.updateOptions({{readOnly: false}});";
        this.MonacoEditorWebView.ExecuteScriptAsync(command);
    }

    public void SetReadOnlyMessage(string content)
    {
        string command = "";
        this._content = content;
        command = $"editor.updateOptions({{readOnlyMessage: {{value: '{content}'}} }});";
        
        this.MonacoEditorWebView.ExecuteScriptAsync(command);
    }

    /// <inheritdoc />
    public async Task LoadContentAsync(string content)
    {
        string ensuredContent = HttpUtility.JavaScriptStringEncode(content);

        this._content = ensuredContent;

        string command = $"editor.setValue('{ensuredContent}');";

        await this.MonacoEditorWebView
            .ExecuteScriptAsync(command);
    }

    /// <inheritdoc />
    public async Task<string> GetEditorContentAsync()
    {
        string command = $"editor.getValue();";

        string contentAsJsRepresentation = await this.MonacoEditorWebView.ExecuteScriptAsync(command);
        string unescapedString = System.Text.RegularExpressions.Regex.Unescape(contentAsJsRepresentation);
        string content = unescapedString.Substring(1, unescapedString.Length - 2).ReplaceLineEndings();

        return content;
    }

    /// <inheritdoc />
    public async Task SetThemeAsync(EditorThemes theme)
    {
        string themeValue = "vs-dark";

        switch (theme)
        {
            case EditorThemes.VisualStudioLight:
                {
                    themeValue = "vs-light";
                }
                break;
            case EditorThemes.VisualStudioDark:
                {
                    themeValue = "vs-dark";
                }
                break;
            case EditorThemes.HighContrastDark:
                {
                    themeValue = "hc-black";
                }
                break;
        }

        string command = $"editor._themeService.setTheme('{themeValue}');";

        await this.MonacoEditorWebView.ExecuteScriptAsync(command);
    }

    /// <inheritdoc />
    public async Task SelectAllAsync()
    {
        string command = $"editor.setSelection(editor.getModel().getFullModelRange());";

        await this.MonacoEditorWebView.ExecuteScriptAsync(command);
    }

    /// <inheritdoc />
    public async Task<CodeLanguage[]> GetLanguagesAsync()
    {
        string command = $"monaco.languages.getLanguages();";

        string languagesJson = await this.MonacoEditorWebView.ExecuteScriptAsync(command);

        CodeLanguage[] codeLanguages = JsonSerializer.Deserialize<CodeLanguage[]>(languagesJson);

        return codeLanguages;
    }

    /// <inheritdoc />
    public async Task SetLanguageAsync(string languageId)
    {
        string command = $"editor.setModel(monaco.editor.createModel(editor.getValue(), '{languageId}'));";

        await this.MonacoEditorWebView.ExecuteScriptAsync(command);

        // Reset the change content event
        string javaScriptContentChangedEventHandlerWebMessage = "window.editor.getModel().onDidChangeContent((event) => { handleWebViewMessage(\"EVENT_EDITOR_CONTENT_CHANGED\"); });";
        _ = await MonacoEditorWebView.ExecuteScriptAsync(javaScriptContentChangedEventHandlerWebMessage);
    }

    /// <inheritdoc />
    public void OpenDebugWebViewDeveloperTools()
    {
        MonacoEditorWebView.CoreWebView2.OpenDevToolsWindow();
    }
}