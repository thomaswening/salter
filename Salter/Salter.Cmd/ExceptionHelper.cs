using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salter.Cmd;

internal static class ExceptionHelper
{
    public static string UnpackException(Exception ex)
    {
        var result = new StringBuilder();
        result.AppendLine($"Exception type: {ex.GetType().Name}");
        result.AppendLine($"Message: {ex.Message}");
        result.AppendLine();
        result.AppendLine($"Stack trace:");
        result.AppendLine(ex.StackTrace);

        if (ex.InnerException is not null)
        {
            result.AppendLine();
            result.AppendLine("Inner exception:");
            result.AppendLine();
            result.AppendLine(UnpackException(ex.InnerException));
        }

        return result.ToString();
    }
}
