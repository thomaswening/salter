using System.Text.RegularExpressions;

namespace Salter.Core.UserManagement;

internal partial class AuthenticationService(UserManager userManager, PasswordHasher passwordHasher)
{
    private const string NoCurrentUserMessage = "No user is currently authenticated.";
    private readonly UserManager userManager = userManager;
    private readonly PasswordHasher passwordHasher = passwordHasher;

    public User? CurrentUser { get; private set; }

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

            var isAuthenticated = passwordHasher.Validate(password, user.PasswordHash, user.Salt);

            if (isAuthenticated)
            {
                CurrentUser = user;
            }

            return isAuthenticated;
        }
        finally
        {
            Array.Clear(password, 0, password.Length);
        }
    }

    public async Task<bool> AuthenticateCurrentUserAsync(char[] password)
    {
        if (CurrentUser is null)
        {
            throw new InvalidOperationException(NoCurrentUserMessage);
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
            ValidateUsername(username);
            ValidatePassword(password);
            await userManager.AddUserAsync(username, password);
        }
        finally
        {
            Array.Clear(password, 0, password.Length);
        }
    }

    public static void ValidatePassword(char[] password)
    {
        if (password.Length < 8)
        {
            throw new InvalidPasswordException("Password must be at least 8 characters long.");
        }

        // at least one uppercase letter, one lowercase letter, one number, special character
        if (!PasswordRegex().IsMatch(password))
        {
            throw new InvalidPasswordException("Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.");
        }
    }

    public void ValidateUsername(string username)
    {
        if (username.Length < 8 || username.Length > 20)
        {
            throw new InvalidUsernameException("Username must be between 8 and 20 characters long.");
        }

        if (!UsernameRegex().IsMatch(username))
        {
            throw new InvalidUsernameException("Username must contain only letters, numbers, underscores, and dashes.");
        }

        if (!char.IsLetter(username[0]))
        {
            throw new InvalidUsernameException("Username must start with a letter.");
        }

        if (userManager.GetUserByUsername(username) is not null)
        {
            throw new InvalidUsernameException("Username already exists.");
        }
    }

    [GeneratedRegex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$")]
    private static partial Regex PasswordRegex();

    [GeneratedRegex(@"^[a-zA-Z0-9_-]+$")]
    private static partial Regex UsernameRegex();
}

public class InvalidPasswordException(string message) : Exception(message)
{
}

public class InvalidUsernameException(string message) : Exception(message)
{
}
