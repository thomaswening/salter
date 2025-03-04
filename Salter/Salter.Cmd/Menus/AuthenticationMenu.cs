using Salter.Core.UserManagement;

namespace Salter.Cmd.Menus;

/// <summary>
/// This menu allows the user to authenticate or register.
/// </summary>
internal class AuthenticationMenu : Menu
{
    private readonly UserActionsMenu _userActionsMenu;
    public override string Title => "Authentication";

    public AuthenticationMenu(AuthenticationService authenticationService, UserActionsMenu userActionsMenu) : base(authenticationService)
    {
        _userActionsMenu = userActionsMenu;
        HasGoBack = false;
    }

    private async Task RegisterNewUserAsync()
    {
        if (!ConsoleInputHelper.GetUserInput(
            "Username",
            _authService.ValidateUsername,
            out var username,
            "Please enter your username"))
        {
            _canProceedToSubMenu = false;
            return;
        }

        if (!ConsoleInputHelper.GetSecretUserInput(
            "Password",
            AuthenticationService.ValidatePassword,
            out var password,
            "Please enter your password"))
        {
            _canProceedToSubMenu = false;
            return;
        }

        Console.WriteLine();

        try
        {
            await _authService.RegisterAsync(username, password).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _canProceedToSubMenu = false;

            Console.WriteLine("Could not register user. Please try again.");
            Console.WriteLine(e.Message);
            return;
        }

        Console.WriteLine("You have successfully registered!");
        ConsoleInputHelper.PromptContinue();
    }

    private async Task AuthenticateUserAsync()
    {
        Console.WriteLine("Please enter your username and password to login.");

        var isAuthenticated = false;

        while (!isAuthenticated)
        {
            if (!ConsoleInputHelper.GetUserInput(
                "Username",
                p => !string.IsNullOrWhiteSpace(p),
                out var username,
                "Please enter your username"))
            {
                _canProceedToSubMenu = false;
                return;
            }

            if (!ConsoleInputHelper.GetSecretUserInput(
                "Password",
                p => p.Length > 0,
                out var password,
                "Please enter your password"))
            {
                _canProceedToSubMenu = false;
                return;
            }

            Console.WriteLine();

            try
            {
                isAuthenticated = await _authService.AuthenticateAsync(username, password).ConfigureAwait(false);

                if (!isAuthenticated)
                {
                    Console.WriteLine("Invalid credentials. Please try again.");
                    Console.WriteLine();
                }
            }
            catch (Exception e)
            {
                _canProceedToSubMenu = false;

                Console.WriteLine("Could not authenticate user. Please try again.");
                Console.WriteLine(ExceptionHelper.UnpackException(e));
                return;
            }
        }

        Console.WriteLine("You have successfully logged in!");
        ConsoleInputHelper.PromptContinue();
    }

    private async Task RegisterAndAuthenticateUserAsync()
    {
        await RegisterNewUserAsync().ConfigureAwait(false);

        if (!_canProceedToSubMenu)
        {
            return;
        }

        Console.WriteLine("Please login to continue.");
        Console.WriteLine();

        await AuthenticateUserAsync().ConfigureAwait(false);
    }

    private void CreatePasswordHash()
    {
        if (!ConsoleInputHelper.GetSecretUserInput("Password", out var password))
        {
            return;
        }

        Console.WriteLine();

        var hash = _authService.Hasher.GenerateHash(password, out var salt);
        Array.Clear(password, 0, password.Length);

        Console.WriteLine($"Password hash: {hash}");
        Console.WriteLine($"Salt:          {salt}");
        Console.WriteLine();
    }

    protected override List<MenuItem> CreateMenuItems()
    {
        return
        [
            new("Authenticate", AuthenticateUserAsync, subMenu: _userActionsMenu),
            new("Register", RegisterAndAuthenticateUserAsync, subMenu: _userActionsMenu),
            new("Create password hash", CreatePasswordHash),
        ];
    }

    protected override void OnExit() => _authService.Logout();
    protected override void OnGoBack() => _authService.Logout();
}
