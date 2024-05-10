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
    /// loads the given content to the monaco editor view
    /// </summary>
    /// <param name="content">the new content</param>
    /// <returns></returns>
    Task LoadContentAsync(string content);

    /// <summary>
    /// loads content from a given StorageFile; you can also make MonacoEditor guess the code language by its file extension
    /// </summary>
    /// <param name="file">the StorageFile that will be loaded</param>
    /// <param name="autodetect">if set to "TRUE", Monaco Editor will try to guess the code language by file extension - default is FALSE</param>
    /// <returns></returns>
    Task LoadFromFileAsync(StorageFile file, bool autodetect=false);

    /// <summary>
    /// Sets the editor to be read only or not (default is FALSE)
    /// </summary>
    /// <param name="status">"true" sets the editor as read only</param>
    /// <returns></returns>
    void AriaLabel(string content);

    /// <summary>
    /// Enables or disables Aria (default is FALSE)
    /// </summary>
    /// <param name="status">"true" enables the feature</param>
    /// <returns></returns>
    void AriaRequired(bool status);

    /// <summary>
    /// Sets the strategy for AutoIndent
    /// </summary>
    /// <param name="mode">one of the accepted values</param>
    /// <returns></returns>
    void AutoIndentStrategy(string mode);

    /// <summary>
    /// Enables or disables CodeLens (default is FALSE)
    /// </summary>
    /// <param name="status">"true" sets the editor as read only</param>
    /// <returns></returns>
    void CodeLens(bool status);

    /// <summary>
    /// enables or disables folding in code editor (default is TRUE)
    /// </summary>
    /// <param name="status">"true" enables folding</param>
    /// <returns></returns>
    void Folding(bool status);

    /// <summary>
    /// Enbales or disables the mini map (default is TRUE)
    /// </summary>
    /// <param name="status">"true" shows the mini map, "false" hides the mini map</param>
    /// <returns></returns>
    void MiniMapEnabled(bool status);

    /// <summary>
    /// Enables or disable autohide for mini map (default is false)
    /// </summary>
    /// <param name="status">"true" autohides mini map, "false" always shows the mini map</param>
    /// <returns></returns>
    void MiniMapAutohide(bool status);

    /// <summary>
    /// Control the side of the minimap in editor.
    /// 
    /// Allowed values:
    /// - right
    /// - left
    /// 
    /// Default is RIGHT
    /// </summary>
    /// <param name="mode">"right" or "left"</param>
    /// <returns></returns>
    void MiniMapSide(string mode);

    /// <summary>
    /// sets the size of minimap. Allowed values:
    /// - fit
    /// - fill
    /// - proportional
    /// 
    /// Default is FIT
    /// </summary>
    /// <param name="mode">one of the allowed values</param>
    /// <returns></returns>
    void MiniMapSize(string mode);

    /// <summary>
    ///Control the rendering of the minimap slider. Defaults to 'mouseover'.
    ///
    /// Allowed values:
    /// - mouseover
    /// - always
    /// 
    /// </summary>
    /// <param name="mode">one of the allowed values</param>
    /// <returns></returns>
    void MiniMapShowSlider(string mode);

    /// <summary>
    /// enables or disable caracters rendering in mini map (default is TRUE)
    /// </summary>
    /// <param name="status">"true" makes mini map render text, "false" makes mini map just draw boxes as placeholder for text</param>
    /// <returns></returns>
    void MiniMapRenderCharacters(bool status);

    /// <summary>
    /// Limit the width of the minimap to render at most a certain number of columns. Defaults to 120.
    /// </summary>
    /// <param name="value">the maximum width to render</param>
    /// <returns></returns>
    void MiniMapMaxColumn(int value);

    /// <summary>
    /// relative size of the font in the mini map - Default is 1
    /// </summary>
    /// <param name="value">the size of scale</param>
    /// <returns></returns>
    void MiniMapScale(int value);

    /// <summary>
    /// sets the font size of section headers. Default is 9
    /// </summary>
    /// <param name="value">the size of section headers font</param>
    /// <returns></returns>
    void MiniMapSectionHeaderFontSize(int value);

    /// <summary>
    /// Whether to show MARK: comments as section headers. Defaults to true.
    /// </summary>
    /// <param name="status">"true" enables this option</param>
    /// <returns></returns>
    void MiniMapShowMarkSectionHeaders(bool status);

    /// <summary>
    /// Whether to show named regions as section headers. Defaults to true.
    /// </summary>
    /// <param name="status">"true" enables this option</param>
    /// <returns></returns>
    void MiniMapShowRegionSectionHeaders(bool status);

    /// <summary>
    /// Enables / disables the highlight of selected line in editor.
    /// This property relates to Monaco Editor "renderLineHighlight" option.
    /// </summary>
    /// <param name="status">"TRUE" turns on line highlighting</param>
    /// <returns></returns>
    void LineHighlight(string mode);

    /// <summary>
    /// show or hides line numbers (default is TRUE)
    /// </summary>
    /// <param name="status">"TRUE" turns on line numbers</param>
    /// <returns></returns>
    void LineNumbers(bool status);

    /// <summary>
    /// Sets the editor to be read only or not (default is FALSE)
    /// </summary>
    /// <param name="status">"true" sets the editor as read only</param>
    /// <returns></returns>
    void ReadOnly(bool status);

    /// <summary>
    /// Sets a custom message to tell user that editor is in read only mode.
    /// </summary>
    /// <param name="content">the content of the message</param>
    /// <returns></returns>
    void SetReadOnlyMessage(string content);

    /// <summary>
    /// Scrolls editor to specified line.
    /// This property relates to Monaco Editor "revealLine" option.
    /// </summary>
    /// <param name="lineNumber">the line number to reach.</param>
    /// <returns></returns>
    void ScrollToLine(int lineNumber);

    /// <summary>
    /// Scrolls editor to specified line but the editor will put that in the middle of the view.
    /// </summary>
    /// This property relates to Monaco Editor "revealLineInCenter" option.
    /// </summary>
    /// <param name="lineNumber">the line number to reach.</param>
    /// <returns></returns>
    void ScrollToLineInCenter(int lineNumber);

    /// <summary>
    /// Scrolls editor view to top.
    /// This property relates to Monaco Editor "setScrollPosition" option.
    /// </summary>
    /// <param></param>
    /// <returns></returns>
    void ScrollToTop();

    /// <summary>
    /// Enables or disables the sticky scroll mode (default is TRUE)
    /// This property relates to Monaco Editor "stickyScroll" option.
    /// </summary>
    /// <param name="status">"true" enables the sticky scroll mode, "false" disables the sticky scroll mode</param>
    /// <returns></returns>
    void StickyScroll(bool status);

    /// <summary>
    /// Sets wordwrapping mode. Accepted values:
    /// - on
    /// - off
    /// - wordWrapColumn 
    /// - bounded
    /// 
    /// </summary>
    /// <param name="mode">specifies if and how to set wordwrapping</param>
    /// <returns></returns>
    void WordWrap(string mode);


    /// <summary>
    /// Gets the content from the monaco editor view
    /// </summary>
    /// <returns>The content of the editor</returns>
    Task<string> GetEditorContentAsync();

    /// <summary>
    /// Gets the selected text in the editor
    /// </summary>
    /// <returns>The selected text</returns>
    Task<string> GetSelectedTextAsync();

    /// <summary>
    /// Select the whole content in the editor
    /// </summary>
    /// <returns></returns>
    Task SelectAllAsync();

    /// <summary>
    /// Returns all registered coding languages
    /// </summary>
    /// <returns></returns>
    Task<CodeLanguage[]> GetLanguagesAsync();

    /// <summary>
    /// Sets the coding language of the editor
    /// </summary>
    /// <param name="languageId">the ID of the coding language (csharp, plaintext, etc.)</param>
    /// <returns></returns>
    Task SetLanguageAsync(string languageId);

    /// <summary>
    /// Opens the webview developer tools (if available)
    /// </summary>
    /// <returns></returns>
    void OpenDebugWebViewDeveloperTools();
}