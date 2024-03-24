using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System.Threading.Tasks;

namespace Monaco;

public abstract class MonacoBaseHandler
{
    protected WebView2? WebView;
    protected MonacoEditor? ParentInstance;

    protected MonacoBaseHandler()
    {
    }

    ~MonacoBaseHandler()
    {
        if (this.ParentInstance is not null)
        {

        }

        if (this.WebView is not null)
        {
            this.WebView.CoreWebView2.WebMessageReceived -= CoreWebView2_WebMessageReceived;
        }
    }

    private async void CoreWebView2_WebMessageReceived(CoreWebView2 sender, CoreWebView2WebMessageReceivedEventArgs args)
    {
        string message = args.TryGetWebMessageAsString();

        await this.ProcessReceivedMessage(message);
    }

    protected virtual async Task ProcessReceivedMessage(string message)
    {
        await Task.CompletedTask;
    }

    public void WithWebView(WebView2 webView)
    {
        this.WebView = webView;

        // events
        this.WebView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
    }

    public void WithParentInstance(MonacoEditor instance)
    {
        this.ParentInstance = instance;

        // events
    }
}
