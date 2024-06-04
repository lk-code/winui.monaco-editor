using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

namespace Monaco.MonacoHandler;

public class MonacoFileRecognitionHandler : MonacoBaseHandler, IMonacoHandler
{
    /// <summary>
    /// This dictionary helps WinUI.Monaco to guess
    /// the coding language of a file from its extension.
    /// This list is not complete as monaco-editor holds
    /// many more lesser languages and they will be added
    /// in future commits.
    /// </summary>
    private Dictionary<string, string> _fileTypeMappings = new()
    {
        { ".txt","plaintext" },
        { ".bat","bat" },
        { ".c","c" },
        { ".h","c" },
        { ".mligo","cameligo" },
        { ".cpp","cpp" },
        { ".cs","csharp" },
        { ".coffee","coffeescript" },
        { ".css","css" },
        { ".clj","clojure" },
        { ".cql","cypher" },
        { ".dart","dart" },
        { ".ecl","ecl" },
        { ".exs","elixir" },
        { ".flow","flow9" },
        { ".go","go" },
        { ".htm","html" },
        { ".html","html" },
        { ".ini","ini" },
        { ".java","java" },
        { ".js","javascript" },
        { ".jl","julia" },
        { ".kt","kotlin" },
        { ".kts","kotlin" },
        { ".ktm","kotlin" },
        { ".md","markdown" },
        { ".lua","lua" },
        { ".pas","pascal" },
        { ".perl","perl" },
        { ".php","php" },
        { ".ps1","powershell" },
        { ".py","python" },
        { ".r","r" },
        { ".rb","ruby" },
        { ".rs","rust" },
        { ".sh","shell" },
        { ".sql","sql" },
        { ".ts","typescript" },
        { ".vb","vb" },
        { ".xml","xml" },
        { ".axml","xml" },
        { ".xaml","xml" },
        { ".json","json" },
        { ".yml","yaml" },
        { ".yaml","yaml" }
    };

    /// <summary>
    /// returns the vs code language by file extension (like .cs, .html, etc.)
    /// </summary>
    /// <param name="fileExtension"></param>
    /// <returns></returns>
    public string RecognizeLanguageByFileType(string fileExtension)
    {
        return _fileTypeMappings.TryGetValue(fileExtension, out var codeLangs) ? codeLangs : "plaintext";
    }
}
