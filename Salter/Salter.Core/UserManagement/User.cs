using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public const string DefaultPasswordHash = "0E443E662DB39E6780D3CD335AD8E93FD756BAEB667137281319D367DE723038951090B77CD6D8DB7361033B0481EBDA5FDD4CFDD1D69EC83EBE9525DBECB2FF";
    public const string DefaultSalt = "909629B522964BCB0A76BB53E6D183C749225E54EF785BD39300C7A912E47C4821A85294A5455372B75EEA2B9FC0662735250CCDFAC1B4F510828C0E3AC95706";

    public static readonly User DefaultUser = CreateDefaultUser();

    public string Username { get; }
    public string PasswordHash { get; }
    public string Salt { get; }
    public bool IsDefault { get; init; }
    public Role Role { get; }

    private User(Guid id, string username, string passwordHash, string salt, Role role, bool isDefault = false)
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

    public User(string username, string passwordHash, string salt, Role? role = null)
        : this(Guid.NewGuid(), username, passwordHash, salt, role ?? Role.User)
    {
    }

    private static User CreateDefaultUser()
    {
        return new User(Guid.NewGuid(), DefaultUsername, DefaultPasswordHash, DefaultSalt, Role.Admin, true);
    }

    /// <summary>
    /// Creates a copy of the user with the new role.
    /// </summary>
    /// <param name="newRole"></param>
    /// <returns></returns>
    public User WithRole(Role newRole)
    {
        return new User(Id, Username, PasswordHash, Salt, newRole, IsDefault);
    }

    public bool HasRole(Role role) => Role.Equals(role);
}
