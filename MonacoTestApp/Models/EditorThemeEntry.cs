using Monaco;

namespace MonacoTestApp.Models;
public class EditorThemeEntry
{
    public string Title { get;set; }
    public EditorThemes EditorTheme { get;set; }

    public EditorThemeEntry(string title, EditorThemes editorTheme)
    {
        this.Title = title;
        this.EditorTheme = editorTheme;
    }
}
