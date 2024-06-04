using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace Monaco;

public interface IMonacoEditor
{
    /// <summary>
    /// is called when the monaco editor is loaded
    /// </summary>
    event EventHandler MonacoEditorLoaded;

    /// <summary>
    /// sets the requested theme to the monaco editor view
    /// </summary>
    /// <param name="theme">the requested theme</param>
    /// <returns></returns>
    Task SetThemeAsync(EditorThemes theme);

    /// <summary>
    /// sets the requested theme to the monaco editor view
    /// </summary>
    /// <param name="theme">the requested theme</param>
    /// <returns></returns>
    Task SetThemeAsync(string theme);

    /// <summary>
    /// loads the given content to the monaco editor view
    /// </summary>
    /// <param name="content">the new content</param>
    /// <returns></returns>
    Task LoadContentAsync(string content);

    /// <summary>
    /// Gets the content form the monaco editor view
    /// </summary>
    /// <returns>The content of the editor</returns>
    Task<string> GetEditorContentAsync();

    /// <summary>
    /// hides or shows the mini code map (default is TRUE)
    /// </summary>
    /// <param name="status">"true" shows the mini map, "false" hides the mini map</param>
    /// <returns></returns>
    void SetEditorMiniMapVisible(bool status);

    /// <summary>
    /// sets the editor to be read only or not (default is FALSE)
    /// </summary>
    /// <param name="status">"true" sets the editor as read only</param>
    /// <returns></returns>
    void SetEditorReadOnly(bool status);

    /// <summary>
    /// set a custom message to tell user that editor is in read only mode.
    /// </summary>
    /// <param name="content">the content of the message</param>
    /// <returns></returns>
    void SetReadOnlyMessage(string content);

    /// <summary>
    /// enables or disables the sticky scroll mode (default is TRUE)
    /// </summary>
    /// <param name="status">"true" enables the sticky scroll mode, "false" disables the sticky scroll mode</param>
    /// <returns></returns>
    void SetEditorStickyScroll(bool status);

    /// <summary>
    /// select the whole content in the editor
    /// </summary>
    /// <returns></returns>
    Task SelectAllAsync();

    /// <summary>
    /// returns all registered coding languages
    /// </summary>
    /// <returns></returns>
    Task<CodeLanguage[]> GetLanguagesAsync();

    /// <summary>
    /// set the code language of the editor
    /// </summary>
    /// <param name="languageId">the language id of the code language (csharp, plaintext, etc.)</param>
    /// <returns></returns>
    Task SetLanguageAsync(string languageId);
}