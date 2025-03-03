using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Salter.Core.UserManagement;
using Salter.Core;

namespace Salter.Cmd.Menus;

/// <summary>
/// This menu provides options for users to manage their accounts and other users.
/// It is only accessible to authenticated users.
/// The user must be an admin to view, create, delete, or make users admins.
/// The user must not be the default user to change their username or delete their account.
/// Any user can change their password.
/// </summary>
internal class UserActionsMenu(AuthenticationService authService, UserManager userManager) : Menu(authService)
{
    private const string InvalidUsernameMessage = "The username does not meet the requirements. Please try again.";

    public override string Title => "User Actions";

    private async Task ChangeCurrentUsernameAsync()
    {
        if (_authService.CurrentUser is null)
        {
            Console.WriteLine("\nYou must be logged in to change your username.");
            return;
        }

        if (_authService.CurrentUser.IsDefault)
        {
            Console.WriteLine("\nYou cannot change the default user's username.");
            return;
        }

        if (!ConsoleInputHelper.GetUserInput(
            "Enter a new username",
            _authService.ValidateUsername,
            out var newUsername,
            InvalidUsernameMessage))
        {
            return;
        }

        Console.WriteLine();

        if (!await AuthenticateCurrentUserAsync().ConfigureAwait(false))
        {
            Console.WriteLine("\nCould not authenticate user. Changing username failed.");
            return;
        }

        Console.WriteLine();

        var changedUser = new User(
            _authService.CurrentUser.Id,
            newUsername,
            _authService.CurrentUser.PasswordHash,
            _authService.CurrentUser.Salt,
            _authService.CurrentUser.Role);

        try
        {
            await userManager.UpdateUserAsync(changedUser).ConfigureAwait(false);
            await _authService.RefreshCurrentUserAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _canProceedToSubMenu = false;

            Console.WriteLine("Could not change username. Please try again.");
            Console.WriteLine(e.Message);
            Console.WriteLine();
            return;
        }

        Console.WriteLine("You have successfully changed your username.");
    }

    private async Task ChangeCurrentPasswordAsync()
    {
        if (_authService.CurrentUser is null)
        {
            Console.WriteLine("You must be logged in to change your password.");
            Console.WriteLine();
            return;
        }

        if (!await AuthenticateCurrentUserAsync().ConfigureAwait(false))
        {
            Console.WriteLine("Could not authenticate user. Changing username failed.");
            Console.WriteLine();
            return;
        }

        var newPassword = PromptForNewPassword();
        var hash = _authService.Hasher.GenerateHash(newPassword, out var salt);
        var changedUser = new User(
            _authService.CurrentUser.Id,
            _authService.CurrentUser.Username,
            hash,
            salt,
            _authService.CurrentUser.Role);

        try
        {
            await userManager.UpdateUserAsync(changedUser).ConfigureAwait(false);
        }
        catch (Exception)
        {
            Console.WriteLine("Could not save the new password. Please try again.");
            Console.WriteLine();
            return;
        }

        await _authService.RefreshCurrentUserAsync().ConfigureAwait(false);

        Console.WriteLine("You have successfully changed your password.");
    }

    private static char[] PromptForNewPassword()
    {
        char[] newPassword = Array.Empty<char>();

        while (true)
        {
            if (!ConsoleInputHelper.GetSecretUserInput(
                "Enter a new password",
                AuthenticationService.ValidatePassword,
                out newPassword,
                "Your password does not meet the requirements. Please try again."))
            {
                continue;
            }

            if (!ConsoleInputHelper.GetSecretUserInput("Please confirm your new password", out var confirmPassword))
            {
                continue;
            }

            if (newPassword.SequenceEqual(confirmPassword))
            {
                Array.Clear(confirmPassword, 0, confirmPassword.Length);
                break;
            }

            Console.WriteLine("Passwords do not match. Please try again.");
            Console.WriteLine();
        }

        return newPassword;
    }

    private async Task DeleteCurrentUserAsync()
    {
        Console.WriteLine();

        if (_authService.CurrentUser is null)
        {
            Console.WriteLine("You must be logged in to delete your account.");
            Console.WriteLine();
            return;
        }
        if (_authService.CurrentUser.IsDefault)
        {
            Console.WriteLine("You cannot delete the default user.");
            Console.WriteLine();
            return;
        }

        if (!await AuthenticateCurrentUserAsync().ConfigureAwait(false))
        {
            Console.WriteLine("Could not authenticate user. Deleting account failed.");
            Console.WriteLine();
            return;
        }

        if (!ConsoleInputHelper.GetUserConfirmation("Deleting your account cannot be undone. Do you want to proceed anyway?"))
        {
            Console.WriteLine("Account deletion cancelled.");
            Console.WriteLine();
            return;
        }

        try
        {
            await userManager.RemoveUserAsync(_authService.CurrentUser).ConfigureAwait(false);
            _authService.Logout();
        }
        catch (Exception)
        {
            Console.WriteLine("Could not delete your account. Please try again.");
            Console.WriteLine();
            return;
        }

        Console.WriteLine("You have successfully deleted your account.");
        NavigateToPreviousMenu();
    }

