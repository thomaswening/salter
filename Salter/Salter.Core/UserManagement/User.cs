using Salter.Core.DataManagement;

namespace Salter.Core.UserManagement;

/// <summary>
/// Represents a user account in the system with a unique username and password.
/// A user is assigned a role of <see cref="Role"/> that determines the permissions they have.
/// There is a default user account with the username "default". This account is assigned the role of <see cref="Role.Admin"/> and is the only account that can access the system when no other accounts are available.
/// It is also the initial account created when the system is first set up.
/// </summary>
public class User : Entity
{
    public const string DefaultUsername = "default";
    private const string DefaultPasswordHash = "0E443E662DB39E6780D3CD335AD8E93FD756BAEB667137281319D367DE723038951090B77CD6D8DB7361033B0481EBDA5FDD4CFDD1D69EC83EBE9525DBECB2FF";
    private const string DefaultSalt = "909629B522964BCB0A76BB53E6D183C749225E54EF785BD39300C7A912E47C4821A85294A5455372B75EEA2B9FC0662735250CCDFAC1B4F510828C0E3AC95706";

    /// <summary>
    /// Meant for creating a new user account that is not persisted in the database.
    /// If no role is provided, the user is assigned the role of <see cref="Role.User"/>.
    /// </summary>
    public User(string username, string passwordHash, string salt, Role? role = null)
        : this(Guid.NewGuid(), username, passwordHash, salt, role ?? Role.User, false) { }

    /// <summary>
    /// Meant for creating a user object from one that is persisted in the database.
    /// </summary>
    public User(Guid id, string username, string passwordHash, string salt, Role role)
        : this(id, username, passwordHash, salt, role, false) { }

    private User(Guid id, string username, string passwordHash, string salt, Role role, bool isDefault)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username, nameof(username));
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash, nameof(passwordHash));
        ArgumentException.ThrowIfNullOrWhiteSpace(salt, nameof(salt));

        Id = id;
        Username = username;
        PasswordHash = passwordHash;
        Salt = salt;
        Role = role;
        IsDefault = isDefault;
    }

    public bool IsDefault { get; init; }
    public string PasswordHash { get; }
    public Role Role { get; }
    public string Salt { get; }
    public string Username { get; }

    /// <summary>
    /// This method is meant to create a user object for the default user account from the database.
    /// This account is assigned the role of <see cref="Role.Admin"/> and the username <see cref="DefaultUsername"/>.
    /// </summary>
    public static User CreateDefaultUser(Guid id, string passwordHash, string salt)
    {
        return new User(id, DefaultUsername, passwordHash, salt, Role.Admin, true);
    }

    /// <summary>
    /// This method is meant to create a user object for the default user account from scratch.
    /// This account is assigned the role of <see cref="Role.Admin"/> and the username <see cref="DefaultUsername"/>,
    /// as well as the default password hash and salt.
    /// </summary>
    public static User CreateDefaultUser()
    {
        return CreateDefaultUser(Guid.NewGuid(), DefaultPasswordHash, DefaultSalt);
    }

    public bool HasRole(Role role) => Role.Equals(role);

    /// <summary>
    /// Creates a copy of the user with the new role.
    /// </summary>
    /// <param name="newRole"></param>
    /// <returns></returns>
    public User WithRole(Role newRole)
    {
        return new User(Id, Username, PasswordHash, Salt, newRole, IsDefault);
    }
}
