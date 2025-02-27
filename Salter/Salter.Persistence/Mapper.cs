using Salter.Core.DataManagement;

namespace Salter.Persistence;

/// <summary>
/// Maps between models and data transfer objects.
/// </summary>
/// <typeparam name="TModel">The type of the model.</typeparam>
/// <typeparam name="TDto">The type of the data transfer object.</typeparam>
internal abstract class Mapper<TModel, TDto>
    where TDto : IDataTransferObject<TModel>
    where TModel : Entity
{
    public abstract TDto MapToDataTransferObject(TModel model);

    public TModel ToModel(TDto dto)
    {
        var validationError = dto.Validate();
        if (validationError is not null)
        {
            throw validationError;
        }

        return MapToModel(dto);
    }

    internal List<TDto> MapToDataTransferObjects(IEnumerable<TModel> models)
    {
        return models.Select(MapToDataTransferObject).ToList();
    }

    internal List<TModel> MapToModels(IEnumerable<TDto> dtos)
    {
        return dtos.Select(ToModel).ToList();
    }

    protected abstract TModel MapToModel(TDto dto);
}