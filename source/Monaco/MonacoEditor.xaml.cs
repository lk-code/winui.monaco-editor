using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
//using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Globalization;
using Windows.Storage;
using Windows.UI.WebUI;
using WinRT;

namespace Monaco;

public sealed partial class MonacoEditor : UserControl, IMonacoEditor
{
    private const string HTML_LAUNCH_FILE = @"monaco-editor\index.html";

    private string _content = "";
    private bool _isminimapvisible = true;
    private bool _minimapautohide = true;
    private string _minimapside = "right";
    private string _minimapsize = "fit";
    private string _minimapshowslider = "always";
    private bool _minimaprendercharacters = true;
    private int _minimapscale = 1;
    private int _minimapsectionheaderfontsize = 9;
    private bool _minimapshowmarksectionheaders = true;
    private bool _minimapshowregionsectionheaders = true;
    private int _minimapmaxcolumn = 120;
    private bool _readonly = false;
    private bool _stickyscroll = true;
    private bool _ariarequired = false;
    private string _arialabel = "";
    private string _autoindentstrategy = "none";
    private bool _codelens = false;
    private bool _folding = true;
    private string _linehighlight = "";
    private bool _linenumbers = true;
    private int _scrollline;
    private int _countlines;
    private string _wordwrapmode = "off";

    public EditorThemes cTheme { get; set; } = EditorThemes.VisualStudioLight;
    public bool LoadCompleted { get; set; } = false;

    public event EventHandler? MonacoEditorLoaded = null;
    /// <summary>
    /// Enables or disables CodeLens (default is FALSE)
    /// </summary>
    /// <param name="status">"true" sets the editor as read only</param>
    /// <returns></returns>
    public int Lines { get; set; }

    /// <summary>
    /// This dictionary helps WinUI.Monaco to guess
    /// the coding language of a file from its extension.
    /// This list is not complete as monaco-editor holds
    /// many more lesser languages and they will be added
    /// in future commits.
    /// </summary>
    public Dictionary<string, string> _CodeLangs = new()
    {
        { ".txt","plaintext" },
        { ".bat","bat" },
        { ".c","c" },
        { ".h","c" },
        { ".mligo","cameligo" },
        { ".cpp","cpp" },
        { ".cs","csharp" },
        { ".coffee","coffeescript" },
        { ".css","css" },
        { ".clj","clojure" },
        { ".cql","cypher" },
        { ".dart","dart" },
        { ".ecl","ecl" },
        { ".exs","elixir" },
        { ".flow","flow9" },
        { ".go","go" },
        { ".htm","html" },
        { ".html","html" },
        { ".ini","ini" },
        { ".java","java" },
        { ".js","javascript" },
        { ".jl","julia" },
        { ".kt","kotlin" },
        { ".kts","kotlin" },
        { ".ktm","kotlin" },
        { ".md","markdown" },
        { ".lua","lua" },
        { ".pas","pascal" },
        { ".perl","perl" },
        { ".php","php" },
        { ".ps1","powershell" },
        { ".py","python" },
        { ".r","r" },
        { ".rb","ruby" },
        { ".rs","rust" },
        { ".sh","shell" },
        { ".sql","sql" },
        { ".ts","typescript" },
        { ".vb","vb" },
        { ".xml","xml" },
        { ".axml","xml" },
        { ".xaml","xml" },
        { ".json","json" },
        { ".yml","yaml" },
        { ".yaml","yaml" }
    };


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

    #region TextSelected Event
    /// <summary>
    /// Added 20240504 - MR
    /// This event is fired when text gets selected
    /// into the editor view.
    /// </summary>
    public event EventHandler<TextSelectionArgs>? EditorTextSelected; 
    
    private async void OnTextSelected()
    {
        _content = await GetSelectedTextAsync();
        TextSelectionArgs args = new TextSelectionArgs(_content);
        EditorTextSelected?.Invoke(this, args);        
    }

    #endregion

    #region CursorPositionChanged Event
    /// <summary>
    /// Added 20240506 - MR
    /// This event is fired when cursor position changes.
    /// </summary>
    public event EventHandler<CursorPositionArgs>? CursorPositionChanged;

