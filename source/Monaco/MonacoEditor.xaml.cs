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
    /// <summary>
    /// 
    /// </summary>
    private bool _isminimapvisible = true;

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
    public bool EditorToggleMiniMap
    {
        get
        {
            return _isminimapvisible;
        }
        set
        {
            SetValue(IsMiniMapVisibleProperty, value);
            OnPropertyChanged();

            this.IsMiniMapVisible(value);
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
            object readOnlyProperty = GetValue(ReadOnlyProperty);
            return readOnlyProperty == null ? false : (bool)readOnlyProperty;
        }
        set
        {
            SetValue(ReadOnlyProperty, value);
            OnPropertyChanged();

            this.SetReadOnly(value);
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

    #region StickyScroll Property

    public static readonly DependencyProperty StickyScrollProperty = DependencyProperty.Register("StickyScroll",
        typeof(bool),
        typeof(MonacoEditor),
        new PropertyMetadata(null));

    /// <summary>
    /// Set the editor to StickyScroll mode or not.
    /// </summary>
    public bool EditorStickyScroll
    {
        get
        {
            object stickyScrollProperty = GetValue(StickyScrollProperty);
            return stickyScrollProperty == null ? false : (bool)stickyScrollProperty;
        }
        set
        {
            SetValue(StickyScrollProperty, value);
            OnPropertyChanged();

            this.StickyScroll(value);
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

    public void IsMiniMapVisible(bool status = true)
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

    public void SetReadOnly(bool status = false)
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
        this._content = content;

        _ = this.MonacoEditorWebView.ExecuteScriptAsync($"editor.updateOptions({{readOnlyMessage: {{value: '{content}'}} }});");
    }

    public void StickyScroll(bool status = true)
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

    private async Task RegisterContentChangingEvent()
    {
        string javaScriptContentChangedEventHandlerWebMessage = "window.editor.getModel().onDidChangeContent((event) => { handleWebViewMessage(\"EVENT_EDITOR_CONTENT_CHANGED\"); });";
        _ = await MonacoEditorWebView.ExecuteScriptAsync(javaScriptContentChangedEventHandlerWebMessage);
    }
}