using System.Threading.Tasks;

namespace Monaco;

public interface IMonacoEditor
{
    /// <summary>
    /// sets the requested theme to the monaco editor view
    /// </summary>
    /// <param name="theme">the requested theme</param>
    /// <returns></returns>
    Task SetThemeAsync(EditorThemes theme);

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