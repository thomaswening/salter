using System.Text.RegularExpressions;

namespace Salter.Core.UserManagement;

public partial class AuthenticationService(UserManager userManager, PasswordHasher passwordHasher)
{
    private readonly UserManager userManager = userManager;
    public PasswordHasher Hasher => passwordHasher;

    public User CurrentUser { get; private set; } = User.NoUser;

    public async Task<bool> AuthenticateAsync(string username, char[] password)
    {
        try
        {
            var users = await userManager.GetUsersAsync();
            var user = users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.InvariantCulture));

            if (user is null)
            {
                return false;
            }

            var isAuthenticated = Hasher.Validate(password, user.PasswordHash, user.Salt);

            if (isAuthenticated)
            {
                CurrentUser = user;
            }

            return isAuthenticated;
        }
        catch (Exception e)
        {
            throw new AuthenticationServiceException("Could not authenticate user.", e);
        }
        finally
        {
            Array.Clear(password, 0, password.Length);
        }
    }

    public async Task<bool> AuthenticateCurrentUserAsync(char[] password)
    {
        if (CurrentUser == User.NoUser)
        {
            throw new AuthenticationServiceException("Current user is null.");
        }

        try
        {
            return await AuthenticateAsync(CurrentUser.Username, password).ConfigureAwait(false);
        }
        finally
        {
            Array.Clear(password, 0, password.Length);
        }
    }

    public async Task RegisterAsync(string username, char[] password)
    {
        try
        {
            if (!ValidateUsername(username, out var usernameError))
            {
                throw new UsernameException(usernameError);
            }

            if (!ValidatePassword(password, out var passwordError))
            {
                throw new InvalidPasswordException(passwordError);
            }

            await userManager.AddUserAsync(username, password);
        }
        finally
        {
            Array.Clear(password, 0, password.Length);
        }
    }

    public async Task RefreshCurrentUserAsync()
    {
        if (CurrentUser == User.NoUser)
        {
            throw new NoAuthenticatedUserException();
        }

        var users = await userManager.GetUsersAsync();
        CurrentUser = users.FirstOrDefault(u => u.Equals(CurrentUser)) ?? User.NoUser;
    }

    public static bool ValidatePassword(char[] password, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (password.Length < 8)
        {
            errorMessage = "Password must be at least 8 characters long.";
            return false;
        }

        if (!PasswordRegex().IsMatch(password))
        {
            errorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.";
            return false;
        }

        return true;
    }

    public void Logout() => CurrentUser = User.NoUser;

    public bool ValidateUsername(string username, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (username.Length < 8 || username.Length > 20)
        {
            errorMessage = "Username must be between 8 and 20 characters long.";
            return false;
        }

        if (!UsernameRegex().IsMatch(username))
        {
            errorMessage = "Username must contain only letters, numbers, underscores, and dashes.";
            return false;
        }

        if (!char.IsLetter(username[0]))
        {
            errorMessage = "Username must start with a letter.";
            return false;
        }

        if (userManager.GetUserByUsername(username) is not null)
        {
            errorMessage = "Username already exists.";
            return false;
        }

        return true;
    }

    [GeneratedRegex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$")]
    private static partial Regex PasswordRegex();

    [GeneratedRegex(@"^[a-zA-Z0-9_-]+$")]
    private static partial Regex UsernameRegex();
}

public class AuthenticationServiceException(string message, Exception? innerException = null) : Exception(message, innerException)
{
}

public class InvalidPasswordException(string message) : AuthenticationServiceException(message)
{
}

public class UsernameException(string message) : AuthenticationServiceException(message)
{
}

public class NoAuthenticatedUserException() : AuthenticationServiceException("No user is currently authenticated.")
{
}