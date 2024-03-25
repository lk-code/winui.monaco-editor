using Microsoft.UI.Dispatching;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace Monaco.MonacoHandler;

public class MonacoEditorContentHandler : MonacoBaseHandler, IMonacoHandler
{
    /// <summary>
    /// is called when the editor content has changed
    /// </summary>
    public event EventHandler? EditorContentChanged;
    /// <summary>
    /// is called when the editor content has stabilized (after a time of <seealso>EditorContentStabilizationTime</seealso>>)
    /// </summary>
    public event EventHandler? EditorContentStabilized;

    /// <summary>
    /// the time in ms to wait for the editor content stabilization
    /// </summary>
    public double EditorContentStabilizationTime { get; set; } = 3000;

    /// <summary>
    /// the timer for the stabilization
    /// </summary>
    private Timer? _stabilizationTimer = null;
    /// <summary>
    /// 
    /// </summary>
    private DispatcherQueue _uiDispatcherQueue = DispatcherQueue.GetForCurrentThread();

    ~MonacoEditorContentHandler()
    {
        if (this._stabilizationTimer is not null)
        {
            this._stabilizationTimer.Elapsed -= StabilizationTimer_Elapsed;
            this._stabilizationTimer.Close();
            this._stabilizationTimer.Dispose();
            this._stabilizationTimer = null;
        }
    }

    protected override async Task OnEditorLoaded()
    {
        await base.OnEditorLoaded();

        string command = "window.editor.getModel().onDidChangeContent((event) => { sendMessageToWebViewHandler(\"EVENT_EDITOR_CONTENT_CHANGED\"); });";
        _ = await this.WebView!.ExecuteScriptAsync(command);
    }

    protected override async Task OnReceivedMessage(string message)
    {
        await base.OnReceivedMessage(message);

        switch (message)
        {
            case "EVENT_EDITOR_CONTENT_CHANGED":
                {
                    await this.ProcessEditorContentChanging();
                }
                break;
        }
    }

    private async Task ProcessEditorContentChanging()
    {
        string editorContent = await this.GetEditorContentAsync();

        this.ParentInstance!.SetEditorContent(editorContent);
        this.EditorContentChanged?.Invoke(this, new EventArgs());
        if (!this.ParentInstance!.UseEditorContentStabilizedEvent)
        {
            this.ParentInstance!.TriggerEditorContentChanged();
        }

        if (this._stabilizationTimer is null)
        {
            this._stabilizationTimer = new Timer(this.EditorContentStabilizationTime);
            this._stabilizationTimer.AutoReset = false;
            this._stabilizationTimer.Elapsed += StabilizationTimer_Elapsed;
        }

        this._stabilizationTimer.Stop();
        this._stabilizationTimer.Start();
    }

    private void StabilizationTimer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        this._uiDispatcherQueue.TryEnqueue(() =>
        {
            this.EditorContentStabilized?.Invoke(this, new EventArgs());
            if (this.ParentInstance!.UseEditorContentStabilizedEvent)
            {
                this.ParentInstance!.TriggerEditorContentChanged();
            }
        });
    }

    /// <summary>
    /// Gets the content form the monaco editor view
    /// </summary>
    /// <returns>The content of the editor</returns>
    public async Task<string> GetEditorContentAsync()
    {
        string command = $"editor.getValue();";

        string contentAsJsRepresentation = await this.WebView!.ExecuteScriptAsync(command);
        string unescapedString = System.Text.RegularExpressions.Regex.Unescape(contentAsJsRepresentation);
        string content = unescapedString.Substring(1, unescapedString.Length - 2).ReplaceLineEndings();

        return content;
    }
}
