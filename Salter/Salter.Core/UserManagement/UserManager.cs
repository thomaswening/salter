using Salter.Core.DataManagement;

namespace Salter.Core.UserManagement;

/// <summary>
/// Manages the creation, retrieval, updating, and deletion of user accounts.
/// </summary>
public class UserManager(IRepository<User> repository, PasswordHasher passwordHasher)
{
    private const string UserDoesNotExistMessage = "User does not exist";
    private const string UserAlreadyExistsMessage = "User already exists";

    private readonly IRepository<User> repo = repository;
    private readonly PasswordHasher passwordHasher = passwordHasher;

    public async Task AddUserAsync(string username, char[] password)
    {
        try
        {
            if (repo.Cache.Any(u => u.Username.Equals(username, StringComparison.InvariantCulture)))
            {
                throw new UserAlreadyExistsException(UserAlreadyExistsMessage, username);
            }

            var user = CreateUser(username, password);
            await repo.AddRecordAsync(user).ConfigureAwait(false);
        }
        finally
        {
            Array.Clear(password, 0, password.Length);
        }
    }

    public async Task<List<User>> GetUsersAsync()
    {
        return await repo.GetRecordsAsync().ConfigureAwait(false);
    }

    public async Task RemoveUserAsync(string username)
    {
        var user = GetUserByUsername(username)
            ?? throw new UserNotFoundException(UserDoesNotExistMessage, username);

        await repo.RemoveRecordAsync(user).ConfigureAwait(false);
    }

    public async Task RemoveUserAsync(User user)
    {
        if (GetUser(user) is null)
        {
            throw new UserNotFoundException(UserDoesNotExistMessage, user.Username);
        }

        await repo.RemoveRecordAsync(user).ConfigureAwait(false);
    }

    public async Task UpdateUserAsync(User newUser)
    {
        if (GetUser(newUser) is null)
        {
            throw new UserNotFoundException(UserDoesNotExistMessage, newUser.Username);
        }

        await repo.UpdateRecordAsync(newUser).ConfigureAwait(false);
    }

    private User CreateUser(string username, char[] password)
    {
        try
        {
            var passwordHash = passwordHasher.GenerateHash(password, out var salt);
            return new User(username, passwordHash, salt);
        }
        finally
        {
            Array.Clear(password, 0, password.Length);
        }
    }

    public User? GetUserByUsername(string username)
    {
        return repo.Cache.FirstOrDefault(u => u.Username.Equals(username, StringComparison.InvariantCulture));
    }

    public User? GetUser(User user)
    {
        return repo.Cache.FirstOrDefault(u => u.Equals(user));
    }

    public async Task ResetToDefaultAsync()
    {
        await repo.ClearAllRecordsAsync().ConfigureAwait(false);
        await repo.AddRecordAsync(User.CreateDefaultUser()).ConfigureAwait(false);
    }

    public async Task InitializeAsync()
    {
        try
        {
            await repo.InitializeAsync().ConfigureAwait(false);
            await EnsureDefaultUserExistsAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            throw new UserManagerInitializationException("The user management could not be initialized.", e);
        }
    }

    private async Task EnsureDefaultUserExistsAsync()
    {
        var defaultUsers = repo.Cache.Where(users => users.IsDefault).ToList();
        if (defaultUsers.Count == 0)
        {
            var newDefaultUser = User.CreateDefaultUser();
            defaultUsers.Add(newDefaultUser);
            await repo.AddRecordAsync(newDefaultUser).ConfigureAwait(false);
        }
        else if (defaultUsers.Count > 1)
        {
            var defaultUsernames = string.Join(", ", defaultUsers.Select(u => u.Username));
            throw new InvalidOperationException($"Multiple default users exist: {defaultUsernames}");
        }

        // Ensure the default user has the admin role
        var defaultUser = defaultUsers[0];
        if (!defaultUser.HasRole(Role.Admin))
        {
            var newDefaultUser = defaultUser.WithRole(Role.Admin);
            await repo.UpdateRecordAsync(newDefaultUser).ConfigureAwait(false);
        }
    }
}

public class UserAlreadyExistsException(string message, string username) : Exception(message)
{
    public string Username { get; } = username;
}

public class UserNotFoundException(string message, string username) : Exception(message)
{
    public string Username { get; } = username;
}

public class UserManagerInitializationException(string message, Exception innerException) : Exception(message, innerException)
{
}
