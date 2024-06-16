using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Windows.ApplicationModel.DataTransfer;

namespace Monaco;

public sealed partial class MonacoEditor : UserControl, IMonacoEditor, IMonacoCore
{
    private const string HTML_LAUNCH_FILE = @"monaco-editor\index.html";

    /// <summary>
    /// 
    /// </summary>
    public static IEnumerable<Type> AdditionalMonacoHandlerTypes = new List<Type>();
    /// <summary>
    /// 
    /// </summary>
    public event EventHandler? MonacoEditorLoaded = null;
    /// <summary>
    /// 
    /// </summary>
    public bool LoadCompleted { get; set; } = false;
    /// <summary>
    /// 
    /// </summary>
    public string CurrentCodeLanguage { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    private string _content = "";
    /// <summary>
    /// 
    /// </summary>
    private List<IMonacoHandler> _registeredHandlers = new();

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

    /// <inheritdoc />
    public void TriggerEditorContentChanged()
    {
        this.EditorContentChanged?.Invoke(this, new EventArgs());
    }

    /// <inheritdoc />
    public async Task LoadContentAsync(string content)
    {
        string ensuredContent = HttpUtility.JavaScriptStringEncode(content);
        this._content = ensuredContent;

        string command = $"editor.setValue('{ensuredContent}');";

        await this.MonacoEditorWebView.ExecuteScriptAsync(command);
    }

    /// <inheritdoc />
    public async Task<string> GetEditorContentAsync()
    {
        string command = $"editor.getValue();";

        string contentAsJsRepresentation = await this.MonacoEditorWebView!.ExecuteScriptAsync(command);
        string unescapedString = System.Text.RegularExpressions.Regex.Unescape(contentAsJsRepresentation);
        string content = unescapedString[1..^1].ReplaceLineEndings();

        return content;
    }

    #endregion

    #region ContextMenuEnabled Property

    public static readonly DependencyProperty EditorContextMenuEnabledProperty = DependencyProperty.Register("ContextMenuEnabled",
        typeof(bool),
        typeof(MonacoEditor),
        new PropertyMetadata(null));

    /// <summary>
    /// Set the editor to readonly or not.
    /// </summary>
    public bool EditorContextMenuEnabled
    {
        get
        {
            object editorContextMenuEnabled = GetValue(EditorContextMenuEnabledProperty);
            return EditorContextMenuEnabledProperty == null ? false : (bool)editorContextMenuEnabled;
        }
        set
        {
            SetValue(EditorContextMenuEnabledProperty, value);
            OnPropertyChanged();

            this.ContextMenuEnabled(value);
        }
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

    #region IsMiniMapVisible Property

    public static readonly DependencyProperty IsMiniMapVisibleProperty = DependencyProperty.Register("IsMiniMapVisible",
        typeof(bool),
        typeof(MonacoEditor),
        new PropertyMetadata(null));

    /// <summary>
    /// Hides/shows the mini code map
    /// </summary>
    public bool IsMiniMapVisible
    {
        get
        {
            object isMiniMapVisibleProperty = GetValue(IsMiniMapVisibleProperty);
            return isMiniMapVisibleProperty == null ? false : (bool)isMiniMapVisibleProperty;
        }
        set
        {
            SetValue(IsMiniMapVisibleProperty, value);
            OnPropertyChanged();

            this.SetEditorMiniMapVisible(value);
        }
    }

    #endregion

    #region EditorReadOnly Property

    public static readonly DependencyProperty EditorReadOnlyProperty = DependencyProperty.Register("EditorReadOnly",
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
            object editorReadOnlyProperty = GetValue(EditorReadOnlyProperty);
            return editorReadOnlyProperty == null ? false : (bool)editorReadOnlyProperty;
        }
        set
        {
            SetValue(EditorReadOnlyProperty, value);
            OnPropertyChanged();

            this.SetEditorReadOnly(value);
        }
    }

    #endregion

    #region EditorReadOnlyMessage Property

    public static readonly DependencyProperty EditorReadOnlyMessageProperty = DependencyProperty.Register("EditorReadOnlyMessage",
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
            object editorReadOnlyMessageProperty = GetValue(EditorReadOnlyMessageProperty);
            return editorReadOnlyMessageProperty == null ? string.Empty : (string)editorReadOnlyMessageProperty;
        }
        set
        {
            SetValue(EditorReadOnlyMessageProperty, value);
            OnPropertyChanged();

            this.SetReadOnlyMessage(value);
        }
    }

    #endregion

    #region IsStickyScrollEnabled Property

    public static readonly DependencyProperty IsStickyScrollEnabledProperty = DependencyProperty.Register("IsStickyScrollEnabled",
        typeof(bool),
        typeof(MonacoEditor),
        new PropertyMetadata(null));

