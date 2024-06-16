using Microsoft.UI.Xaml.Controls;

namespace Monaco;

/// <summary>
/// 
/// </summary>
public interface IMonacoHandler
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="webView"></param>
    void WithWebView(WebView2 webView);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="instance"></param>
    void WithParentInstance(MonacoEditor instance);
}