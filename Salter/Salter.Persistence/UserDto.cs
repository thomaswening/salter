using System.Text;
using System.Text.Json.Serialization;

using Salter.Core.UserManagement;

namespace Salter.Persistence;

internal class UserDto : DataTransferObject<User>
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public string RoleName { get; set; } = string.Empty;

    [JsonIgnore]
    public Role Role
    {
        get => Role.GetRoleByName(RoleName);
        set => RoleName = value.Name;
    }

    public override ValidationException? Validate()
    {
        const string IsRequiredMessage = " is required.";
        var msg = new StringBuilder();

        if (Id == Guid.Empty)
        {
            msg.AppendLine(nameof(Id) + IsRequiredMessage);
        }
        if (string.IsNullOrWhiteSpace(Username))
        {
            msg.AppendLine(nameof(Username) + IsRequiredMessage);
        }
        if (string.IsNullOrWhiteSpace(PasswordHash))
        {
            msg.AppendLine(nameof(PasswordHash) + IsRequiredMessage);
        }
        if (string.IsNullOrWhiteSpace(Salt))
        {
            msg.AppendLine(nameof(Salt) + IsRequiredMessage);
        }
        if (string.IsNullOrWhiteSpace(RoleName))
        {
            msg.AppendLine(nameof(RoleName) + IsRequiredMessage);
        }
        else if (!Role.IsValidRoleName(RoleName))
        {
            msg.AppendLine("Invalid " + nameof(RoleName) + ".");
        }

        if (IsDefault && Username != User.DefaultUsername)
        {
            msg.AppendLine($"Default user must have username '{User.DefaultUsername}'.");
        }
        if (!IsDefault && Username == User.DefaultUsername)
        {
            msg.AppendLine($"Non-default user cannot have username '{User.DefaultUsername}'.");
        }
        if (IsDefault && Role != Role.Admin)
        {
            msg.AppendLine($"Default user must have role '{Role.Admin.Name}'.");
        }

        if (msg.Length > 0)
        {
            return new ValidationException(msg.ToString());
        }

        return null;
    }
}