    private async Task ViewUsersAsync()
    {
        try
        {
            var users = await userManager.GetUsersAsync().ConfigureAwait(false);
            Console.WriteLine("-- Registered users --");
            Console.WriteLine();

            foreach (var user in users)
            {
                Console.WriteLine($"{user.Username} - {user.Role.Name}");
            }

            Console.WriteLine();
        }
        catch (Exception e)
        {
            _canProceedToSubMenu = false;

            Console.WriteLine("Could not retrieve users. Please try again.");
            Console.WriteLine(e.Message);
            Console.WriteLine();
            return;
        }
    }

    private async Task CreateUserAsync()
    {
        if (!ConsoleInputHelper.GetUserInput(
            "Username",
            _authService.ValidateUsername,
            out var username,
            "Please enter a username"))
        {
            return;
        }

        var password = PromptForNewPassword();

        try
        {
            if (!await AuthenticateCurrentUserAsync().ConfigureAwait(false))
            {
                Console.WriteLine("Could not authenticate user. Creating user failed.");
                Console.WriteLine();
                return;
            }

            await userManager.AddUserAsync(username, password).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _canProceedToSubMenu = false;

            Console.WriteLine("Could not create user. Please try again.");
            Console.WriteLine(e.Message);
            Console.WriteLine();
            return;
        }

        Console.WriteLine("You have successfully created a new user.");
    }

    private async Task DeleteUserAsync()
    {
        if (!ConsoleInputHelper.GetUserInput(
            "Username",
            u => !string.IsNullOrWhiteSpace(u),
            out var username,
            "Please enter the username of the user you want to delete"))
        {
            return;
        }

        if (username.Equals(User.DefaultUsername, StringComparison.InvariantCulture))
        {
            Console.WriteLine("You cannot delete the default user.");
            Console.WriteLine();
            return;
        }

        var user = userManager.GetUserByUsername(username);
        if (user is null)
        {
            Console.WriteLine("User not found.");
            Console.WriteLine();
            return;
        }

        Console.WriteLine();

        try
        {
            if (!await AuthenticateCurrentUserAsync().ConfigureAwait(false))
            {
                Console.WriteLine("Could not authenticate user. Deleting user failed.");
                Console.WriteLine();
                return;
            }

            await userManager.RemoveUserAsync(user).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _canProceedToSubMenu = false;

            Console.WriteLine("Could not delete user. Please try again.");
            Console.WriteLine(e.Message);
            Console.WriteLine();

            return;
        }

        Console.WriteLine("You have successfully deleted the user.");
    }

    private async Task MakeUserAdminAsync()
    {
        if (!ConsoleInputHelper.GetUserInput(
            "Username",
            u => !string.IsNullOrWhiteSpace(u),
            out var username,
            "Please enter the username of the user you want to make an admin"))
        {
            return;
        }

        Console.WriteLine();

        var user = userManager.GetUserByUsername(username);
        if (user is null)
        {
            Console.WriteLine("User not found.");
            Console.WriteLine();
            return;
        }

        try
        {
            if (!await AuthenticateCurrentUserAsync().ConfigureAwait(false))
            {
                Console.WriteLine("Could not authenticate user. Making user an admin failed.");
                Console.WriteLine();
                return;
            }

            var changedUser = new User(
                user.Id,
                user.Username,
                user.PasswordHash,
                user.Salt,
                Role.Admin);

            await userManager.UpdateUserAsync(changedUser).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _canProceedToSubMenu = false;

            Console.WriteLine("Could not make user admin. Please try again.");
            Console.WriteLine(e.Message);
            Console.WriteLine();
            return;
        }

        Console.WriteLine("You have successfully made the user an admin.");
    }

    private void Logout()
    {
        _authService.Logout();
        Console.WriteLine("You have successfully logged out.");
        NavigateToPreviousMenu();
    }

    private async Task<bool> AuthenticateCurrentUserAsync()
    {
        if (_authService.CurrentUser is null)
        {
            throw new InvalidOperationException("Cannot authenticate current user because no user is currently logged in.");
        }

        Console.WriteLine("Please enter your current password to proceed.");

        var isAuthenticated = false;

        while (!isAuthenticated)
        {
            if (!ConsoleInputHelper.GetSecretUserInput(
                "Password",
                p => p.Length > 0,
                out var password,
                "Please enter your password"))
            {
                return false;
            }

            isAuthenticated = await _authService.AuthenticateCurrentUserAsync(password).ConfigureAwait(false);
            if (!isAuthenticated)
            {
                Console.WriteLine("Invalid password. Please try again.");
            }
        }

        Console.WriteLine();
        return isAuthenticated;
    }

    protected override List<MenuItem> CreateMenuItems()
    {
        return
        [
            new("Change My Username", ChangeCurrentUsernameAsync, u => !u.IsDefault),
            new("Change My Password", ChangeCurrentPasswordAsync),
            new("Delete My Account", DeleteCurrentUserAsync, u => !u.IsDefault),
            new("View Users", ViewUsersAsync, Role.Admin),
            new("Create User", CreateUserAsync, Role.Admin),
            new("Delete User", DeleteUserAsync, Role.Admin),
            new("Make User Admin", MakeUserAdminAsync, Role.Admin),
            new("Logout", Logout)
        ];
    }
    protected override void OnExit() => _authService.Logout();
    protected override void OnGoBack() => _authService.Logout();
}
