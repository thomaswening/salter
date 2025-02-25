using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Salter.Core.DataManagement;

public class Entity
{
    public Guid Id { get; protected init; } = Guid.NewGuid();

    public override bool Equals(object? obj)
    {
        return obj is Entity user && Id.Equals(user.Id);
    }

    public override int GetHashCode() => Id.GetHashCode();
}
