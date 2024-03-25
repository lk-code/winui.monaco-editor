using System;
using System.Threading.Tasks;

namespace Monaco.MonacoHandler;

public class MonacoEditorThemeHandler : MonacoBaseHandler, IMonacoHandler
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="theme">vs-dark, vs-light, hc-black</param>
    /// <returns></returns>
    public async Task SetThemeAsync(string theme)
    {
        string command = $"editor._themeService.setTheme('{theme}');";
        
        await this.WebView!.ExecuteScriptAsync(command);
    }
}
