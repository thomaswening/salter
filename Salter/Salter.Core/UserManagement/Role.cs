using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

using Salter.Core.DataManagement;

namespace Salter.Core.UserManagement;

/// <summary>
/// Represents a collection of permissions that can be assigned to a user.
/// A user can be assigned the roles of <see cref="Admin"/> or <see cref="User"/>.
/// </summary>
public class Role : Entity
{
    public const string AdminRoleName = "Admin";
    public const string UserRoleName = "User";

    public static readonly Role Admin = new(AdminRoleName, Permission.Read | Permission.Write | Permission.Delete);
    public static readonly Role User = new(UserRoleName, Permission.Read | Permission.Write);

    private Role(string name, Permission permissions)
    {
        Name = name;
        Permissions = permissions;
    }

    public string Name { get; }
    public Permission Permissions { get; }

    public static Role GetRoleByName(string roleName)
    {
        return roleName switch
        {
            AdminRoleName => Admin,
            UserRoleName => User,
            _ => throw new ArgumentException("Invalid role name.", nameof(roleName))
        };
    }

    public static bool IsValidRoleName(string roleName)
    {
        return roleName == AdminRoleName || roleName == UserRoleName;
    }
}

[Flags]
public enum Permission
{
    None = 0,
    Read = 1 << 0,
    Write = 1 << 1,
    Delete = 1 << 2,
}