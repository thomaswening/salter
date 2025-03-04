using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salter.Cmd;
internal static class UninstallerUtility
{
    const int WaitingTimeSeconds = 10;

    public static bool RunUninstallProcess()
    {
        var exePath = Environment.ProcessPath;
        var appDir = AppConstants.AppDirectoryUri.LocalPath;

        if (string.IsNullOrEmpty(exePath))
        {
            Console.WriteLine("Could not find the executable path.");
            Console.ReadKey();
            return false;
        }

        if (!DeleteAppDirectory(appDir))
        {
            return false;
        }

        return UninstallExecutable(exePath);
    }

    private static void OpenDirectoryInExplorer(string dirPath)
    {
        try
        {
            Process.Start("explorer.exe", dirPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to open directory in explorer: {ex.Message}");
        }
    }

    private static bool UninstallExecutable(string exePath)
    {
        var directory = Path.GetDirectoryName(exePath);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(exePath);

        if (directory is null)
        {
            Console.WriteLine($"Could not find the directory of the executable: {exePath}");
            return false;
        }

        try
        {
            var filesToDelete = Directory.GetFiles(directory, $"{fileNameWithoutExtension}.*");

            var deleteCommands = new StringBuilder();
            deleteCommands.Append($"timeout /t {WaitingTimeSeconds}");
            foreach (var file in filesToDelete)
            {
                deleteCommands.Append($" & del \"{file}\"");
            }

            var processInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C {deleteCommands}",
                CreateNoWindow = true,
                UseShellExecute = false
            };

            Process.Start(processInfo);

            Console.WriteLine($"Uninstallation process started.\n" +
                $"The files will be deleted in {WaitingTimeSeconds} seconds.");

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to uninstall executable.");
            Console.WriteLine(ExceptionHelper.UnpackException(ex));
            Console.WriteLine();

            // Continue to ask user if they want to try and manually delete the files
        }

        if (!ConsoleInputHelper.GetUserConfirmation("Do you want to try and manually delete the executable?"))
        {
            Console.WriteLine("Aborting uninstallation procedure.");
            return false;
        }

        var exeDirPath = Path.GetFileName(exePath);
        OpenDirectoryInExplorer(exeDirPath);

        ConsoleInputHelper.GetUserConfirmation("Press any key to continue after you have manually deleted the executable.");

        if (File.Exists(exePath))
        {
            Console.WriteLine("Failed to delete the executable. Aborting uninstallation procedure.");
            return false;
        }

        return true;
    }

    private static bool DeleteAppDirectory(string appDir)
    {
        try
        {
            if (!Directory.Exists(appDir))
            {
                // Strange but nothing to do then...
                return true;
            }

            Directory.Delete(appDir, true);
            Console.WriteLine($"App directory '{appDir}' and its contents have been deleted.");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to delete app directory.");
            Console.WriteLine(ExceptionHelper.UnpackException(ex));
            Console.WriteLine();

            // Contine to ask user if they want to try and manually delete the app directory
        }

        if (!ConsoleInputHelper.GetUserConfirmation("Do you want to try and manually delete the app directory?"))
        {
            return false;
        }

        OpenDirectoryInExplorer(appDir);
        ConsoleInputHelper.GetUserConfirmation("Press any key to continue after you have manually deleted the app directory.");

        if (Directory.Exists(appDir))
        {
            Console.WriteLine("Failed to delete the app directory. Aborting uninstallation procedure.");
            return false;
        }

        return true;
    }
}