    private async void OnCursorPositionChanged()
    {
        string jPosition = await MonacoEditorWebView.ExecuteScriptAsync("editor.getPosition();");
        EditorCursorPosition cPosition = JsonSerializer.Deserialize<EditorCursorPosition>(jPosition);
        CursorPositionArgs args = new CursorPositionArgs(cPosition);
        CursorPositionChanged?.Invoke(this, args);        
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

            this.EnableMiniMap(value);
        }
    }

    #endregion

    #region MiniMapAutoHide Property

    public static readonly DependencyProperty MiniMapAutohideProperty = DependencyProperty.Register("MiniMapAutohide",
        typeof(bool),
        typeof(MonacoEditor),
        new PropertyMetadata(null));

    public bool EditorMiniMapAutohide
    {
        get
        {
            return _minimapautohide;
        }
        set
        {
            SetValue(MiniMapAutohideProperty, value);
            OnPropertyChanged();

            this.EnableMapAutoHide(value);
        }
    }

    #endregion

    #region MiniMapRenderCharacters Property

    public static readonly DependencyProperty MiniMapRenderCharactersProperty = DependencyProperty.Register("MiniMapRenderCharacters",
        typeof(bool),
        typeof(MonacoEditor),
        new PropertyMetadata(null));

    public bool EditorMiniMapRenderCharacters
    {
        get
        {
            return _minimaprendercharacters;
        }
        set
        {
            SetValue(MiniMapRenderCharactersProperty, value);
            OnPropertyChanged();

            this.RenderMapCharacters(value);
        }
    }

    #endregion

    #region MiniMapShowSlider Property

    public static readonly DependencyProperty MiniMapShowSliderProperty = DependencyProperty.Register("MiniMapShowSlider",
        typeof(string),
        typeof(MonacoEditor),
        new PropertyMetadata(null));

    public string EditorMiniMapShowSlider
    {
        get
        {
            return _minimapshowslider;
        }
        set
        {
            SetValue(MiniMapShowSliderProperty, value);
            OnPropertyChanged();

            this.ShowMapSlider(value);
        }
    }

    #endregion

    #region MiniMapSide Property

    public static readonly DependencyProperty MiniMapSideProperty = DependencyProperty.Register("MiniMapSide",
        typeof(string),
        typeof(MonacoEditor),
        new PropertyMetadata(null));

    public string EditorMiniMapSide
    {
        get
        {
            return _minimapside;
        }
        set
        {
            SetValue(MiniMapSideProperty, value);
            OnPropertyChanged();

            this.SetMapSide(value);
        }
    }

    #endregion

    #region MiniMapSize Property

    public static readonly DependencyProperty MiniMapSizeProperty = DependencyProperty.Register("MiniMapSize",
        typeof(string),
        typeof(MonacoEditor),
        new PropertyMetadata(null));

    public string EditorMiniMapSize
    {
        get
        {
            return _minimapsize;
        }
        set
        {
            SetValue(MiniMapSizeProperty, value);
            OnPropertyChanged();

            this.SetMapSize(value);
        }
    }

    #endregion

    #region MiniMapScale Property

    public static readonly DependencyProperty MiniMapScaleProperty = DependencyProperty.Register("MiniMapScale",
        typeof(int),
        typeof(MonacoEditor),
        new PropertyMetadata(null));

    public int EditorMiniMapScale
    {
        get
        {
            return _minimapscale;
        }
        set
        {
            SetValue(MiniMapScaleProperty, value);
            OnPropertyChanged();

            this.SetMapScale(value);
        }
    }

    #endregion

    #region MiniMapSectionHeaderFontSize Property

    public static readonly DependencyProperty MiniMapSectionHeaderFontSizeProperty = DependencyProperty.Register("MiniMapSectionHeaderFontSize",
        typeof(int),
        typeof(MonacoEditor),
        new PropertyMetadata(null));

    public int EditorMiniMapSectionHeaderFontSize
    {
        get
        {
            return _minimapsectionheaderfontsize;
        }
        set
        {
            SetValue(MiniMapSectionHeaderFontSizeProperty, value);
            OnPropertyChanged();

            this.SetMapSectionHeaderFontSize(value);
        }
    }

    #endregion

    #region MiniMapShowMarkSectionHeaders Property

    public static readonly DependencyProperty MiniMapShowMarkSectionHeadersProperty = DependencyProperty.Register("MiniMapShowMarkSectionHeaders",
        typeof(bool),
        typeof(MonacoEditor),
        new PropertyMetadata(null));

    /// <summary>
    /// Hides/shows the mini code map
    /// </summary>
    public bool EditorMiniMapShowMarkSectionHeaders
    {
        get
        {
            return _minimapshowmarksectionheaders;
        }
        set
        {
            SetValue(MiniMapShowMarkSectionHeadersProperty, value);
            OnPropertyChanged();

            this.SetMapShowMarkSectionHeaders(value);
        }
    }

    #endregion

    #region MiniMapShowRegionSectionHeaders Property

    public static readonly DependencyProperty MiniMapShowRegionSectionHeadersProperty = DependencyProperty.Register("MiniMapShowRegionSectionHeaders",
        typeof(bool),
        typeof(MonacoEditor),
        new PropertyMetadata(null));

    /// <summary>
    /// Hides/shows the mini code map
    /// </summary>
    public bool EditorMiniMapShowRegionSectionHeaders
    {
        get
        {
            return _minimapshowregionsectionheaders;
        }
        set
        {
            SetValue(MiniMapShowRegionSectionHeadersProperty, value);
            OnPropertyChanged();

            this.SetMapShowRegionSectionHeaders(value);
        }
    }

    #endregion

    #region MiniMapMaxColumn Property

    public static readonly DependencyProperty MiniMapMaxColumnProperty = DependencyProperty.Register("MiniMapMaxColumn",
        typeof(int),
        typeof(MonacoEditor),
        new PropertyMetadata(null));

    public int EditorMiniMapMaxColumnSize
    {
        get
        {
            return _minimapmaxcolumn;
        }
        set
        {
            SetValue(MiniMapMaxColumnProperty, value);
            OnPropertyChanged();

            this.SetMapMaxColumn(value);
        }
    }

    #endregion


    #region AriaRequired Property

    public static readonly DependencyProperty AriaRequiredProperty = DependencyProperty.Register("AriaRequired",
        typeof(bool),
        typeof(MonacoEditor),
        new PropertyMetadata(null));

    /// <summary>
    /// Hides/shows the mini code map
    /// </summary>
    public bool EditorAriaRequired
    {
        get
        {
            return _ariarequired;
        }
        set
        {
            SetValue(AriaRequiredProperty, value);
            OnPropertyChanged();

            this.AriaRequired(value);
        }
    }

    #endregion

    #region AriaLabel Property

    public static readonly DependencyProperty AriaLabelProperty = DependencyProperty.Register("AriaLabel",
        typeof(string),
        typeof(MonacoEditor),
        new PropertyMetadata(null));

    /// <summary>
    /// Set a custom message to tell user that the editor is in read only mode.
    /// </summary>
    public string EditorAriaLabel
    {
        get
        {
            return _arialabel;
        }
        set
        {
            SetValue(AriaLabelProperty, value);
            OnPropertyChanged();

            this.AriaLabel(value);
        }
    }

    #endregion

    #region AutoIndentStrategy Property

    public static readonly DependencyProperty AutoIndentStrategyProperty = DependencyProperty.Register("AutoIndentStrategy",
        typeof(string),
        typeof(MonacoEditor),
        new PropertyMetadata(null));

    /// <summary>
    /// Set a custom message to tell user that the editor is in read only mode.
    /// </summary>
    public string EditorAutoIndentStartegy
    {
        get
        {
            return _autoindentstrategy;
        }
        set
        {
            SetValue(AutoIndentStrategyProperty, value);
            OnPropertyChanged();

            this.AutoIndentStrategy(value);
        }
    }

    #endregion

    #region CodeLens Property

    public static readonly DependencyProperty CodeLensProperty = DependencyProperty.Register("CodeLens",
        typeof(bool),
        typeof(MonacoEditor),
        new PropertyMetadata(null));

    /// <summary>
    /// Set the editor to readonly or not.
    /// </summary>
    public bool EditorCodeLens
    {
        get
        {
            return _codelens;
        }
        set
        {
            SetValue(CodeLensProperty, value);
            OnPropertyChanged();

            this.CodeLens(value);
        }
    }

    #endregion

    #region Folding Property

    public static readonly DependencyProperty FoldingProperty = DependencyProperty.Register("Folding",
        typeof(bool),
        typeof(MonacoEditor),
        new PropertyMetadata(null));

    /// <summary>
    /// Set the editor to readonly or not.
    /// </summary>
    public bool EditorFolding
    {
        get
        {
            return _folding;
        }
        set
        {
            SetValue(FoldingProperty, value);
            OnPropertyChanged();

            this.EnableFolding(value);
        }
    }

    #endregion

    #region LineHighlight Property

    public static readonly DependencyProperty LineHighlightProperty = DependencyProperty.Register("LineHighlight",
        typeof(string),
        typeof(MonacoEditor),
        new PropertyMetadata(null));

    /// <summary>
    /// Set the editor to readonly or not.
    /// </summary>
    public string EditorLineHighlight
    {
        get
        {
            return _linehighlight;
        }
        set
        {
            SetValue(LineHighlightProperty, value);
            OnPropertyChanged();

            this.LineHighlight(value);
        }
    }

    #endregion


    #region LineNumbers Property

    public static readonly DependencyProperty LineNumbersProperty = DependencyProperty.Register("LineNumbers",
        typeof(bool),
        typeof(MonacoEditor),
        new PropertyMetadata(null));

    /// <summary>
    /// Set the editor to readonly or not.
    /// </summary>
    public bool EditorLineNumbers
    {
        get
        {
            return _linenumbers;
        }
        set
        {
            SetValue(LineNumbersProperty, value);
            OnPropertyChanged();

            this.EnableLineNumbers(value);
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

    #region ScrollToLineProperty

    public static readonly DependencyProperty ScrollToLineProperty = DependencyProperty.Register("ScrollToLine",
        typeof(int),
        typeof(MonacoEditor),
        new PropertyMetadata(null));

    /// <summary>
    /// Set the editor to StickyScroll mode or not.
    /// </summary>
    public int EditorScrollToLine
    {
        get
        {
            return _scrollline;
        }
        set
        {
            SetValue(ScrollToLineProperty, value);
            OnPropertyChanged();

            this.ScrollToLine(value);
        }
    }

    #endregion

    #region ScrollToLineInCenterProperty

    public static readonly DependencyProperty ScrollToLineInCenterProperty = DependencyProperty.Register("ScrollToLineInCenter",
        typeof(int),
        typeof(MonacoEditor),
        new PropertyMetadata(null));

    /// <summary>
    /// Set the editor to StickyScroll mode or not.
    /// </summary>
    public int EditorScrollToLineInCenter
    {
        get
        {
            return _scrollline;
        }
        set
        {
            SetValue(ScrollToLineInCenterProperty, value);
            OnPropertyChanged();

            this.ScrollToLineInCenter(value);
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
            return _stickyscroll;
        }
        set
        {
            SetValue(StickyScrollProperty, value);
            OnPropertyChanged();

            this.EnableStickyScroll(value);
        }
    }



    #endregion

    #region WordWrap Property

    public static readonly DependencyProperty WordWrapProperty = DependencyProperty.Register("WordWrap",
        typeof(string),
        typeof(MonacoEditor),
        new PropertyMetadata(null));

    /// <summary>
    /// Set a custom message to tell user that the editor is in read only mode.
    /// </summary>
    public string EditorWordWrap
    {
        get
        {
            return _content;
        }
        set
        {
            SetValue(WordWrapProperty, value);
            OnPropertyChanged();

            this.WordWrap(value);
        }
    }

    #endregion


    public string CurrentCodeLanguage { get; set; }

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
                    break;
                }
            case "EVENT_EDITOR_CONTENT_CHANGED":
                {
                    OnContentChanged();
                    break;
                }
            case "EVENT_TEXT_SELECTED":
                {
                    OnTextSelected();
                    break;
                }
            case "EVENT_CURSOR_MOVED":
                {
                    OnCursorPositionChanged();
                    break;
                }
                

            // monaco events
        }
    }

    private async void WebView_NavigationCompleted(object sender, object e)
    {
        await MonacoEditorWebView.EnsureCoreWebView2Async();

        LoadCompleted = true;
        _ = this.SetThemeAsync(this.EditorTheme);
        _ = this.SetLanguageAsync(this.EditorLanguage);

        string javaScriptContentChangedEventHandlerWebMessage = "window.editor.getModel().onDidChangeContent((event) => { handleWebViewMessage(\"EVENT_EDITOR_CONTENT_CHANGED\"); });";
        _ = await MonacoEditorWebView.ExecuteScriptAsync(javaScriptContentChangedEventHandlerWebMessage);
        string javaScriptTextSelectedEventHandlerWebMessage = "editor.onDidChangeCursorSelection((e) =>{handleWebViewMessage(\"EVENT_TEXT_SELECTED\");});";
        _ = await MonacoEditorWebView.ExecuteScriptAsync(javaScriptTextSelectedEventHandlerWebMessage);
        string javaScriptCursorMovedEventHandlerWebMessage = "editor.onDidChangeCursorPosition((e) =>{handleWebViewMessage('EVENT_CURSOR_MOVED'); });";
        _ = await MonacoEditorWebView.ExecuteScriptAsync(javaScriptCursorMovedEventHandlerWebMessage);
    }

    public void EnableMiniMap(bool status=true)
    {
        string command = "";
        if (status)
            command = $"editor.updateOptions({{ minimap: {{ enabled: true }} }});";
        else
            command = $"editor.updateOptions({{ minimap: {{ enabled: false }} }});";
        this.MonacoEditorWebView.ExecuteScriptAsync(command);
    }
    public void EnableMapAutoHide(bool status = true)
    {
        string command = "";
        if (status)
            command = $"editor.updateOptions({{ minimap: {{ autohide: true }} }});";
        else
            command = $"editor.updateOptions({{ minimap: {{ autohide: false }} }});";
        this.MonacoEditorWebView.ExecuteScriptAsync(command);
    }

    public void SetMapSide(string mode = "right")
    {
        string command = "";
        if (string.IsNullOrEmpty(mode) || (mode.ToLower() != "right" && mode.ToLower() != "left"))
            mode = "right";
        command = $"editor.updateOptions({{ minimap: {{ side: '{mode}' }} }});";
        this.MonacoEditorWebView.ExecuteScriptAsync(command);
    }

    public void SetMapSize(string mode = "fit")
    {
        string command = "";
        if (string.IsNullOrEmpty(mode) || (mode.ToLower() != "fit" && mode.ToLower() != "fill" && mode.ToLower() != "proportional"))
            mode = "fit";
        command = $"editor.updateOptions({{ minimap: {{ size: '{mode}' }} }});";
        this.MonacoEditorWebView.ExecuteScriptAsync(command);
    }

    public void ShowMapSlider(string mode = "mouseover")
    {
        string command = "";
        if (string.IsNullOrEmpty(mode) || (mode.ToLower() != "always" && mode.ToLower() != "mouseover"))
            mode = "mouseover";
        command = $"editor.updateOptions({{ minimap: {{ showSlider: '{mode}' }} }});";
        this.MonacoEditorWebView.ExecuteScriptAsync(command);
    }

    public void RenderMapCharacters(bool status = true)
    {
        string command = "";
        if (status)
            command = $"editor.updateOptions({{ minimap: {{ renderCharacters: true }} }});";
        else
            command = $"editor.updateOptions({{ minimap: {{ renderCharacters: false }} }});";
        this.MonacoEditorWebView.ExecuteScriptAsync(command);
    }

    public void SetMapScale(int value = 1)
    {
        string command = "";
        if (value < 1 && value > 5)
            value = 1;
        command = $"editor.updateOptions({{ minimap: {{ scale: {value} }} }});";
        this.MonacoEditorWebView.ExecuteScriptAsync(command);
    }

    public void SetMapSectionHeaderFontSize(int value = 9)
    {
        string command = "";
        if (value < 5 && value > 18)
            value = 9;
        command = $"editor.updateOptions({{ minimap: {{ sectionHeaderFontSize: {value} }} }});";
        this.MonacoEditorWebView.ExecuteScriptAsync(command);
    }

    public void SetMapShowMarkSectionHeaders(bool status = true)
    {
        string command = "";
        if (status)
            command = $"editor.updateOptions({{ minimap: {{ showMarkSectionHeaders: true }} }});";
        else
            command = $"editor.updateOptions({{ minimap: {{ showMarkSectionHeaders: false }} }});";
        this.MonacoEditorWebView.ExecuteScriptAsync(command);
    }

    public void SetMapShowRegionSectionHeaders(bool status = true)
    {
        string command = "";
        if (status)
            command = $"editor.updateOptions({{ minimap: {{ showRegionSectionHeaders: true }} }});";
        else
            command = $"editor.updateOptions({{ minimap: {{ showRegionSectionHeaders: false }} }});";
        this.MonacoEditorWebView.ExecuteScriptAsync(command);
    }

    public void SetMapMaxColumn(int value = 120)
    {
        string command = "";
        if (value < 80 && value > 300)
            value = 120;
        command = $"editor.updateOptions({{ minimap: {{ maxColumn: {value} }} }});";
        this.MonacoEditorWebView.ExecuteScriptAsync(command);
    }

    public void AriaRequired(bool status = false)
    {
        string command = "";
        if (status)
            command = $"editor.updateOptions({{ ariaRequired: true }});";
        else
            command = $"editor.updateOptions({{ ariaRequired: false }});";
        MonacoEditorWebView.ExecuteScriptAsync(command);
    }

    public void AriaLabel(string content)
    {
        string command = "";
        this._arialabel = content;
        command = $"editor.updateOptions({{ariaLabel: '{content}' }});";

        this.MonacoEditorWebView.ExecuteScriptAsync(command);
    }

    public void AutoIndentStrategy(string content)
    {
        string command = "";
        this._autoindentstrategy = content;
        command = $"editor.updateOptions({{autoIndent: '{content}' }});";

        this.MonacoEditorWebView.ExecuteScriptAsync(command);
    }

    public void CodeLens(bool status = false)
    {
        string command = "";
        if (status)
            command = $"editor.updateOptions({{ codeLens: true }});";
        else
            command = $"editor.updateOptions({{ codeLens: false }});";
        this.MonacoEditorWebView.ExecuteScriptAsync(command);
    }

    public async Task<int> CountLines()
    {
        string command = "";
        command = "editor.getModel().getLineCount();";        
        var result = await this.MonacoEditorWebView.ExecuteScriptAsync(command);
        //Debug.WriteLine(result);
        int lineCount = int.Parse(result);
        //Debug.WriteLine("count: " + lineCount);
        return lineCount;
    }

    public void EnableFolding(bool status = true)
    {
        string command = "";
        if (status)
            command = $"editor.updateOptions({{ folding: true }});";
        else
            command = $"editor.updateOptions({{ folding: false }});";
        this.MonacoEditorWebView.ExecuteScriptAsync(command);
    }

    public void LineHighlight(string mode)
    {
        string command = "";
        if (mode!="")
            command = $"editor.updateOptions({{renderLineHighlight: '{mode}' }});";
        this.MonacoEditorWebView.ExecuteScriptAsync(command);
    }

    public void EnableLineNumbers(bool status = true)
    {
        string command = "";
        if (status)
            command = $"editor.updateOptions({{lineNumbers: 'on'}});";
        else
            command = $"editor.updateOptions({{lineNumbers: 'off'}});";
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

    public void ScrollToLine(int lineNumber)
    {
        string command = "";
        command = $"editor.revealLine({lineNumber}); editor.setPosition({{lineNumber: {lineNumber}, column: 0 }});";
        this.MonacoEditorWebView.ExecuteScriptAsync(command);
    }
    public void ScrollToLineInCenter(int lineNumber)
    {
        string command = "";
        command = $"editor.revealLineInCenter({lineNumber}); editor.setPosition({{lineNumber: {lineNumber}, column: 0 }});";
        this.MonacoEditorWebView.ExecuteScriptAsync(command);
    }

    public void ScrollToTop()
    {
        string command = "editor.setScrollPosition({scrollTop: 0});";
        this.MonacoEditorWebView.ExecuteScriptAsync(command);
    }

    public void EnableStickyScroll(bool status = true)
    {
        string command = "";
        if (status)
            command = $"editor.updateOptions({{ stickyScroll: {{ enabled: true }} }});";
        else
            command = $"editor.updateOptions({{ stickyScroll: {{ enabled: false }} }});";
        this.MonacoEditorWebView.ExecuteScriptAsync(command);
    }

    public void WordWrap(string mode="off")
    {
        string command = "";
        this._content = mode;
        command = $"editor.updateOptions({{wordWrap: {{value: '{mode}'}} }});";

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

    public async Task LoadFromFileAsync(StorageFile file, bool autodetect=false)
    {
        if (file == null) throw new ArgumentNullException("file not specified");
        string fContent = await FileIO.ReadTextAsync(file);
        if (autodetect)
        {
            string fExt = Path.GetExtension(file.Path).ToLower();
            if (_CodeLangs.TryGetValue(fExt, out var codeLangs))
            {
                await SetLanguageAsync(codeLangs);
                CurrentCodeLanguage = codeLangs;
            }
            else
            {
                await SetLanguageAsync("plaintext");
                CurrentCodeLanguage="plaintext";
            }
        }
        await LoadContentAsync(fContent);
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

    public async Task<string> GetSelectedTextAsync()
    {
        await MonacoEditorWebView.EnsureCoreWebView2Async();
        string command = "editor.getModel().getValueInRange(editor.getSelection());";
        string contentAsJsRepresentation = await this.MonacoEditorWebView.ExecuteScriptAsync(command);
        string unescapedString = System.Text.RegularExpressions.Regex.Unescape(contentAsJsRepresentation);
        string content = unescapedString.Substring(1, unescapedString.Length - 2).ReplaceLineEndings();

        return content;
    }

    /// <inheritdoc />
    public async Task SetThemeAsync(EditorThemes theme)
    {
        await MonacoEditorWebView.EnsureCoreWebView2Async();
        string themeValue = "vs-light"; // Changed by MR - 20240504

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
        CurrentCodeLanguage = languageId;
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

    public async void CopyTextToClipBoard()
    {
        string rawText = await MonacoEditorWebView.ExecuteScriptAsync("editor.getModel().getValueInRange(editor.getSelection());");
        string selectedText = System.Text.RegularExpressions.Regex.Unescape(rawText).TrimStart('"').TrimEnd('"');
        DataPackage dataPackage = new DataPackage();
        dataPackage.SetText(selectedText);
        Clipboard.SetContent(dataPackage);        
    }

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

   
}

public class TextSelectionArgs: EventArgs
{
    /// <summary>
    /// Added 20240504 - MR
    /// This class allows to get the selected text through
    /// the OntextSelected event. See usage in the TestApp project.
    /// </summary>
    public string SelectedText { get; private set; }

    public TextSelectionArgs(string mText)
    {
        SelectedText = mText;
    }
}

public class EditorCursorPosition
{
    /// Added 20240505 - MR
    /// REMARKS: Do not change members names of CurPos
    /// because they reflect the JSON object returned
    /// by Monaco Editor.
    public int lineNumber { get; set; } = 0;
    public int column { get; set; } = 0;
}

public class CursorPositionArgs : EventArgs
{
    /// <summary>
    /// Added 20240505 - MR
    /// This class allows to get the selected text through
    /// the OntextSelected event. See usage in the TestApp project.
    /// Monaco Editor returns cursor position as a JSON object which
    /// gets deserialized as two numeric values.
    /// </summary>
    public int mLine { get; private set; }
    public int mColumn { get; private set; }

    public CursorPositionArgs(EditorCursorPosition CurPos)
    {
        /// Added 20240505 - MR
        /// REMARKS: Do not change members names of CurPos
        /// because they reflect the JSON object returned
        /// by Monaco Editor.
        mLine = CurPos.lineNumber;
        mColumn = CurPos.column;
    }
}

public class MiniMapOptions
{
    public string showSlider { get; set; } = "always";
    public bool autoHide { get; set; } = true;
    public bool enabled { get; set; } = true;
    public string side { get; set; } = "right";
    public string size { get; set; } = "fit";
    public bool renderCharacters { get; set; } = true;
    public int scale { get; set; } = 1;
    public bool showMarkSectionHeaders { get; set; } = false;
    public bool showRegionSectionHeaders { get; set; } = false;
    public int sectionHeaderFontSize { get; set; } = 9;
    public int maxColumn { get; set; } = 120;
}