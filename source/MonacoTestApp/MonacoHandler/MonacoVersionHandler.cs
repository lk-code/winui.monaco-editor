using Monaco;
using System.Threading.Tasks;

namespace MonacoTestApp.Extensions;

public class MonacoVersionHandler : MonacoBaseHandler, IMonacoHandler
{
    public async Task<string> GetVersionAsync()
    {
        return "";
    }
}
