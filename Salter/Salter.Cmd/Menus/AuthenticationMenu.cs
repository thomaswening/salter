﻿using Salter.Core.UserManagement;

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
            ConsoleInputHelper.PromptContinue();
            return;
        }

        if (!ConsoleInputHelper.GetSecretUserInput(
            "Password",
            AuthenticationService.ValidatePassword,
            out var password,
            "Please enter your password"))
        {
            ConsoleInputHelper.PromptContinue();
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
                ConsoleInputHelper.PromptContinue();
                return;
            }

            if (!ConsoleInputHelper.GetSecretUserInput(
                "Password",
                p => p.Length > 0,
                out var password,
                "Please enter your password"))
            {
                ConsoleInputHelper.PromptContinue();
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

        Console.WriteLine("Please login to continue.");
        Console.WriteLine();

        await AuthenticateUserAsync().ConfigureAwait(false);
    }

    protected override List<MenuItem> CreateMenuItems()
    {
        return
        [
            new("Authenticate", AuthenticateUserAsync, subMenu: _userActionsMenu),
            new("Register", RegisterAndAuthenticateUserAsync, subMenu: _userActionsMenu),
        ];
    }

    protected override void OnExit() => _authService.Logout();
    protected override void OnGoBack() => _authService.Logout();
}
