namespace Monaco;

public interface IMonacoCore
{
    bool UseEditorContentStabilizedEvent { get; set; }

    /// <summary>
    /// set the editor content into the parent instance (not to the editor => for sync from monaco editor)
    /// </summary>
    /// <param name="content"></param>
    void SetEditorContent(string content);

    /// <summary>
    /// triggers the editor content changed event
    /// </summary>
    void TriggerEditorContentChanged();
}
