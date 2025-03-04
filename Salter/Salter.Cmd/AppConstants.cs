namespace Salter.Cmd;
internal static class AppConstants
{
    public const string UserRepositoryFileName = "users.bin";
    public const string ApplicationName = "Salter";
    public const string KeyEnvironmentVariable = "SALTER_KEY";
    public const string IvEnvironmentVariable = "SALTER_IV";
    public const string ErrorLogFileNameBase = "error";

    public const string AppAsciiArt = @"
   _____         _   _______ ______ _____  
  / ____|  /\   | | |__   __|  ____|  __ \ 
 | (___   /  \  | |    | |  | |__  | |__) |
  \___ \ / /\ \ | |    | |  |  __| |  _  / 
  ____) / ____ \| |____| |  | |____| | \ \ 
 |_____/_/    \_\______|_|  |______|_|  \_\
";

    public static Uri UserRepositoryUri => GetUserRepositoryUri();
    public static Uri AppDirectoryUri => GetAppDirectoryUri();

    private static Uri GetAppDirectoryUri()
    {
        var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appDir = Path.Combine(appDataDir, ApplicationName);
        return new Uri(appDir);
    }

    private static Uri GetUserRepositoryUri()
    {
        var userRepoPath = Path.Combine(AppDirectoryUri.LocalPath, UserRepositoryFileName);
        return new Uri(userRepoPath);
    }

    public static string GetErrorLogPath(int iteration)
    {
        if (iteration < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(iteration), "Iteration must be greater than or equal to 0.");
        }

        if (iteration == 0)
        {
            return Path.Combine(AppDirectoryUri.LocalPath, $"{ErrorLogFileNameBase}.log");
        }
        
        var errorLogPath = Path.Combine(AppDirectoryUri.LocalPath, $"{ErrorLogFileNameBase}_{iteration}.log");
        return errorLogPath;
    }
}
