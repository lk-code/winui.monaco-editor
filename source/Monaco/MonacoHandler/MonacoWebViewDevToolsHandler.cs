namespace Monaco.MonacoHandler;

public class MonacoWebViewDevToolsHandler : MonacoBaseHandler, IMonacoHandler
{
    /// <summary>
    /// opens the webview developer tools (if available)
    /// </summary>
    public void OpenDebugWebViewDeveloperTools()
    {
        this.WebView!.CoreWebView2.OpenDevToolsWindow();
    }
}
