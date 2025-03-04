using Salter.Core.UserManagement;

namespace Salter.Persistence;

public class UserMapper : Mapper<User, UserDto>
{
    public override UserDto MapToDataTransferObject(User model)
    {
        return new UserDto
        {
            Id = model.Id,
            Username = model.Username,
            PasswordHash = model.PasswordHash,
            Salt = model.Salt,
            IsDefault = model.IsDefault,
            RoleName = model.Role.Name
        };
    }

    protected override User MapToModel(UserDto dto)
    {
        if (dto.IsDefault)
        {
            return User.CreateDefaultUser(dto.Id, dto.PasswordHash, dto.Salt);
        }

        return new User(dto.Id, dto.Username, dto.PasswordHash, dto.Salt, dto.Role);
    }
}