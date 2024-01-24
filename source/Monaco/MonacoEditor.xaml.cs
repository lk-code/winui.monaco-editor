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
    public bool LoadCompleted { get; set; } = false;

    private string _content = "";

    #region PropertyChanged Event

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
        this.Loaded += MonacoEditor_Loaded;
        MonacoEditorWebView.NavigationCompleted += WebView_NavigationCompleted;
        MonacoEditorWebView.WebMessageReceived += WebMessageReceived;
    }

    private void WebMessageReceived(WebView2 sender, CoreWebView2WebMessageReceivedEventArgs args)
    {

        switch(args.WebMessageAsJson)
        {
            case "\"EVENT_EDITOR_CONTENT_CHANGED\"":
                OnContentChanged();
                break;
            default:
                throw new ArgumentOutOfRangeException();
                break;
        }
    }

    private async void WebView_NavigationCompleted(object sender, object e)
    {
        string javaScriptMessageFunction =
           "\n" +
           "function postWebViewMessage(message){\n" +
                "try{\n" +
                    "if (window.hasOwnProperty(\"chrome\") && typeof chrome.webview !== undefined) {\n" +
                        "// Windows\n" +
                        "chrome.webview.postMessage(message);\n" +
                    "} else if (window.hasOwnProperty(\"unoWebView\")) {\n" +
                        "// Android\n" +
                        "unoWebView.postMessage(JSON.stringify(message));\n" +
                    "} else if (window.hasOwnProperty(\"webkit\") && typeof webkit.messageHandlers !== undefined) {\n" +
                        "// iOS and macOS\n" +
                        "webkit.messageHandlers.unoWebView.postMessage(JSON.stringify(message));\n" +
                    "}\n" +
                "}\n" +
                "catch (ex){\n" +
                    "alert(\"Error occurred: \" + ex);\n" +
                "}\n" +
            "}";

        _ = await MonacoEditorWebView.ExecuteScriptAsync(javaScriptMessageFunction);
        await Task.Delay(100);

        LoadCompleted = true;
        _ = this.SetThemeAsync(this.EditorTheme);
        _ = this.SetLanguageAsync(this.EditorLanguage);

        string javaScriptContentChangedEventHandlerWebMessage = "window.editor.getModel().onDidChangeContent((event) => { console.log(\"Editor content changed.\");  postWebViewMessage(\"EVENT_EDITOR_CONTENT_CHANGED\");});";
        _ = await MonacoEditorWebView.ExecuteScriptAsync(javaScriptContentChangedEventHandlerWebMessage);

    }

    private void MonacoEditor_Loaded(object sender, RoutedEventArgs e)
    {
        string monacoHtmlFile = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, @"MonacoEditorSource\index.html");
        this.MonacoEditorWebView.Source = new Uri(monacoHtmlFile);
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
    }
}