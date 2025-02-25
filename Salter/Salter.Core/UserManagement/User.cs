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
    public const string DefaultPasswordHash = "9CCE8D59D0399FAA65DFE6731ADFBE829FBC2CDAABFD73C0243A8140B12CFB44B7355111A3B10D125E16372A8E269CC4A1391311B571E3A299F577A72E175670";
    public const string DefaultSalt = "3B298D0D4352846470303ED1963E2F4607A5D0BE3AB3ED02F8250E70FC70770CE13756F6FEAA5E15C51C3F2E45154616BA07AFDC6AA9C10019A7FB25AF87C72A";

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
