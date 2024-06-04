using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;

namespace Monaco;

public abstract class MonacoBaseHandler
{
    protected WebView2? WebView;
    protected IMonacoCore? ParentInstance;

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
            this.WebView.CoreWebView2.NavigationCompleted -= CoreWebView2_NavigationCompleted;
        }
    }

    private async void CoreWebView2_WebMessageReceived(CoreWebView2 sender, CoreWebView2WebMessageReceivedEventArgs args)
    {
        string message = args.TryGetWebMessageAsString();

        await this.OnReceivedMessage(message);
    }

    private async void CoreWebView2_NavigationCompleted(CoreWebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
    {
        await this.OnEditorLoaded();
    }

    protected virtual async Task OnReceivedMessage(string message)
    {
        await Task.CompletedTask;
    }

    protected virtual async Task OnEditorLoaded()
    {
        await Task.CompletedTask;
    }

    public void WithWebView(WebView2 webView)
    {
        this.WebView = webView;

        // events
        this.WebView.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
        this.WebView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
    }

    public void WithParentInstance(MonacoEditor instance)
    {
        this.ParentInstance = instance;

        // events
    }
}