    /// <summary>
    /// Set the editor to StickyScroll mode or not.
    /// </summary>
    public bool IsStickyScrollEnabled
    {
        get
        {
            object isStickyScrollEnabledProperty = GetValue(IsStickyScrollEnabledProperty);
            return isStickyScrollEnabledProperty == null ? false : (bool)isStickyScrollEnabledProperty;
        }
        set
        {
            SetValue(IsStickyScrollEnabledProperty, value);
            OnPropertyChanged();

            this.SetEditorStickyScroll(value);
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

    #region Editor Core

    public MonacoEditor()
    {
        this.InitializeComponent();

        this.Loaded += MonacoEditorParentView_Loaded;

        MonacoEditorWebView.NavigationCompleted += WebView_NavigationCompleted;
    }

    private async void MonacoEditorParentView_Loaded(object sender, RoutedEventArgs e)
    {
        /*
        // LOAD ALL FROM WHOLE APP
        var internalMonacoHandlerTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes());
        /* */

        // get all monaco handler types
        var internalMonacoHandlerTypes = Assembly.GetAssembly(typeof(MonacoEditor))!
            .GetTypes();
        var additionalMonacoHandlerTypes = MonacoEditor.AdditionalMonacoHandlerTypes;
        var monacoHandlerTypes = internalMonacoHandlerTypes.Concat(additionalMonacoHandlerTypes)
            .Where(p => typeof(IMonacoHandler).IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);

        this._registeredHandlers = monacoHandlerTypes
            .Select(x => (Activator.CreateInstance(x) as IMonacoHandler)!)
            .ToList();



        // first setup the parent instance (this control)
        this._registeredHandlers.ForEach(handler => handler.WithParentInstance(this));

        await MonacoEditorWebView.EnsureCoreWebView2Async();

        // then setup the webview after the EnsureCoreWebView2Async() call
        this._registeredHandlers.ForEach(handler => handler.WithWebView(this.MonacoEditorWebView));

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

    /// <inheritdoc />
    public T GetHandler<T>() where T : IMonacoHandler
    {
        T handler = (T)this._registeredHandlers.First(x => x.GetType() == typeof(T));

        if (handler is null)
        {
            throw new ArgumentNullException($"{nameof(handler)} is null. either because the handler was not registered or because the call was made too early.");
        }

        return handler;
    }

    private async void WebView_NavigationCompleted(object sender, object e)
    {
        LoadCompleted = true;
        _ = this.SetThemeAsync(this.EditorTheme);
        _ = this.SetLanguageAsync(this.EditorLanguage);

        await this.RegisterContentChangingEvent();
    }

    private async Task RegisterContentChangingEvent()
    {
        string javaScriptContentChangedEventHandlerWebMessage = "window.editor.getModel().onDidChangeContent((event) => { handleWebViewMessage(\"EVENT_EDITOR_CONTENT_CHANGED\"); });";
        _ = await MonacoEditorWebView.ExecuteScriptAsync(javaScriptContentChangedEventHandlerWebMessage);
    }

    #endregion

    #region Editor Theme Logic

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

        await this.SetThemeAsync(themeValue);
    }

    /// <inheritdoc />
    public async Task SetThemeAsync(string theme)
    {
        string command = $"editor._themeService.setTheme('{theme}');";

        await this.MonacoEditorWebView.ExecuteScriptAsync(command);
    }

    #endregion

    #region Editor Language

    /// <inheritdoc />
    public async Task<CodeLanguage[]> GetLanguagesAsync()
    {
        string command = $"monaco.languages.getLanguages();";

        string languagesJson = await this.MonacoEditorWebView.ExecuteScriptAsync(command);

        CodeLanguage[]? codeLanguages = JsonSerializer.Deserialize<CodeLanguage[]>(languagesJson);

        if (codeLanguages is null)
        {
            return Array.Empty<CodeLanguage>();
        }

        return codeLanguages;
    }

    /// <inheritdoc />
    public async Task SetLanguageAsync(string languageId)
    {
        CurrentCodeLanguage = languageId;
        string command = $"editor.setModel(monaco.editor.createModel(editor.getValue(), '{languageId}'));";

        await this.MonacoEditorWebView.ExecuteScriptAsync(command);

        // Reset the change content event
        await this.RegisterContentChangingEvent();
    }

    #endregion

    public void SetEditorMiniMapVisible(bool status = true)
    {
        if (status)
        {
            _ = this.MonacoEditorWebView.ExecuteScriptAsync($"editor.updateOptions({{ minimap: {{ enabled: true }} }});");
        }
        else
        {
            _ = this.MonacoEditorWebView.ExecuteScriptAsync($"editor.updateOptions({{ minimap: {{ enabled: false }} }});");
        }
    }

    public void SetEditorReadOnly(bool status = false)
    {
        if (status)
        {
            _ = this.MonacoEditorWebView.ExecuteScriptAsync($"editor.updateOptions({{readOnly: true}});");
        }
        else
        {
            _ = this.MonacoEditorWebView.ExecuteScriptAsync($"editor.updateOptions({{readOnly: false}});");
        }
    }

    public void SetReadOnlyMessage(string content)
    {
        _ = this.MonacoEditorWebView.ExecuteScriptAsync($"editor.updateOptions({{readOnlyMessage: {{value: '{content}'}} }});");
    }

    public void SetEditorStickyScroll(bool status = true)
    {
        if (status)
        {
            _ = this.MonacoEditorWebView.ExecuteScriptAsync($"editor.updateOptions({{ stickyScroll: {{ enabled: true }} }});");
        }
        else
        {
            _ = this.MonacoEditorWebView.ExecuteScriptAsync($"editor.updateOptions({{ stickyScroll: {{ enabled: false }} }});");
        }
    }

    /// <inheritdoc />
    public async Task SelectAllAsync()
    {
        string command = $"editor.setSelection(editor.getModel().getFullModelRange());";

        await this.MonacoEditorWebView.ExecuteScriptAsync(command);
    }

    /// <summary>
    /// Copy text to clipboard
    /// </summary>
    public async void CopyTextToClipBoard()
    {
        string rawText = await MonacoEditorWebView.ExecuteScriptAsync("editor.getModel().getValueInRange(editor.getSelection());");
        string selectedText = System.Text.RegularExpressions.Regex.Unescape(rawText).TrimStart('"').TrimEnd('"');
        DataPackage dataPackage = new DataPackage();
        dataPackage.SetText(selectedText);
        Clipboard.SetContent(dataPackage);
    }

    /// <summary>
    /// Cut text to clipboard
    /// </summary>
    public async void CutTextToClipBoard()
    {

        string rawText = await MonacoEditorWebView.ExecuteScriptAsync("editor.getModel().getValueInRange(editor.getSelection());");
        string selectedText = System.Text.RegularExpressions.Regex.Unescape(rawText).TrimStart('"').TrimEnd('"');

        string command = "var selection = editor.getSelection();";
        command += "var id = { major: 1, minor: 1 }; ";
        command += "var text=''; ";
        command += "var op= {identifier:id, range: selection, text: text, forceMoveMarkers: true}; ";
        command += "editor.executeEdits(\"my-source\", [op]);";

        await MonacoEditorWebView.ExecuteScriptAsync(command);
        DataPackage dataPackage = new DataPackage();
        dataPackage.SetText(selectedText);
        Clipboard.SetContent(dataPackage);
    }

    /// <summary>
    /// Paste text from clipboard
    /// </summary>
    public async void PasteTextFromClipBoard(string cpbText)
    {
        if (string.IsNullOrEmpty(cpbText))
            return;
        // Let's build the complex string holding
        // the Javascript code to handle pasting into Monaco.
        // The code checks if some text is selected in order
        // to paste and replace text or just paste the new text
        // in an empty position.
        string inText = System.Text.RegularExpressions.Regex.Escape(cpbText);

        string command = "var selection = editor.getSelection(); ";
        command += "var id = { major: 1, minor: 1 }; ";
        command += "var text='" + inText + "'; ";
        command += "var op = {identifier: id, range: selection, text: text, forceMoveMarkers: true}; ";
        command += "editor.executeEdits(\"my-source\", [op]);";
        await MonacoEditorWebView.ExecuteScriptAsync(command);

    }

    public Task ContextMenuEnabled(bool status=true)
    {
        string command = "";
        if (status)
            command = $"editor.updateOptions({{ contextmenu: true }});";
        else
            command = $"editor.updateOptions({{ contextmenu: false }});";
        this.MonacoEditorWebView.ExecuteScriptAsync(command);
        return null;
    }

    /// <summary>
    /// Scroll to specified line number
    /// </summary>
    /// <param name="lineNumber">
    /// int value, specifies the line number to jump to.
    /// </param>
    public void ScrollToLine(int lineNumber)
    {
        string command = "";
        command = $"editor.revealLine({lineNumber}); editor.setPosition({{lineNumber: {lineNumber}, column: 0 }});";
        this.MonacoEditorWebView.ExecuteScriptAsync(command);
    }
    
    /// <summary>
    /// Scroll to specified line number, the destination line shows in the middle of the editor
    /// </summary>
    /// <param name="lineNumber">
    /// int value, specifies the line number to jump to.
    /// </param>
    public void ScrollToLineInCenter(int lineNumber)
    {
        string command = "";
        command = $"editor.revealLineInCenter({lineNumber}); editor.setPosition({{lineNumber: {lineNumber}, column: 0 }});";
        this.MonacoEditorWebView.ExecuteScriptAsync(command);
    }
    /// <summary>
    /// Scroll editor to top
    /// </summary>
    public void ScrollToTop()
    {
        string command = "editor.setScrollPosition({scrollTop: 0});";
        this.MonacoEditorWebView.ExecuteScriptAsync(command);
    }

}