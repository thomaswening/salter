using Salter.Core.UserManagement;
using Salter.Core.Encryption;
using Salter.Persistence;
using Salter.Encryption;
using System.Security.Cryptography;
using Salter.Core;
using Salter.Cmd.Menus;

namespace Salter.Cmd;

internal class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            JsonRepository<User, UserDto> repository = InitializeRepository();
            var passwordHasher = new PasswordHasher();
            var userManager = new UserManager(repository, passwordHasher);
            await userManager.InitializeAsync();

            var authService = new AuthenticationService(userManager, passwordHasher);
            var userActionsMenu = new UserActionsMenu(authService, userManager);
            var mainMenu = new AuthenticationMenu(authService, userActionsMenu);
            var menuNavigator = new MenuNavigator(mainMenu);
            var menuDisplay = new MenuDisplay(menuNavigator);

            menuDisplay.Start();
        }
        catch (OperationCanceledException)
        {
            // Input cancellation, just exit
        }
        catch (Exception e)
        {
            Console.WriteLine();
            Console.WriteLine("A critical error has occurred and the application will exit now.");
            Console.WriteLine();
            Console.WriteLine("== Details ==");
            Console.WriteLine();
            Console.WriteLine(ExceptionHelper.UnpackException(e));

            SaveErrorLog(e);
        }

        ConsoleInputHelper.PromptContinue();
    }

    private static JsonRepository<User, UserDto> InitializeRepository()
    {
        var keyManOptions = new KeyManager.Options()
        {
            SourceType = SecretManager.SourceType.Environment,
            KeySource = AppConstants.KeyEnvironmentVariable,
            InitializationVectorSource = AppConstants.IvEnvironmentVariable
        };

        var keyManager = new KeyManager(keyManOptions);
        var encryptor = new KeyIvEncryptor(keyManager, Aes.Create);
        var mapper = new UserMapper();
        var repository = new JsonRepository<User, UserDto>(AppConstants.UserRepositoryUri, encryptor, mapper);
        return repository;
    }

    private static void SaveErrorLog(Exception e)
    {
        // Get next iteration suffix for error log file
        var iteration = 0;
        while (File.Exists(AppConstants.GetErrorLogPath(iteration)))
        {
            iteration++;
        }

        var errorLogPath = AppConstants.GetErrorLogPath(iteration);
        File.WriteAllText(errorLogPath, ExceptionHelper.UnpackException(e));
    }
}
