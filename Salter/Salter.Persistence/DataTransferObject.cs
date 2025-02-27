using Salter.Core.DataManagement;

namespace Salter.Persistence;

internal interface IDataTransferObject<T> where T : Entity
{
    ValidationException? Validate();
}

internal abstract class DataTransferObject<T> : IDataTransferObject<T> where T : Entity
{
    public abstract ValidationException? Validate();
}

public class ValidationException(string message) : Exception(message)
{
}