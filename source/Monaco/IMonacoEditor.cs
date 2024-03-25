using System;
using System.Threading.Tasks;

namespace Monaco;

public interface IMonacoEditor
{
    /// <summary>
    /// is called when the monaco editor is loaded
    /// </summary>
    event EventHandler MonacoEditorLoaded;

    /// <summary>
    /// returns the initialized monaco handler by the given type
    /// </summary>
    /// <returns></returns>
    T GetHandler<T>() where T : IMonacoHandler;

    /// <summary>
    /// loads the given content to the monaco editor view
    /// </summary>
    /// <param name="content">the new content</param>
    /// <returns></returns>
    Task LoadContentAsync(string content);

    /// <summary>
    /// select the whole content in the editor
    /// </summary>
    /// <returns></returns>
    Task SelectAllAsync();

    /// <summary>
    /// returns all registered coding languages
    /// </summary>
    /// <returns></returns>
    [Obsolete("use the MonacoEditorLanguageHandler instead (see documentation)")]
    Task<CodeLanguage[]> GetLanguagesAsync();

    /// <summary>
    /// set the code language of the editor
    /// </summary>
    /// <param name="languageId">the language id of the code language (csharp, plaintext, etc.)</param>
    /// <returns></returns>
    [Obsolete("use the MonacoEditorLanguageHandler instead (see documentation)")]
    Task SetLanguageAsync(string languageId);

    /// <summary>
    /// open the webview developer tools (if available)
    /// </summary>
    /// <returns></returns>
    [Obsolete("use the MonacoWebViewDevToolsHandler instead (see documentation)")]
    void OpenDebugWebViewDeveloperTools();

    /// <summary>
    /// sets the requested theme to the monaco editor view
    /// </summary>
    /// <param name="theme">the requested theme</param>
    /// <returns></returns>
    [Obsolete("use the MonacoEditorThemeHandler instead (see documentation)")]
    Task SetThemeAsync(EditorThemes theme);

    /// <summary>
    /// Gets the content form the monaco editor view
    /// </summary>
    /// <returns>The content of the editor</returns>
    [Obsolete("use the MonacoEditorContentHandler instead (see documentation)")]
    Task<string> GetEditorContentAsync();
}