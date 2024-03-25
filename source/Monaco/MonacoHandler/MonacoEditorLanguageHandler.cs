using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Monaco.MonacoHandler;

public class MonacoEditorLanguageHandler : MonacoBaseHandler, IMonacoHandler
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<CodeLanguage[]> GetLanguagesAsync()
    {
        string command = $"monaco.languages.getLanguages();";

        string languagesJson = await this.WebView!.ExecuteScriptAsync(command);

        CodeLanguage[] codeLanguages = JsonSerializer.Deserialize<CodeLanguage[]>(languagesJson)!;

        return codeLanguages;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="languageId"></param>
    /// <returns></returns>
    public async Task SetLanguageAsync(string languageId)
    {
        string command = $"editor.setModel(monaco.editor.createModel(editor.getValue(), '{languageId}'));";

        await this.WebView!.ExecuteScriptAsync(command);

        // Reset the change content event
        string javaScriptContentChangedEventHandlerWebMessage = "window.editor.getModel().onDidChangeContent((event) => { handleWebViewMessage(\"EVENT_EDITOR_CONTENT_CHANGED\"); });";
        _ = await this.WebView!.ExecuteScriptAsync(javaScriptContentChangedEventHandlerWebMessage);
    }
}